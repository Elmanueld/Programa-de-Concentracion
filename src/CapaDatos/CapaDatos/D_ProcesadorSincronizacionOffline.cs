using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaDatos
{
    /// <summary>Uploads the durable SQLite outbox to the authenticated remote API.</summary>
    public sealed class D_ProcesadorSincronizacionOffline
    {
        private sealed class Cambio
        {
            public long Id;
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
            var cambios = SQLiteLocal.Consultar(@"SELECT IdCambio,TipoEntidad,SyncId,Operacion,Payload,CreadoUtc
                FROM SyncOutbox WHERE IdUsuario=? ORDER BY IdCambio",
                f => new Cambio
                {
                    Id = Convert.ToInt64(f["IdCambio"], CultureInfo.InvariantCulture),
                    Tipo = f.Texto("TipoEntidad"), SyncId = f.Texto("SyncId"),
                    Operacion = f.Texto("Operacion"), Payload = f.Texto("Payload"), CreadoUtc = f.Fecha("CreadoUtc")
                }, idUsuario);
            if (cambios.Count == 0)
            {
                try
                {
                    D_SincronizacionOffline.CapturarCopiaCompletaDesdeCache(idUsuario);
                    D_SincronizacionOffline.ActivarModoOnline();
                    return true;
                }
                catch (ApiRemotaNoDisponibleException) { return false; }
            }

            Guid dispositivo = ObtenerIdDispositivo();
            try
            {
                ClienteApiDatos.SincronizarCambios(idUsuario, dispositivo, cambios.Select(c => new CambioOfflineApi
                {
                    Id = c.Id, Type = c.Tipo, SyncId = c.SyncId, Operation = c.Operacion,
                    Payload = c.Payload, CreatedUtc = c.CreadoUtc,
                    RelatedSyncIds = BuscarReferencias(c, idUsuario)
                }).ToList());
                foreach (Cambio cambio in cambios)
                    SQLiteLocal.Ejecutar("DELETE FROM SyncOutbox WHERE IdUsuario=? AND IdCambio=?", idUsuario, cambio.Id);
                D_SincronizacionOffline.CapturarCopiaCompletaDesdeCache(idUsuario);
                D_SincronizacionOffline.ActivarModoOnline();
                var callback = SincronizacionCompletada;
                if (callback != null) callback(idUsuario);
                return true;
            }
            catch (ApiRemotaNoDisponibleException) { return false; }
            catch (Exception ex)
            {
                try
                {
                    SQLiteLocal.Ejecutar(@"UPDATE SyncOutbox SET Intentos=Intentos+1,UltimoError=?
                        WHERE IdUsuario=? AND IdCambio=(SELECT MIN(IdCambio) FROM SyncOutbox WHERE IdUsuario=?)",
                        "Error de sincronización (" + ex.GetType().Name + ").", idUsuario, idUsuario);
                }
                catch (Exception localError)
                {
                    throw new AggregateException("Falló la sincronización y no se pudo guardar el estado del error local.", ex, localError);
                }
                throw;
            }
        }

        private static Guid ObtenerIdDispositivo()
        {
            var rows = SQLiteLocal.Consultar("SELECT Valor FROM MetadatosLocal WHERE Clave='IdDispositivo'", f => f.Texto("Valor"));
            Guid id;
            if (rows.Count > 0 && Guid.TryParse(rows[0], out id)) return id;
            id = Guid.NewGuid();
            SQLiteLocal.Ejecutar("INSERT OR REPLACE INTO MetadatosLocal(Clave,Valor) VALUES('IdDispositivo',?)", id.ToString("D"));
            return id;
        }

        private static string[] BuscarReferencias(Cambio cambio, int idUsuario)
        {
            string[] valores = String.IsNullOrEmpty(cambio.Payload) ? new string[0] : cambio.Payload.Split('.')
                .Select(v => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(v))).ToArray();
            if (cambio.Tipo == "Descanso" && valores.Length > 0)
                return BuscarSyncId("Tareas", "IdTarea", valores[0], idUsuario);
            if ((cambio.Tipo == "TareaAplicacion" || cambio.Tipo == "TareaWeb") && valores.Length > 1)
            {
                string tablaElemento = cambio.Tipo == "TareaAplicacion" ? "Aplicaciones" : "Webs";
                string campoElemento = cambio.Tipo == "TareaAplicacion" ? "IdAplicacion" : "IdWeb";
                return BuscarSyncId("Tareas", "IdTarea", valores[0], idUsuario)
                    .Concat(BuscarSyncId(tablaElemento, campoElemento, valores[1], idUsuario)).ToArray();
            }
            return new string[0];
        }

        private static string[] BuscarSyncId(string tabla, string campoId, string idLocal, int idUsuario)
        {
            var resultado = SQLiteLocal.Consultar("SELECT SyncId FROM " + tabla + " WHERE IdUsuario=? AND " + campoId + "=?",
                f => f.Texto("SyncId"), idUsuario, Convert.ToInt32(idLocal, CultureInfo.InvariantCulture));
            if (resultado.Count == 0) throw new InvalidOperationException("Falta una referencia local necesaria para sincronizar.");
            return resultado.ToArray();
        }
    }
}
