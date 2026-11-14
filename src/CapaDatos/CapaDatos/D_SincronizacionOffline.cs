using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace CapaDatos
{
    /// <summary>
    /// Maintains the per-user local snapshot and synchronization state.
    /// Account data in this snapshot is always scoped by IdUsuario.
    /// </summary>
    public sealed class D_SincronizacionOffline
    {
        private static volatile bool modoOffline;

        public static bool ModoOffline { get { return modoOffline; } }

        public static void ActivarModoOffline(int idUsuario)
        {
            if (idUsuario <= 0) throw new ArgumentException("El usuario no es válido.");
            if (!TieneCopiaCompleta(idUsuario))
                throw new InvalidOperationException("No hay una copia local completa para esta cuenta.");
            modoOffline = true;
        }

        public static void ActivarModoOnline() { modoOffline = false; }

        public static T ConFallback<T>(int idUsuario, Func<T> principal, Func<T> local)
        {
            if (modoOffline) return local();
            try { return principal(); }
            catch (SqlException ex)
            {
                if (!D_Comun.EsFalloDeConexion(ex)) throw;
                ActivarModoOffline(idUsuario);
                return local();
            }
        }

        public static void ConFallback(int idUsuario, Action principal, Action local)
        {
            ConFallback<object>(idUsuario, () => { principal(); return null; }, () => { local(); return null; });
        }

        public static T ConFallbackEscritura<T>(int idUsuario, Func<T> principal, Func<T> local)
        {
            if (modoOffline) return SQLiteLocal.EnTransaccion(local);
            try
            {
                T resultado = principal();
                try { CapturarDesdeCache(idUsuario); }
                catch (SqlException) { }
                catch (InvalidOperationException) { }
                return resultado;
            }
            catch (SqlException ex)
            {
                if (!D_Comun.EsFalloDeConexion(ex)) throw;
                ActivarModoOffline(idUsuario);
                return SQLiteLocal.EnTransaccion(local);
            }
        }

        public static void ConFallbackEscritura(int idUsuario, Action principal, Action local)
        {
            ConFallbackEscritura<object>(idUsuario, () => { principal(); return null; }, () => { local(); return null; });
        }

        private static void CapturarDesdeCache(int idUsuario)
        {
            var usuarios = SQLiteLocal.Consultar("SELECT Correo,Nombre FROM UsuariosLocales WHERE IdUsuario=? AND SyncDeleted=0",
                f => new { Correo = f.Texto("Correo"), Nombre = f.Texto("Nombre") }, idUsuario);
            if (usuarios.Count == 0) return;
            new D_SincronizacionOffline().CapturarCopiaCompleta(idUsuario, usuarios[0].Correo, usuarios[0].Nombre);
        }

        public static int InsertarTarea(CapaEntidades.E_Tarea tarea)
        {
            string syncId = String.IsNullOrEmpty(tarea.SyncId) ? Guid.NewGuid().ToString("D") : tarea.SyncId;
            var existente = SQLiteLocal.Consultar("SELECT IdTarea FROM Tareas WHERE IdUsuario=? AND SyncId=? AND SyncDeleted=0",
                f => f.Entero("IdTarea"), tarea.IdUsuario, syncId);
            if (existente.Count != 0)
            {
                tarea.IdTarea = existente[0];
                tarea.SyncId = syncId;
                return existente[0];
            }
            if (tarea.EsPlantilla)
            {
                var plantillasAnteriores = SQLiteLocal.Consultar(
                    "SELECT IdTarea,SyncId FROM Tareas WHERE IdUsuario=? AND EsPlantilla=1 AND SyncDeleted=0",
                    f => new { Id = f.Entero("IdTarea"), SyncId = f.Texto("SyncId") }, tarea.IdUsuario);
                foreach (var plantilla in plantillasAnteriores)
                {
                    SQLiteLocal.Ejecutar("UPDATE Tareas SET SyncDeleted=1,SyncUpdatedUtc=? WHERE IdTarea=? AND IdUsuario=?",
                        DateTime.UtcNow, plantilla.Id, tarea.IdUsuario);
                    Encolar(tarea.IdUsuario, "Tarea", plantilla.SyncId, "D", plantilla.Id);
                }
            }
            object siguiente = SQLiteLocal.Escalar("SELECT COALESCE(MIN(IdTarea),0)-1 FROM Tareas");
            int id = Convert.ToInt32(siguiente);
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar(@"INSERT INTO Tareas
                (IdTarea,IdUsuario,Nombre,FechaInicio,FechaFin,Estado,EsPlantilla,NoBloquearApps,NoBloquearWebs,SyncId,SyncUpdatedUtc,SyncDeleted)
                VALUES(?,?,?,?,?,'P',?,?,?,?,?,0)", id, tarea.IdUsuario, tarea.Nombre,
                tarea.FechaInicio, tarea.FechaFin, tarea.EsPlantilla, tarea.NoBloquearApps,
                tarea.NoBloquearWebs, syncId, ahora);
            tarea.IdTarea = id;
            tarea.SyncId = syncId;
            Encolar(tarea.IdUsuario, "Tarea", syncId, "I",
                tarea.Nombre, tarea.FechaInicio, tarea.FechaFin, 'P', tarea.EsPlantilla,
                tarea.NoBloquearApps, tarea.NoBloquearWebs);
            return id;
        }

        public static List<CapaEntidades.E_Tarea> BuscarTareas(int idUsuario, string buscar)
        {
            return SQLiteLocal.Consultar(@"SELECT IdTarea,Nombre,FechaInicio,FechaFin,Estado,EsPlantilla,
                NoBloquearApps,NoBloquearWebs,SyncId FROM Tareas
                WHERE IdUsuario=? AND EsPlantilla=0 AND SyncDeleted=0 AND Nombre LIKE '%' || ? || '%' ORDER BY FechaInicio",
                f => new CapaEntidades.E_Tarea
                {
                    IdTarea = f.Entero("IdTarea"), IdUsuario = idUsuario, Nombre = f.Texto("Nombre"),
                    FechaInicio = f.Fecha("FechaInicio"), FechaFin = f.Fecha("FechaFin"),
                    Estado = String.IsNullOrEmpty(f.Texto("Estado")) ? 'P' : f.Texto("Estado")[0],
                    EsPlantilla = f.Booleano("EsPlantilla"), NoBloquearApps = f.Booleano("NoBloquearApps"),
                    NoBloquearWebs = f.Booleano("NoBloquearWebs"), SyncId = f.Texto("SyncId")
                }, idUsuario, buscar ?? String.Empty);
        }

        public static void ModificarTarea(CapaEntidades.E_Tarea tarea)
        {
            var actual = SQLiteLocal.Consultar("SELECT Estado,EsPlantilla,SyncId FROM Tareas WHERE IdTarea=? AND IdUsuario=? AND SyncDeleted=0",
                f => new { Estado = f.Texto("Estado"), EsPlantilla = f.Booleano("EsPlantilla"), SyncId = f.Texto("SyncId") }, tarea.IdTarea, tarea.IdUsuario);
            if (actual.Count == 0) throw new ArgumentException("La tarea no existe o no pertenece al usuario.");
            if (actual[0].EsPlantilla) throw new ArgumentException("La plantilla no se puede editar como una tarea normal.");
            if (actual[0].Estado == "A" || actual[0].Estado == "X" || actual[0].Estado == "C")
                throw new ArgumentException("Una tarea activa, completada o detenida no se puede editar.");
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar(@"UPDATE Tareas SET Nombre=?,FechaInicio=?,FechaFin=?,NoBloquearApps=?,NoBloquearWebs=?,SyncUpdatedUtc=?
                WHERE IdTarea=? AND IdUsuario=? AND SyncDeleted=0", tarea.Nombre, tarea.FechaInicio, tarea.FechaFin,
                tarea.NoBloquearApps, tarea.NoBloquearWebs, ahora, tarea.IdTarea, tarea.IdUsuario);
            tarea.SyncId = actual[0].SyncId;
            Encolar(tarea.IdUsuario, "Tarea", tarea.SyncId, "U", tarea.Nombre, tarea.FechaInicio,
                tarea.FechaFin, actual[0].Estado, tarea.NoBloquearApps, tarea.NoBloquearWebs);
        }

        public static void EliminarTarea(int idUsuario, int idTarea)
        {
            var filas = SQLiteLocal.Consultar("SELECT SyncId,Estado,EsPlantilla FROM Tareas WHERE IdTarea=? AND IdUsuario=? AND SyncDeleted=0",
                f => new { SyncId = f.Texto("SyncId"), Estado = f.Texto("Estado"), EsPlantilla = f.Booleano("EsPlantilla") }, idTarea, idUsuario);
            if (filas.Count == 0) throw new ArgumentException("La tarea no existe o no pertenece al usuario.");
            if (filas[0].EsPlantilla) throw new ArgumentException("La plantilla no se puede eliminar como una tarea normal.");
            if (filas[0].Estado == "A" || filas[0].Estado == "X" || filas[0].Estado == "C")
                throw new ArgumentException("Una tarea activa, completada o detenida no se puede eliminar.");
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar("UPDATE Tareas SET SyncDeleted=1,SyncUpdatedUtc=? WHERE IdTarea=? AND IdUsuario=?", ahora, idTarea, idUsuario);
            SQLiteLocal.Ejecutar("UPDATE Descansos SET SyncDeleted=1,SyncUpdatedUtc=? WHERE IdTarea=? AND IdUsuario=?", ahora, idTarea, idUsuario);
            SQLiteLocal.Ejecutar("UPDATE Tarea_Aplicacion SET SyncDeleted=1,SyncUpdatedUtc=? WHERE IdTarea=? AND IdUsuario=?", ahora, idTarea, idUsuario);
            SQLiteLocal.Ejecutar("UPDATE Tarea_Web SET SyncDeleted=1,SyncUpdatedUtc=? WHERE IdTarea=? AND IdUsuario=?", ahora, idTarea, idUsuario);
            Encolar(idUsuario, "Tarea", filas[0].SyncId, "D", idTarea);
        }

        public static void CambiarEstadoTarea(int idUsuario, int idTarea, char estado)
        {
            var filas = SQLiteLocal.Consultar("SELECT Estado,EsPlantilla,SyncId FROM Tareas WHERE IdTarea=? AND IdUsuario=? AND SyncDeleted=0",
                f => new { Estado = f.Texto("Estado"), EsPlantilla = f.Booleano("EsPlantilla"), SyncId = f.Texto("SyncId") }, idTarea, idUsuario);
            if (filas.Count == 0) throw new ArgumentException("La tarea no existe o no pertenece al usuario.");
            if (filas[0].EsPlantilla) throw new ArgumentException("La plantilla no tiene estado de ejecución.");
            string anterior = filas[0].Estado;
            if (!((anterior == "P" && (estado == 'A' || estado == 'X')) ||
                  (anterior == "A" && (estado == 'C' || estado == 'X'))))
                throw new ArgumentException("La transición de estado no es válida.");
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar("UPDATE Tareas SET Estado=?,SyncUpdatedUtc=? WHERE IdTarea=? AND IdUsuario=?", estado, ahora, idTarea, idUsuario);
            Encolar(idUsuario, "Tarea", filas[0].SyncId, "U", "Estado", estado);
        }

        public static int? BuscarPlantilla(int idUsuario)
        {
            object id = SQLiteLocal.Escalar("SELECT IdTarea FROM Tareas WHERE IdUsuario=? AND EsPlantilla=1 AND SyncDeleted=0 LIMIT 1", idUsuario);
            return id == null ? (int?)null : Convert.ToInt32(id);
        }

        public static CapaEntidades.E_Tarea ObtenerPlantilla(int idUsuario)
        {
            var filas = SQLiteLocal.Consultar(@"SELECT IdTarea,Nombre,FechaInicio,FechaFin,Estado,EsPlantilla,
                NoBloquearApps,NoBloquearWebs,SyncId FROM Tareas
                WHERE IdUsuario=? AND EsPlantilla=1 AND SyncDeleted=0 LIMIT 1",
                f => new CapaEntidades.E_Tarea
                {
                    IdTarea = f.Entero("IdTarea"), IdUsuario = idUsuario, Nombre = f.Texto("Nombre"),
                    FechaInicio = f.Fecha("FechaInicio"), FechaFin = f.Fecha("FechaFin"),
                    Estado = String.IsNullOrEmpty(f.Texto("Estado")) ? 'P' : f.Texto("Estado")[0],
                    EsPlantilla = f.Booleano("EsPlantilla"), NoBloquearApps = f.Booleano("NoBloquearApps"),
                    NoBloquearWebs = f.Booleano("NoBloquearWebs"), SyncId = f.Texto("SyncId")
                }, idUsuario);
            return filas.Count == 0 ? null : filas[0];
        }

        public static int InsertarAplicacion(int idUsuario, CapaEntidades.E_Aplicacion app)
        {
            var existente = SQLiteLocal.Consultar("SELECT IdAplicacion,SyncId FROM Aplicaciones WHERE IdUsuario=? AND Identificador=? AND SyncDeleted=0",
                f => new { Id = f.Entero("IdAplicacion"), SyncId = f.Texto("SyncId") }, idUsuario, app.Identificador);
            if (existente.Count != 0)
            {
                app.IdAplicacion = existente[0].Id; app.IdUsuario = idUsuario; app.SyncId = existente[0].SyncId;
                return existente[0].Id;
            }
            string syncId = String.IsNullOrEmpty(app.SyncId) ? Guid.NewGuid().ToString("D") : app.SyncId;
            int id = Convert.ToInt32(SQLiteLocal.Escalar("SELECT COALESCE(MIN(IdAplicacion),0)-1 FROM Aplicaciones"));
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar(@"INSERT INTO Aplicaciones(IdAplicacion,IdUsuario,Nombre,Identificador,EsCritica,SyncId,SyncUpdatedUtc,SyncDeleted)
                VALUES(?,?,?,?,?,?,?,0)", id, idUsuario, app.Nombre, app.Identificador, app.EsCritica, syncId, ahora);
            app.IdAplicacion = id;
            app.IdUsuario = idUsuario;
            app.SyncId = syncId;
            Encolar(idUsuario, "Aplicacion", syncId, "I", app.Nombre, app.Identificador, app.EsCritica);
            return id;
        }

        public static List<CapaEntidades.E_Aplicacion> BuscarAplicaciones(int idUsuario, string buscar)
        {
            return SQLiteLocal.Consultar(@"SELECT IdAplicacion,Nombre,Identificador,EsCritica,SyncId FROM Aplicaciones
                WHERE IdUsuario=? AND SyncDeleted=0 AND (Nombre LIKE '%' || ? || '%' OR Identificador LIKE '%' || ? || '%')
                ORDER BY Nombre COLLATE NOCASE", f => new CapaEntidades.E_Aplicacion
                {
                    IdAplicacion = f.Entero("IdAplicacion"), IdUsuario = idUsuario,
                    Nombre = f.Texto("Nombre"), Identificador = f.Texto("Identificador"),
                    EsCritica = f.Booleano("EsCritica"), SyncId = f.Texto("SyncId")
                }, idUsuario, buscar ?? "", buscar ?? "");
        }

        public static void ModificarAplicacion(int idUsuario, CapaEntidades.E_Aplicacion app)
        {
            var filas = SQLiteLocal.Consultar("SELECT SyncId FROM Aplicaciones WHERE IdAplicacion=? AND IdUsuario=? AND SyncDeleted=0",
                f => f.Texto("SyncId"), app.IdAplicacion, idUsuario);
            if (filas.Count == 0) throw new ArgumentException("La aplicación no existe o pertenece a otro usuario.");
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar("UPDATE Aplicaciones SET Nombre=?,Identificador=?,SyncUpdatedUtc=? WHERE IdAplicacion=? AND IdUsuario=?",
                app.Nombre, app.Identificador, ahora, app.IdAplicacion, idUsuario);
            app.SyncId = filas[0];
            Encolar(idUsuario, "Aplicacion", app.SyncId, "U", app.Nombre, app.Identificador, app.EsCritica);
        }

        public static void EliminarAplicacion(int idUsuario, int idAplicacion)
        {
            var filas = SQLiteLocal.Consultar("SELECT SyncId FROM Aplicaciones WHERE IdAplicacion=? AND IdUsuario=? AND SyncDeleted=0",
                f => f.Texto("SyncId"), idAplicacion, idUsuario);
            if (filas.Count == 0) throw new ArgumentException("La aplicación no existe o pertenece a otro usuario.");
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar("UPDATE Aplicaciones SET SyncDeleted=1,SyncUpdatedUtc=? WHERE IdAplicacion=? AND IdUsuario=?", ahora, idAplicacion, idUsuario);
            SQLiteLocal.Ejecutar("UPDATE Tarea_Aplicacion SET SyncDeleted=1,SyncUpdatedUtc=? WHERE IdAplicacion=? AND IdUsuario=?", ahora, idAplicacion, idUsuario);
            Encolar(idUsuario, "Aplicacion", filas[0], "D", idAplicacion);
        }

        public static int InsertarWeb(int idUsuario, CapaEntidades.E_Web web)
        {
            var existente = SQLiteLocal.Consultar("SELECT IdWeb,SyncId FROM Webs WHERE IdUsuario=? AND Link=? AND SyncDeleted=0",
                f => new { Id = f.Entero("IdWeb"), SyncId = f.Texto("SyncId") }, idUsuario, web.Link);
            if (existente.Count != 0)
            {
                web.IdWeb = existente[0].Id; web.IdUsuario = idUsuario; web.SyncId = existente[0].SyncId;
                return existente[0].Id;
            }
            string syncId = String.IsNullOrEmpty(web.SyncId) ? Guid.NewGuid().ToString("D") : web.SyncId;
            int id = Convert.ToInt32(SQLiteLocal.Escalar("SELECT COALESCE(MIN(IdWeb),0)-1 FROM Webs"));
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar("INSERT INTO Webs(IdWeb,IdUsuario,Nombre,Link,SyncId,SyncUpdatedUtc,SyncDeleted) VALUES(?,?,?,?,?,?,0)",
                id, idUsuario, web.Nombre, web.Link, syncId, ahora);
            web.IdWeb = id;
            web.IdUsuario = idUsuario;
            web.SyncId = syncId;
            Encolar(idUsuario, "Web", syncId, "I", web.Nombre, web.Link);
            return id;
        }

        public static List<CapaEntidades.E_Web> BuscarWebs(int idUsuario, string buscar)
        {
            return SQLiteLocal.Consultar(@"SELECT IdWeb,Nombre,Link,SyncId FROM Webs WHERE IdUsuario=? AND SyncDeleted=0
                AND (Nombre LIKE '%' || ? || '%' OR Link LIKE '%' || ? || '%') ORDER BY Nombre COLLATE NOCASE",
                f => new CapaEntidades.E_Web
                {
                    IdWeb = f.Entero("IdWeb"), IdUsuario = idUsuario, Nombre = f.Texto("Nombre"),
                    Link = f.Texto("Link"), SyncId = f.Texto("SyncId")
                }, idUsuario, buscar ?? "", buscar ?? "");
        }

        public static void ModificarWeb(int idUsuario, CapaEntidades.E_Web web)
        {
            var filas = SQLiteLocal.Consultar("SELECT SyncId FROM Webs WHERE IdWeb=? AND IdUsuario=? AND SyncDeleted=0",
                f => f.Texto("SyncId"), web.IdWeb, idUsuario);
            if (filas.Count == 0) throw new ArgumentException("La web no existe o pertenece a otro usuario.");
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar("UPDATE Webs SET Nombre=?,Link=?,SyncUpdatedUtc=? WHERE IdWeb=? AND IdUsuario=?",
                web.Nombre, web.Link, ahora, web.IdWeb, idUsuario);
            web.SyncId = filas[0];
            Encolar(idUsuario, "Web", web.SyncId, "U", web.Nombre, web.Link);
        }

        public static void EliminarWeb(int idUsuario, int idWeb)
        {
            var filas = SQLiteLocal.Consultar("SELECT SyncId FROM Webs WHERE IdWeb=? AND IdUsuario=? AND SyncDeleted=0",
                f => f.Texto("SyncId"), idWeb, idUsuario);
            if (filas.Count == 0) throw new ArgumentException("La web no existe o pertenece a otro usuario.");
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar("UPDATE Webs SET SyncDeleted=1,SyncUpdatedUtc=? WHERE IdWeb=? AND IdUsuario=?", ahora, idWeb, idUsuario);
            SQLiteLocal.Ejecutar("UPDATE Tarea_Web SET SyncDeleted=1,SyncUpdatedUtc=? WHERE IdWeb=? AND IdUsuario=?", ahora, idWeb, idUsuario);
            Encolar(idUsuario, "Web", filas[0], "D", idWeb);
        }

        public static void InsertarDescanso(int idUsuario, CapaEntidades.E_Descanso descanso)
        {
            var tarea = SQLiteLocal.Consultar("SELECT 1 FROM Tareas WHERE IdTarea=? AND IdUsuario=? AND SyncDeleted=0",
                f => Convert.ToInt32(f[0], CultureInfo.InvariantCulture), descanso.IdTarea, idUsuario);
            if (tarea.Count == 0) throw new ArgumentException("La tarea no existe o pertenece a otro usuario.");
            string sync = String.IsNullOrEmpty(descanso.SyncId) ? Guid.NewGuid().ToString("D") : descanso.SyncId;
            var existente = SQLiteLocal.Consultar("SELECT IdDescanso,IdTarea,SyncId FROM Descansos WHERE IdUsuario=? AND SyncId=? AND SyncDeleted=0",
                f => new { Id = f.Entero("IdDescanso"), SyncId = f.Texto("SyncId") }, idUsuario, sync);
            if (existente.Count != 0)
            {
                descanso.IdDescanso = existente[0].Id; descanso.SyncId = existente[0].SyncId;
                return;
            }
            object descansoExistente = SQLiteLocal.Escalar(
                "SELECT COUNT(*) FROM Descansos WHERE IdUsuario=? AND IdTarea=? AND SyncDeleted=0", idUsuario, descanso.IdTarea);
            if (Convert.ToInt32(descansoExistente ?? 0) > 0)
                throw new ArgumentException("La tarea ya tiene un descanso configurado.");
            int id = Convert.ToInt32(SQLiteLocal.Escalar("SELECT COALESCE(MIN(IdDescanso),0)-1 FROM Descansos"));
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar(@"INSERT INTO Descansos(IdDescanso,IdTarea,IdUsuario,IntervaloDescanso,DuracionDescanso,SyncId,SyncUpdatedUtc,SyncDeleted)
                VALUES(?,?,?,?,?,?,?,0)", id, descanso.IdTarea, idUsuario, descanso.IntervaloDescanso, descanso.DuracionDescanso, sync, ahora);
            descanso.IdDescanso = id;
            descanso.SyncId = sync;
            Encolar(idUsuario, "Descanso", sync, "I", descanso.IdTarea, descanso.IntervaloDescanso, descanso.DuracionDescanso);
        }

        public static CapaEntidades.E_Descanso BuscarDescanso(int idUsuario, int idTarea)
        {
            var filas = SQLiteLocal.Consultar(@"SELECT IdDescanso,IdTarea,IntervaloDescanso,DuracionDescanso,SyncId
                FROM Descansos WHERE IdUsuario=? AND IdTarea=? AND SyncDeleted=0 LIMIT 1",
                f => new CapaEntidades.E_Descanso
                {
                    IdDescanso = f.Entero("IdDescanso"), IdTarea = f.Entero("IdTarea"),
                    IntervaloDescanso = f["IntervaloDescanso"] == null ? (int?)null : f.Entero("IntervaloDescanso"),
                    DuracionDescanso = f["DuracionDescanso"] == null ? (int?)null : f.Entero("DuracionDescanso"),
                    SyncId = f.Texto("SyncId")
                }, idUsuario, idTarea);
            return filas.Count == 0 ? null : filas[0];
        }

        public static void ModificarDescanso(int idUsuario, CapaEntidades.E_Descanso descanso)
        {
            var filas = SQLiteLocal.Consultar("SELECT SyncId,IdTarea FROM Descansos WHERE IdDescanso=? AND IdUsuario=? AND SyncDeleted=0",
                f => new { SyncId = f.Texto("SyncId"), IdTarea = f.Entero("IdTarea") }, descanso.IdDescanso, idUsuario);
            if (filas.Count == 0) throw new ArgumentException("El descanso no existe o pertenece a otro usuario.");
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar("UPDATE Descansos SET IntervaloDescanso=?,DuracionDescanso=?,SyncUpdatedUtc=? WHERE IdDescanso=? AND IdUsuario=?",
                descanso.IntervaloDescanso, descanso.DuracionDescanso, ahora, descanso.IdDescanso, idUsuario);
            descanso.SyncId = filas[0].SyncId;
            Encolar(idUsuario, "Descanso", descanso.SyncId, "U", filas[0].IdTarea, descanso.IntervaloDescanso, descanso.DuracionDescanso);
        }

        public static void EliminarDescanso(int idUsuario, int idDescanso)
        {
            var filas = SQLiteLocal.Consultar("SELECT SyncId FROM Descansos WHERE IdDescanso=? AND IdUsuario=? AND SyncDeleted=0",
                f => f.Texto("SyncId"), idDescanso, idUsuario);
            if (filas.Count == 0) throw new ArgumentException("El descanso no existe o pertenece a otro usuario.");
            SQLiteLocal.Ejecutar("UPDATE Descansos SET SyncDeleted=1,SyncUpdatedUtc=? WHERE IdDescanso=? AND IdUsuario=?",
                DateTime.UtcNow, idDescanso, idUsuario);
            Encolar(idUsuario, "Descanso", filas[0], "D", idDescanso);
        }

        public static void InsertarTareaAplicacion(int idUsuario, int idTarea, int idAplicacion)
        {
            string syncTarea = SyncEntidad("Tareas", "IdTarea", idTarea, idUsuario);
            string syncApp = SyncEntidad("Aplicaciones", "IdAplicacion", idAplicacion, idUsuario);
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar("INSERT OR REPLACE INTO Tarea_Aplicacion(IdTarea,IdUsuario,IdAplicacion,SyncUpdatedUtc,SyncDeleted) VALUES(?,?,?,?,0)",
                idTarea, idUsuario, idAplicacion, ahora);
            Encolar(idUsuario, "TareaAplicacion", Guid.NewGuid().ToString("D"), "I", syncTarea, syncApp);
        }

        public static List<CapaEntidades.E_TareaAplicacion> BuscarTareaAplicacion(int idUsuario, int idTarea)
        {
            return SQLiteLocal.Consultar(@"SELECT TA.IdTarea,TA.IdAplicacion,A.Nombre Aplicacion,T.Nombre Tarea
                FROM Tarea_Aplicacion TA JOIN Aplicaciones A ON A.IdAplicacion=TA.IdAplicacion AND A.IdUsuario=TA.IdUsuario
                JOIN Tareas T ON T.IdTarea=TA.IdTarea AND T.IdUsuario=TA.IdUsuario
                WHERE TA.IdUsuario=? AND TA.IdTarea=? AND TA.SyncDeleted=0 AND A.SyncDeleted=0 AND T.SyncDeleted=0",
                f => new CapaEntidades.E_TareaAplicacion
                {
                    IdTarea = f.Entero("IdTarea"), IdAplicacion = f.Entero("IdAplicacion"),
                    NombreAplicacion = f.Texto("Aplicacion"), NombreTarea = f.Texto("Tarea")
                }, idUsuario, idTarea);
        }

        public static void EliminarTareaAplicacion(int idUsuario, int idTarea, int idAplicacion)
        {
            string syncTarea = SyncEntidad("Tareas", "IdTarea", idTarea, idUsuario);
            string syncApp = SyncEntidad("Aplicaciones", "IdAplicacion", idAplicacion, idUsuario);
            SQLiteLocal.Ejecutar("UPDATE Tarea_Aplicacion SET SyncDeleted=1,SyncUpdatedUtc=? WHERE IdTarea=? AND IdUsuario=? AND IdAplicacion=?",
                DateTime.UtcNow, idTarea, idUsuario, idAplicacion);
            Encolar(idUsuario, "TareaAplicacion", Guid.NewGuid().ToString("D"), "D", syncTarea, syncApp);
        }

        public static void InsertarTareaWeb(int idUsuario, int idTarea, int idWeb)
        {
            string syncTarea = SyncEntidad("Tareas", "IdTarea", idTarea, idUsuario);
            string syncWeb = SyncEntidad("Webs", "IdWeb", idWeb, idUsuario);
            SQLiteLocal.Ejecutar("INSERT OR REPLACE INTO Tarea_Web(IdTarea,IdUsuario,IdWeb,SyncUpdatedUtc,SyncDeleted) VALUES(?,?,?,?,0)",
                idTarea, idUsuario, idWeb, DateTime.UtcNow);
            Encolar(idUsuario, "TareaWeb", Guid.NewGuid().ToString("D"), "I", syncTarea, syncWeb);
        }

        public static List<CapaEntidades.E_TareaWeb> BuscarTareaWeb(int idUsuario, int idTarea)
        {
            return SQLiteLocal.Consultar(@"SELECT TW.IdTarea,TW.IdWeb,W.Nombre Web,T.Nombre Tarea
                FROM Tarea_Web TW JOIN Webs W ON W.IdWeb=TW.IdWeb AND W.IdUsuario=TW.IdUsuario
                JOIN Tareas T ON T.IdTarea=TW.IdTarea AND T.IdUsuario=TW.IdUsuario
                WHERE TW.IdUsuario=? AND TW.IdTarea=? AND TW.SyncDeleted=0 AND W.SyncDeleted=0 AND T.SyncDeleted=0",
                f => new CapaEntidades.E_TareaWeb
                {
                    IdTarea = f.Entero("IdTarea"), IdWeb = f.Entero("IdWeb"),
                    NombreWeb = f.Texto("Web"), NombreTarea = f.Texto("Tarea")
                }, idUsuario, idTarea);
        }

        public static void EliminarTareaWeb(int idUsuario, int idTarea, int idWeb)
        {
            string syncTarea = SyncEntidad("Tareas", "IdTarea", idTarea, idUsuario);
            string syncWeb = SyncEntidad("Webs", "IdWeb", idWeb, idUsuario);
            SQLiteLocal.Ejecutar("UPDATE Tarea_Web SET SyncDeleted=1,SyncUpdatedUtc=? WHERE IdTarea=? AND IdUsuario=? AND IdWeb=?",
                DateTime.UtcNow, idTarea, idUsuario, idWeb);
            Encolar(idUsuario, "TareaWeb", Guid.NewGuid().ToString("D"), "D", syncTarea, syncWeb);
        }

        public static void InsertarConfiguracion(CapaEntidades.E_Configuracion configuracion)
        {
            string sync = Guid.NewGuid().ToString("D");
            int id = Convert.ToInt32(SQLiteLocal.Escalar("SELECT COALESCE(MIN(IdConfiguracion),0)-1 FROM Configuraciones"));
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar("INSERT INTO Configuraciones(IdConfiguracion,IdUsuario,AutoSesion,SyncId,SyncUpdatedUtc,SyncDeleted) VALUES(?,?,?,?,?,0)",
                id, configuracion.IdUsuario, configuracion.AutoSesion, sync, ahora);
            configuracion.IdConfiguracion = id;
            configuracion.SyncId = sync;
            Encolar(configuracion.IdUsuario, "Configuracion", sync, "I", configuracion.AutoSesion);
        }

        public static CapaEntidades.E_Configuracion BuscarConfiguracion(int idUsuario)
        {
            var rows = SQLiteLocal.Consultar("SELECT IdConfiguracion,IdUsuario,AutoSesion,SyncId FROM Configuraciones WHERE IdUsuario=? AND SyncDeleted=0 LIMIT 1",
                f => new CapaEntidades.E_Configuracion
                {
                    IdConfiguracion = f.Entero("IdConfiguracion"), IdUsuario = f.Entero("IdUsuario"),
                    AutoSesion = f.Booleano("AutoSesion"), SyncId = f.Texto("SyncId")
                }, idUsuario);
            return rows.Count == 0 ? null : rows[0];
        }

        public static void ModificarConfiguracion(CapaEntidades.E_Configuracion configuracion)
        {
            var rows = SQLiteLocal.Consultar("SELECT SyncId FROM Configuraciones WHERE IdConfiguracion=? AND IdUsuario=? AND SyncDeleted=0",
                f => f.Texto("SyncId"), configuracion.IdConfiguracion, configuracion.IdUsuario);
            if (rows.Count == 0) throw new ArgumentException("La configuración no existe o pertenece a otro usuario.");
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar("UPDATE Configuraciones SET AutoSesion=?,SyncUpdatedUtc=? WHERE IdConfiguracion=? AND IdUsuario=?",
                configuracion.AutoSesion, ahora, configuracion.IdConfiguracion, configuracion.IdUsuario);
            configuracion.SyncId = rows[0];
            Encolar(configuracion.IdUsuario, "Configuracion", configuracion.SyncId, "U", configuracion.AutoSesion);
        }

        private static string SyncEntidad(string tabla, string campoId, int id, int idUsuario)
        {
            var ids = SQLiteLocal.Consultar("SELECT SyncId FROM " + tabla + " WHERE " + campoId + "=? AND IdUsuario=? AND SyncDeleted=0",
                f => f.Texto("SyncId"), id, idUsuario);
            if (ids.Count == 0) throw new ArgumentException("El elemento no existe o pertenece a otro usuario.");
            return ids[0];
        }

        private static void Encolar(int idUsuario, string tipo, string syncId, string operacion, params object[] valores)
        {
            var partes = new List<string>();
            foreach (object valor in valores)
            {
                string texto = valor == null ? "" : valor is DateTime
                    ? ((DateTime)valor).ToUniversalTime().ToString("o", CultureInfo.InvariantCulture)
                    : Convert.ToString(valor, CultureInfo.InvariantCulture);
                partes.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(texto)));
            }
            SQLiteLocal.Ejecutar(@"INSERT INTO SyncOutbox
                (IdUsuario,TipoEntidad,SyncId,Operacion,Payload,CreadoUtc,Intentos)
                VALUES(?,?,?,?,?,?,0)", idUsuario, tipo, syncId, operacion,
                String.Join(".", partes), DateTime.UtcNow);
        }

        public static bool TieneCopiaCompleta(int idUsuario)
        {
            if (idUsuario <= 0) return false;
            object valor = SQLiteLocal.Escalar(@"SELECT COUNT(*) FROM EstadoSincronizacion s
                INNER JOIN UsuariosLocales u ON u.IdUsuario=s.IdUsuario
                WHERE s.IdUsuario=? AND s.UltimaCopiaCompletaUtc IS NOT NULL AND u.SyncDeleted=0", idUsuario);
            return Convert.ToInt32(valor ?? 0) > 0;
        }

        public static DateTime? UltimaCopiaCompleta(int idUsuario)
        {
            object valor = SQLiteLocal.Escalar("SELECT UltimaCopiaCompletaUtc FROM EstadoSincronizacion WHERE IdUsuario=?", idUsuario);
            if (valor == null) return null;
            DateTime fecha;
            return DateTime.TryParse(Convert.ToString(valor), out fecha) ? (DateTime?)fecha : null;
        }

        public static void CapturarCopiaCompletaDesdeCache(int idUsuario)
        {
            var usuarios = SQLiteLocal.Consultar("SELECT Correo,Nombre FROM UsuariosLocales WHERE IdUsuario=? AND SyncDeleted=0",
                f => new { Correo = f.Texto("Correo"), Nombre = f.Texto("Nombre") }, idUsuario);
            if (usuarios.Count == 0) throw new InvalidOperationException("No existe el perfil local de esta cuenta.");
            new D_SincronizacionOffline().CapturarCopiaCompleta(idUsuario, usuarios[0].Correo, usuarios[0].Nombre);
        }

        public void CapturarCopiaCompleta(int idUsuario, string correo, string nombre)
        {
            if (idUsuario <= 0) throw new ArgumentException("El usuario no es válido.");

            using (var conexion = ConexionDB.Instancia.CrearConexion())
            {
                conexion.Open();
                using (var transaccion = conexion.BeginTransaction())
                {
                    // Check the outbox under the same write lock used to replace the snapshot.
                    // Otherwise a concurrent offline write could be cascaded away by REPLACE.
                    bool omitirCopia = SQLiteLocal.EnTransaccion(() =>
                    {
                        object pendientes = SQLiteLocal.Escalar("SELECT COUNT(*) FROM SyncOutbox WHERE IdUsuario=?", idUsuario);
                        if (Convert.ToInt32(pendientes ?? 0) != 0) return true;

                        GuardarUsuario(idUsuario, correo, nombre);
                        LimpiarDatosUsuario(idUsuario);
                        CopiarConfiguracion(conexion, transaccion, idUsuario);
                        CopiarTareas(conexion, transaccion, idUsuario);
                        CopiarAplicaciones(conexion, transaccion, idUsuario);
                        CopiarWebs(conexion, transaccion, idUsuario);
                        CopiarDescansos(conexion, transaccion, idUsuario);
                        CopiarRelaciones(conexion, transaccion, idUsuario, "Tarea_Aplicacion", "IdAplicacion");
                        CopiarRelaciones(conexion, transaccion, idUsuario, "Tarea_Web", "IdWeb");
                        MarcarCopiaCompleta(idUsuario);
                        return false;
                    });
                    if (omitirCopia) return;
                    transaccion.Commit();
                }
            }
        }

        private static void GuardarUsuario(int idUsuario, string correo, string nombre)
        {
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar(@"INSERT OR REPLACE INTO UsuariosLocales
                (IdUsuario,Correo,Nombre,SyncUpdatedUtc,SyncDeleted) VALUES(?,?,?,?,0)",
                idUsuario, correo ?? "", nombre ?? "", ahora);
            SQLiteLocal.Ejecutar("INSERT OR REPLACE INTO EstadoSincronizacion(IdUsuario,UltimaCopiaCompletaUtc,UltimaSincronizacionUtc) VALUES(?,NULL,NULL)", idUsuario);
        }

        private static void LimpiarDatosUsuario(int idUsuario)
        {
            SQLiteLocal.Ejecutar("DELETE FROM Tarea_Aplicacion WHERE IdUsuario=?", idUsuario);
            SQLiteLocal.Ejecutar("DELETE FROM Tarea_Web WHERE IdUsuario=?", idUsuario);
            SQLiteLocal.Ejecutar("DELETE FROM Descansos WHERE IdUsuario=?", idUsuario);
            SQLiteLocal.Ejecutar("DELETE FROM Tareas WHERE IdUsuario=?", idUsuario);
            SQLiteLocal.Ejecutar("DELETE FROM Aplicaciones WHERE IdUsuario=?", idUsuario);
            SQLiteLocal.Ejecutar("DELETE FROM Webs WHERE IdUsuario=?", idUsuario);
            SQLiteLocal.Ejecutar("DELETE FROM Configuraciones WHERE IdUsuario=?", idUsuario);
        }

        private static void CopiarConfiguracion(SqlConnection cn, SqlTransaction tx, int idUsuario)
        {
            using (var cmd = new SqlCommand("SELECT IdConfiguracion,AutoSesion,CONVERT(varchar(36),SyncId),SyncUpdatedUtc FROM dbo.Configuraciones WHERE IdUsuario=@IdUsuario", cn, tx))
            {
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read()) SQLiteLocal.Ejecutar(@"INSERT OR REPLACE INTO Configuraciones
                        (IdConfiguracion,IdServidor,IdUsuario,AutoSesion,SyncId,SyncUpdatedUtc,SyncDeleted)
                        VALUES(?,?,?,?,?,?,0)", rd.GetInt32(0), rd.GetInt32(0), idUsuario,
                        rd.GetBoolean(1), rd.GetString(2), rd.GetDateTime(3));
            }
        }

        private static void CopiarTareas(SqlConnection cn, SqlTransaction tx, int idUsuario)
        {
            using (var cmd = new SqlCommand(@"SELECT IdTarea,Nombre,FechaInicio,FechaFin,Estado,EsPlantilla,
                    NoBloquearApps,NoBloquearWebs,CONVERT(varchar(36),SyncId),SyncUpdatedUtc
                FROM dbo.Tareas WHERE IdUsuario=@IdUsuario", cn, tx))
            {
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read()) SQLiteLocal.Ejecutar(@"INSERT OR REPLACE INTO Tareas
                        (IdTarea,IdServidor,IdUsuario,Nombre,FechaInicio,FechaFin,Estado,EsPlantilla,
                         NoBloquearApps,NoBloquearWebs,SyncId,SyncUpdatedUtc,SyncDeleted)
                        VALUES(?,?,?,?,?,?,?,?,?,?,?,?,0)", rd.GetInt32(0), rd.GetInt32(0), idUsuario,
                        rd.GetString(1), rd.GetDateTime(2), rd.GetDateTime(3), rd.GetString(4),
                        rd.GetBoolean(5), rd.GetBoolean(6), rd.GetBoolean(7), rd.GetString(8), rd.GetDateTime(9));
            }
        }

        private static void CopiarAplicaciones(SqlConnection cn, SqlTransaction tx, int idUsuario)
        {
            using (var cmd = new SqlCommand("SELECT IdAplicacion,Nombre,Identificador,EsCritica,CONVERT(varchar(36),SyncId),SyncUpdatedUtc FROM dbo.Aplicaciones WHERE IdUsuario=@IdUsuario", cn, tx))
            {
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read()) SQLiteLocal.Ejecutar(@"INSERT OR REPLACE INTO Aplicaciones
                        (IdAplicacion,IdServidor,IdUsuario,Nombre,Identificador,EsCritica,SyncId,SyncUpdatedUtc,SyncDeleted)
                        VALUES(?,?,?,?,?,?,?,?,0)", rd.GetInt32(0), rd.GetInt32(0), idUsuario,
                        rd.GetString(1), rd.GetString(2), rd.GetBoolean(3), rd.GetString(4), rd.GetDateTime(5));
            }
        }

        private static void CopiarWebs(SqlConnection cn, SqlTransaction tx, int idUsuario)
        {
            using (var cmd = new SqlCommand("SELECT IdWeb,Nombre,Link,CONVERT(varchar(36),SyncId),SyncUpdatedUtc FROM dbo.Webs WHERE IdUsuario=@IdUsuario", cn, tx))
            {
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read()) SQLiteLocal.Ejecutar(@"INSERT OR REPLACE INTO Webs
                        (IdWeb,IdServidor,IdUsuario,Nombre,Link,SyncId,SyncUpdatedUtc,SyncDeleted)
                        VALUES(?,?,?,?,?,?,?,0)", rd.GetInt32(0), rd.GetInt32(0), idUsuario,
                        rd.GetString(1), rd.GetString(2), rd.GetString(3), rd.GetDateTime(4));
            }
        }

        private static void CopiarDescansos(SqlConnection cn, SqlTransaction tx, int idUsuario)
        {
            using (var cmd = new SqlCommand(@"SELECT IdDescanso,IdTarea,IntervaloDescanso,DuracionDescanso,
                    CONVERT(varchar(36),SyncId),SyncUpdatedUtc FROM dbo.Descansos WHERE IdUsuario=@IdUsuario", cn, tx))
            {
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read()) SQLiteLocal.Ejecutar(@"INSERT OR REPLACE INTO Descansos
                        (IdDescanso,IdServidor,IdTarea,IdUsuario,IntervaloDescanso,DuracionDescanso,SyncId,SyncUpdatedUtc,SyncDeleted)
                        VALUES(?,?,?,?,?,?,?,?,0)", rd.GetInt32(0), rd.GetInt32(0), rd.GetInt32(1), idUsuario,
                        rd.IsDBNull(2) ? null : (object)rd.GetInt32(2), rd.IsDBNull(3) ? null : (object)rd.GetInt32(3),
                        rd.GetString(4), rd.GetDateTime(5));
            }
        }

        private static void CopiarRelaciones(SqlConnection cn, SqlTransaction tx, int idUsuario, string tabla, string idRelacion)
        {
            string sql = "SELECT IdTarea," + idRelacion + ",SyncUpdatedUtc FROM dbo." + tabla + " WHERE IdUsuario=@IdUsuario";
            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        string destino = tabla == "Tarea_Aplicacion" ? "Tarea_Aplicacion" : "Tarea_Web";
                        string idCampo = tabla == "Tarea_Aplicacion" ? "IdAplicacion" : "IdWeb";
                        SQLiteLocal.Ejecutar("INSERT OR REPLACE INTO " + destino +
                            "(IdTarea,IdUsuario," + idCampo + ",SyncUpdatedUtc,SyncDeleted) VALUES(?,?,?,?,0)",
                            rd.GetInt32(0), idUsuario, rd.GetInt32(1), rd.GetDateTime(2));
                    }
                }
            }
        }

        private static void MarcarCopiaCompleta(int idUsuario)
        {
            DateTime ahora = DateTime.UtcNow;
            SQLiteLocal.Ejecutar("UPDATE EstadoSincronizacion SET UltimaCopiaCompletaUtc=?,UltimaSincronizacionUtc=? WHERE IdUsuario=?", ahora, ahora, idUsuario);
        }
    }
}
