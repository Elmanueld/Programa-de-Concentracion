using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace CapaDatos
{
    /// <summary>
    /// Replays the per-user SQLite outbox into SQL Server. Each change has a
    /// device-scoped sequence so retries are idempotent.
    /// Conflict policy: the newest UTC change timestamp wins.
    /// </summary>
    public sealed class D_ProcesadorSincronizacionOffline
    {
        private sealed class Cambio
        {
            public long Id;
            public int IdUsuario;
            public string Tipo;
            public string SyncId;
            public string Operacion;
            public string Payload;
            public DateTime CreadoUtc;
        }

        public static event Action<int> SincronizacionCompletada;

        public static bool HayPendientes(int idUsuario)
        {
            if (idUsuario <= 0) return false;
            object n = SQLiteLocal.Escalar("SELECT COUNT(*) FROM SyncOutbox WHERE IdUsuario=?", idUsuario);
            return Convert.ToInt32(n ?? 0, CultureInfo.InvariantCulture) > 0;
        }

        public bool Sincronizar(int idUsuario)
        {
            if (idUsuario <= 0) throw new ArgumentException("El usuario no es válido.");
            if (!D_SincronizacionOffline.TieneCopiaCompleta(idUsuario)) return false;

            List<Cambio> cambios = SQLiteLocal.Consultar(@"SELECT IdCambio,IdUsuario,TipoEntidad,SyncId,Operacion,Payload,CreadoUtc
                FROM SyncOutbox WHERE IdUsuario=? ORDER BY IdCambio",
                f => new Cambio
                {
                    Id = Convert.ToInt64(f["IdCambio"], CultureInfo.InvariantCulture),
                    IdUsuario = f.Entero("IdUsuario"), Tipo = f.Texto("TipoEntidad"),
                    SyncId = f.Texto("SyncId"), Operacion = f.Texto("Operacion"),
                    Payload = f.Texto("Payload"), CreadoUtc = f.Fecha("CreadoUtc")
                }, idUsuario);

            if (cambios.Count == 0)
            {
                try
                {
                    D_SincronizacionOffline.CapturarCopiaCompletaDesdeCache(idUsuario);
                    D_SincronizacionOffline.ActivarModoOnline();
                    return true;
                }
                catch (SqlException ex)
                {
                    if (D_Comun.EsFalloDeConexion(ex)) return false;
                    throw;
                }
            }

            Guid dispositivo = ObtenerIdDispositivo();
            try
            {
                using (var conexion = ConexionDB.Instancia.CrearConexion())
                {
                    conexion.Open();
                    RegistrarDispositivo(conexion, idUsuario, dispositivo);
                    foreach (Cambio cambio in cambios)
                    {
                        AplicarCambio(conexion, cambio, dispositivo);
                        SQLiteLocal.Ejecutar("DELETE FROM SyncOutbox WHERE IdUsuario=? AND IdCambio=?", idUsuario, cambio.Id);
                    }
                }

                D_SincronizacionOffline.CapturarCopiaCompletaDesdeCache(idUsuario);
                D_SincronizacionOffline.ActivarModoOnline();
                Action<int> manejador = SincronizacionCompletada;
                if (manejador != null) manejador(idUsuario);
                return true;
            }
            catch (SqlException ex)
            {
                if (D_Comun.EsFalloDeConexion(ex)) return false;
                RegistrarErrorLocal(idUsuario, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                RegistrarErrorLocal(idUsuario, ex.Message);
                throw;
            }
        }

        private static Guid ObtenerIdDispositivo()
        {
            var rows = SQLiteLocal.Consultar("SELECT Valor FROM MetadatosLocal WHERE Clave='IdDispositivo'",
                f => f.Texto("Valor"));
            Guid id;
            if (rows.Count > 0 && Guid.TryParse(rows[0], out id)) return id;
            id = Guid.NewGuid();
            SQLiteLocal.Ejecutar("INSERT OR REPLACE INTO MetadatosLocal(Clave,Valor) VALUES('IdDispositivo',?)", id.ToString("D"));
            return id;
        }

        private static void RegistrarDispositivo(SqlConnection cn, int idUsuario, Guid dispositivo)
        {
            using (var cmd = new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.SyncDispositivos WHERE IdUsuario=@IdUsuario AND IdDispositivo=@IdDispositivo)
                UPDATE dbo.SyncDispositivos SET NombreDispositivo=@Nombre WHERE IdUsuario=@IdUsuario AND IdDispositivo=@IdDispositivo
                ELSE INSERT dbo.SyncDispositivos(IdUsuario,IdDispositivo,NombreDispositivo) VALUES(@IdUsuario,@IdDispositivo,@Nombre)", cn))
            {
                cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario;
                cmd.Parameters.Add("@IdDispositivo", SqlDbType.UniqueIdentifier).Value = dispositivo;
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = Environment.MachineName;
                cmd.ExecuteNonQuery();
            }
        }

        private static void AplicarCambio(SqlConnection cn, Cambio cambio, Guid dispositivo)
        {
            using (var tx = cn.BeginTransaction())
            {
                using (var consulta = new SqlCommand(@"SELECT COUNT(*) FROM dbo.SyncCambiosAplicados
                    WHERE IdUsuario=@IdUsuario AND IdDispositivo=@IdDispositivo AND IdCambioLocal=@IdCambio", cn, tx))
                {
                    Parametro(consulta, "@IdUsuario", SqlDbType.Int, cambio.IdUsuario);
                    Parametro(consulta, "@IdDispositivo", SqlDbType.UniqueIdentifier, dispositivo);
                    Parametro(consulta, "@IdCambio", SqlDbType.BigInt, cambio.Id);
                    if (Convert.ToInt32(consulta.ExecuteScalar(), CultureInfo.InvariantCulture) == 0)
                    {
                        AplicarEntidad(cn, tx, cambio);
                        using (var marcar = new SqlCommand(@"INSERT dbo.SyncCambiosAplicados
                            (IdUsuario,IdDispositivo,IdCambioLocal) VALUES(@IdUsuario,@IdDispositivo,@IdCambio)", cn, tx))
                        {
                            Parametro(marcar, "@IdUsuario", SqlDbType.Int, cambio.IdUsuario);
                            Parametro(marcar, "@IdDispositivo", SqlDbType.UniqueIdentifier, dispositivo);
                            Parametro(marcar, "@IdCambio", SqlDbType.BigInt, cambio.Id);
                            marcar.ExecuteNonQuery();
                        }
                    }
                }
                tx.Commit();
            }
        }

        private static void AplicarEntidad(SqlConnection cn, SqlTransaction tx, Cambio c)
        {
            string[] v = Decodificar(c.Payload);
            switch (c.Tipo)
            {
                case "Tarea": AplicarTarea(cn, tx, c, v); break;
                case "Aplicacion": AplicarAplicacion(cn, tx, c, v); break;
                case "Web": AplicarWeb(cn, tx, c, v); break;
                case "Descanso": AplicarDescanso(cn, tx, c, v); break;
                case "Configuracion": AplicarConfiguracion(cn, tx, c, v); break;
                case "TareaAplicacion": AplicarRelacion(cn, tx, c, v, true); break;
                case "TareaWeb": AplicarRelacion(cn, tx, c, v, false); break;
                default: throw new InvalidOperationException("Tipo de cambio offline no reconocido: " + c.Tipo);
            }
        }

        private static void AplicarTarea(SqlConnection cn, SqlTransaction tx, Cambio c, string[] v)
        {
            if (c.Operacion == "D") { EliminarEntidad(cn, tx, c, "Tareas"); return; }
            if (v.Length == 2 && v[0] == "Estado")
            {
                using (var cmd = new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.SyncTombstones WHERE IdUsuario=@IdUsuario AND TipoEntidad='Tarea' AND SyncId=@SyncId AND EliminadoUtc>=@Fecha) RETURN;
                    UPDATE dbo.Tareas SET Estado=@Estado,SyncUpdatedUtc=@Fecha
                    WHERE IdUsuario=@IdUsuario AND SyncId=@SyncId AND SyncUpdatedUtc<=@Fecha", cn, tx))
                {
                    Parametro(cmd, "@Estado", SqlDbType.Char, 1, v[1]); Parametro(cmd, "@Fecha", SqlDbType.DateTime2, c.CreadoUtc);
                    AgregarClave(cmd, c); cmd.ExecuteNonQuery();
                }
                return;
            }

            string estado = v.Length >= 6 ? v[3] : "P";
            bool plantilla = v.Length >= 7 && Bool(v[4]);
            bool noApps = v.Length >= 7 ? Bool(v[5]) : Bool(v[4]);
            bool noWebs = v.Length >= 7 ? Bool(v[6]) : Bool(v[5]);
            using (var cmd = new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.SyncTombstones WHERE IdUsuario=@IdUsuario AND TipoEntidad='Tarea' AND SyncId=@SyncId AND EliminadoUtc>=@Fecha) RETURN;
                IF EXISTS(SELECT 1 FROM dbo.Tareas WHERE IdUsuario=@IdUsuario AND SyncId=@SyncId)
                UPDATE dbo.Tareas SET Nombre=@Nombre,FechaInicio=@Inicio,FechaFin=@Fin,Estado=@Estado,
                  EsPlantilla=@Plantilla,NoBloquearApps=@NoApps,NoBloquearWebs=@NoWebs,SyncUpdatedUtc=@Fecha
                WHERE IdUsuario=@IdUsuario AND SyncId=@SyncId AND SyncUpdatedUtc<=@Fecha
                ELSE INSERT dbo.Tareas(IdUsuario,Nombre,FechaInicio,FechaFin,Estado,EsPlantilla,NoBloquearApps,NoBloquearWebs,SyncId,SyncUpdatedUtc)
                  VALUES(@IdUsuario,@Nombre,@Inicio,@Fin,@Estado,@Plantilla,@NoApps,@NoWebs,@SyncId,@Fecha)", cn, tx))
            {
                AgregarClave(cmd, c); Parametro(cmd, "@Nombre", SqlDbType.VarChar, 70, v[0]);
                Parametro(cmd, "@Inicio", SqlDbType.DateTime, Fecha(v[1])); Parametro(cmd, "@Fin", SqlDbType.DateTime, Fecha(v[2]));
                Parametro(cmd, "@Estado", SqlDbType.Char, 1, estado); Parametro(cmd, "@Plantilla", SqlDbType.Bit, plantilla);
                Parametro(cmd, "@NoApps", SqlDbType.Bit, noApps); Parametro(cmd, "@NoWebs", SqlDbType.Bit, noWebs);
                Parametro(cmd, "@Fecha", SqlDbType.DateTime2, c.CreadoUtc); cmd.ExecuteNonQuery();
            }
        }

        private static void AplicarAplicacion(SqlConnection cn, SqlTransaction tx, Cambio c, string[] v)
        {
            if (c.Operacion == "D") { EliminarEntidad(cn, tx, c, "Aplicaciones"); return; }
            using (var cmd = new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.SyncTombstones WHERE IdUsuario=@IdUsuario AND TipoEntidad='Aplicacion' AND SyncId=@SyncId AND EliminadoUtc>=@Fecha) RETURN;
                IF EXISTS(SELECT 1 FROM dbo.Aplicaciones WHERE IdUsuario=@IdUsuario AND SyncId=@SyncId)
                UPDATE dbo.Aplicaciones SET Nombre=@Nombre,Identificador=@Identificador,EsCritica=@Critica,SyncUpdatedUtc=@Fecha
                WHERE IdUsuario=@IdUsuario AND SyncId=@SyncId AND SyncUpdatedUtc<=@Fecha
                ELSE IF NOT EXISTS(SELECT 1 FROM dbo.Aplicaciones WHERE IdUsuario=@IdUsuario AND Identificador=@Identificador)
                    INSERT dbo.Aplicaciones(IdUsuario,Nombre,Identificador,EsCritica,SyncId,SyncUpdatedUtc)
                    VALUES(@IdUsuario,@Nombre,@Identificador,@Critica,@SyncId,@Fecha)", cn, tx))
            {
                AgregarClave(cmd, c); Parametro(cmd, "@Nombre", SqlDbType.VarChar, 100, v[0]);
                Parametro(cmd, "@Identificador", SqlDbType.VarChar, 255, v[1]); Parametro(cmd, "@Critica", SqlDbType.Bit, Bool(v[2]));
                Parametro(cmd, "@Fecha", SqlDbType.DateTime2, c.CreadoUtc); cmd.ExecuteNonQuery();
            }
        }

        private static void AplicarWeb(SqlConnection cn, SqlTransaction tx, Cambio c, string[] v)
        {
            if (c.Operacion == "D") { EliminarEntidad(cn, tx, c, "Webs"); return; }
            using (var cmd = new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.SyncTombstones WHERE IdUsuario=@IdUsuario AND TipoEntidad='Web' AND SyncId=@SyncId AND EliminadoUtc>=@Fecha) RETURN;
                IF EXISTS(SELECT 1 FROM dbo.Webs WHERE IdUsuario=@IdUsuario AND SyncId=@SyncId)
                UPDATE dbo.Webs SET Nombre=@Nombre,Link=@Link,SyncUpdatedUtc=@Fecha
                WHERE IdUsuario=@IdUsuario AND SyncId=@SyncId AND SyncUpdatedUtc<=@Fecha
                ELSE IF NOT EXISTS(SELECT 1 FROM dbo.Webs WHERE IdUsuario=@IdUsuario AND HashLink=CONVERT(VARBINARY(32),HASHBYTES('SHA2_256',@Link)))
                    INSERT dbo.Webs(IdUsuario,Nombre,Link,SyncId,SyncUpdatedUtc)
                    VALUES(@IdUsuario,@Nombre,@Link,@SyncId,@Fecha)", cn, tx))
            {
                AgregarClave(cmd, c); Parametro(cmd, "@Nombre", SqlDbType.VarChar, 100, v[0]);
                Parametro(cmd, "@Link", SqlDbType.VarChar, 2048, v[1]);
                Parametro(cmd, "@Fecha", SqlDbType.DateTime2, c.CreadoUtc); cmd.ExecuteNonQuery();
            }
        }

        private static void AplicarDescanso(SqlConnection cn, SqlTransaction tx, Cambio c, string[] v)
        {
            if (c.Operacion == "D") { EliminarEntidad(cn, tx, c, "Descansos"); return; }
            int tareaLocal = Int32.Parse(v[0], CultureInfo.InvariantCulture);
            int tareaRemota = BuscarIdRemoto(cn, tx, c.IdUsuario, "Tareas", "IdTarea", tareaLocal);
            using (var cmd = new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.SyncTombstones WHERE IdUsuario=@IdUsuario AND TipoEntidad='Descanso' AND SyncId=@SyncId AND EliminadoUtc>=@Fecha) RETURN;
                IF EXISTS(SELECT 1 FROM dbo.Descansos WHERE IdUsuario=@IdUsuario AND SyncId=@SyncId)
                UPDATE dbo.Descansos SET IdTarea=@IdTarea,IntervaloDescanso=@Intervalo,DuracionDescanso=@Duracion,SyncUpdatedUtc=@Fecha
                WHERE IdUsuario=@IdUsuario AND SyncId=@SyncId AND SyncUpdatedUtc<=@Fecha
                ELSE IF EXISTS(SELECT 1 FROM dbo.Descansos WHERE IdUsuario=@IdUsuario AND IdTarea=@IdTarea)
                    UPDATE dbo.Descansos SET IntervaloDescanso=@Intervalo,DuracionDescanso=@Duracion,SyncUpdatedUtc=@Fecha
                    WHERE IdUsuario=@IdUsuario AND IdTarea=@IdTarea AND SyncUpdatedUtc<=@Fecha
                ELSE
                    INSERT dbo.Descansos(IdTarea,IdUsuario,IntervaloDescanso,DuracionDescanso,SyncId,SyncUpdatedUtc)
                    VALUES(@IdTarea,@IdUsuario,@Intervalo,@Duracion,@SyncId,@Fecha)", cn, tx))
            {
                AgregarClave(cmd, c); Parametro(cmd, "@IdTarea", SqlDbType.Int, tareaRemota);
                Parametro(cmd, "@Intervalo", SqlDbType.Int, IntNullable(v[1])); Parametro(cmd, "@Duracion", SqlDbType.Int, IntNullable(v[2]));
                Parametro(cmd, "@Fecha", SqlDbType.DateTime2, c.CreadoUtc); cmd.ExecuteNonQuery();
            }
        }

        private static void AplicarConfiguracion(SqlConnection cn, SqlTransaction tx, Cambio c, string[] v)
        {
            using (var cmd = new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.Configuraciones WHERE IdUsuario=@IdUsuario)
                UPDATE dbo.Configuraciones SET AutoSesion=@AutoSesion,SyncUpdatedUtc=@Fecha
                WHERE IdUsuario=@IdUsuario AND SyncUpdatedUtc<=@Fecha
                ELSE INSERT dbo.Configuraciones(IdUsuario,AutoSesion,SyncId,SyncUpdatedUtc) VALUES(@IdUsuario,@AutoSesion,@SyncId,@Fecha)", cn, tx))
            {
                AgregarClave(cmd, c); Parametro(cmd, "@AutoSesion", SqlDbType.Bit, Bool(v[0]));
                Parametro(cmd, "@Fecha", SqlDbType.DateTime2, c.CreadoUtc); cmd.ExecuteNonQuery();
            }
        }

        private static void AplicarRelacion(SqlConnection cn, SqlTransaction tx, Cambio c, string[] v, bool esApp)
        {
            int tarea = BuscarIdRemotoSync(cn, tx, c.IdUsuario, "Tareas", "IdTarea", v[0]);
            string tablaElemento = esApp ? "Aplicaciones" : "Webs";
            string campoElemento = esApp ? "IdAplicacion" : "IdWeb";
            int elemento = BuscarIdRemotoSync(cn, tx, c.IdUsuario, tablaElemento, campoElemento, v[1]);
            string tabla = esApp ? "Tarea_Aplicacion" : "Tarea_Web";
            if (c.Operacion == "D")
            {
                using (var cmd = new SqlCommand("DELETE FROM dbo." + tabla + " WHERE IdUsuario=@IdUsuario AND IdTarea=@IdTarea AND " + campoElemento + "=@IdElemento", cn, tx))
                {
                    Parametro(cmd, "@IdUsuario", SqlDbType.Int, c.IdUsuario); Parametro(cmd, "@IdTarea", SqlDbType.Int, tarea);
                    Parametro(cmd, "@IdElemento", SqlDbType.Int, elemento); cmd.ExecuteNonQuery();
                }
            }
            else
            {
                using (var cmd = new SqlCommand("IF NOT EXISTS(SELECT 1 FROM dbo." + tabla + " WHERE IdUsuario=@IdUsuario AND IdTarea=@IdTarea AND " + campoElemento + "=@IdElemento) INSERT dbo." + tabla + "(IdUsuario,IdTarea," + campoElemento + ") VALUES(@IdUsuario,@IdTarea,@IdElemento)", cn, tx))
                {
                    Parametro(cmd, "@IdUsuario", SqlDbType.Int, c.IdUsuario); Parametro(cmd, "@IdTarea", SqlDbType.Int, tarea);
                    Parametro(cmd, "@IdElemento", SqlDbType.Int, elemento); cmd.ExecuteNonQuery();
                }
            }
        }

        private static void EliminarEntidad(SqlConnection cn, SqlTransaction tx, Cambio c, string tabla)
        {
            using (var tomb = new SqlCommand(@"INSERT dbo.SyncTombstones(IdUsuario,TipoEntidad,SyncId,EliminadoUtc)
                SELECT @IdUsuario,@Tipo,@SyncId,@Fecha
                WHERE EXISTS(SELECT 1 FROM dbo." + tabla + @" WHERE IdUsuario=@IdUsuario AND SyncId=@SyncId AND SyncUpdatedUtc<=@Fecha)
                  AND NOT EXISTS(SELECT 1 FROM dbo.SyncTombstones WHERE IdUsuario=@IdUsuario AND TipoEntidad=@Tipo AND SyncId=@SyncId AND EliminadoUtc>=@Fecha)", cn, tx))
            {
                AgregarClave(tomb, c); Parametro(tomb, "@Tipo", SqlDbType.VarChar, 32, c.Tipo);
                Parametro(tomb, "@Fecha", SqlDbType.DateTime2, c.CreadoUtc); tomb.ExecuteNonQuery();
            }
            using (var cmd = new SqlCommand("DELETE FROM dbo." + tabla + " WHERE IdUsuario=@IdUsuario AND SyncId=@SyncId AND SyncUpdatedUtc<=@Fecha", cn, tx))
            {
                AgregarClave(cmd, c); Parametro(cmd, "@Fecha", SqlDbType.DateTime2, c.CreadoUtc); cmd.ExecuteNonQuery();
            }
        }

        private static int BuscarIdRemoto(SqlConnection cn, SqlTransaction tx, int idUsuario, string tabla, string campoId, int idLocal)
        {
            var sync = SQLiteLocal.Consultar("SELECT SyncId FROM " + tabla + " WHERE IdUsuario=? AND " + campoId + "=?",
                f => f.Texto("SyncId"), idUsuario, idLocal);
            if (sync.Count == 0) throw new InvalidOperationException("No se encontró el identificador de sincronización local.");
            return BuscarIdRemotoSync(cn, tx, idUsuario, tabla, campoId, sync[0]);
        }

        private static int BuscarIdRemotoSync(SqlConnection cn, SqlTransaction tx, int idUsuario, string tabla, string campoId, string syncId)
        {
            using (var cmd = new SqlCommand("SELECT " + campoId + " FROM dbo." + tabla + " WHERE IdUsuario=@IdUsuario AND SyncId=@SyncId", cn, tx))
            {
                Parametro(cmd, "@IdUsuario", SqlDbType.Int, idUsuario); Parametro(cmd, "@SyncId", SqlDbType.UniqueIdentifier, Guid.Parse(syncId));
                object id = cmd.ExecuteScalar();
                if (id == null || id == DBNull.Value) throw new InvalidOperationException("No se encontró el registro remoto relacionado.");
                return Convert.ToInt32(id, CultureInfo.InvariantCulture);
            }
        }

        private static void AgregarClave(SqlCommand cmd, Cambio c)
        {
            Parametro(cmd, "@IdUsuario", SqlDbType.Int, c.IdUsuario);
            Parametro(cmd, "@SyncId", SqlDbType.UniqueIdentifier, Guid.Parse(c.SyncId));
        }

        private static void Parametro(SqlCommand cmd, string nombre, SqlDbType tipo, object valor)
        {
            cmd.Parameters.Add(nombre, tipo).Value = valor ?? DBNull.Value;
        }

        private static void Parametro(SqlCommand cmd, string nombre, SqlDbType tipo, int longitud, object valor)
        {
            cmd.Parameters.Add(nombre, tipo, longitud).Value = valor ?? DBNull.Value;
        }

        private static string[] Decodificar(string payload)
        {
            if (String.IsNullOrEmpty(payload)) return new string[0];
            string[] partes = payload.Split('.');
            var valores = new string[partes.Length];
            for (int i = 0; i < partes.Length; i++)
                valores[i] = Encoding.UTF8.GetString(Convert.FromBase64String(partes[i]));
            return valores;
        }

        private static bool Bool(string valor)
        {
            return valor == "1" || String.Equals(valor, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static DateTime Fecha(string valor)
        {
            return DateTime.Parse(valor, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        private static int? IntNullable(string valor)
        {
            int n;
            return Int32.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out n) ? (int?)n : null;
        }

        private static void RegistrarErrorLocal(int idUsuario, string error)
        {
            // Leave the operation at the head of the queue; the next retry will
            // either apply it or report the error again without losing data.
            SQLiteLocal.Ejecutar(@"UPDATE SyncOutbox SET Intentos=Intentos+1,UltimoError=?
                WHERE IdUsuario=? AND IdCambio=(SELECT MIN(IdCambio) FROM SyncOutbox WHERE IdUsuario=?)",
                error ?? "Error desconocido", idUsuario, idUsuario);
        }
    }
}
