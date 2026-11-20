using System.Collections.Generic;
using CapaEntidades;

namespace CapaDatos
{
    public class D_Configuracion
    {
        public void InsertarConfiguracion(E_Configuracion configuracion)
        {
            D_SincronizacionOffline.ConFallbackEscritura(configuracion.IdUsuario, () => D_Comun.Ejecutar("sp_InsertarConfiguracion", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", configuracion.IdUsuario);
                D_Comun.Booleano(comando, "@AutoSesion", configuracion.AutoSesion);
            }), () => D_SincronizacionOffline.InsertarConfiguracion(configuracion));
            D_SincronizacionOffline.ReflejarConfiguracionLocal(configuracion);
        }

        public E_Configuracion BuscarConfiguracion(int idUsuario)
        {
            return D_SincronizacionOffline.ConFallback(idUsuario, () =>
            {
                List<E_Configuracion> resultados = D_Comun.Consultar("sp_BuscarConfiguracion", comando =>
                    D_Comun.Entero(comando, "@IdUsuario", idUsuario), lector => new E_Configuracion
                    {
                        IdConfiguracion = D_Comun.LeerEntero(lector, "IdConfiguracion"),
                        IdUsuario = D_Comun.LeerEntero(lector, "IdUsuario"),
                        AutoSesion = D_Comun.LeerBooleano(lector, "AutoSesion")
                    });

                E_Configuracion configuracion = resultados.Count == 0 ? null : resultados[0];
                // A local change that has not reached the service is the latest
                // preference for this account. Do not hide it behind an older
                // remote snapshot while the outbox is waiting to sync.
                if (D_SincronizacionOffline.TieneConfiguracionPendiente(idUsuario))
                {
                    E_Configuracion pendiente = D_SincronizacionOffline.BuscarConfiguracion(idUsuario);
                    if (pendiente != null)
                    {
                        System.Diagnostics.Trace.TraceInformation(
                            "AutoSesion leído de un cambio local pendiente (IdUsuario=" + idUsuario + ").");
                        return pendiente;
                    }
                }
                if (configuracion != null)
                    D_SincronizacionOffline.ReflejarConfiguracionLocal(configuracion);
                System.Diagnostics.Trace.TraceInformation("AutoSesion remoto " +
                    (configuracion != null && configuracion.AutoSesion ? "activo" : "inactivo") +
                    " (IdUsuario=" + idUsuario + ").");
                return configuracion;
            }, () => D_SincronizacionOffline.BuscarConfiguracion(idUsuario));
        }

        public void ModificarConfiguracion(E_Configuracion configuracion)
        {
            // A pending offline insert/update uses a local ID (often negative).
            // Never send that ID to the remote stored procedure. Keep the newest
            // value in this user's row and outbox; sync will upsert by IdUsuario.
            if (D_SincronizacionOffline.TieneConfiguracionPendiente(configuracion.IdUsuario))
            {
                GuardarConfiguracionLocal(configuracion);
                return;
            }

            try
            {
                D_SincronizacionOffline.ConFallbackEscritura(configuracion.IdUsuario, () => D_Comun.Ejecutar("sp_ModificarConfiguracion", comando =>
                {
                    D_Comun.Entero(comando, "@IdUsuario", configuracion.IdUsuario);
                    D_Comun.Entero(comando, "@IdConfiguracion", configuracion.IdConfiguracion);
                    D_Comun.Booleano(comando, "@AutoSesion", configuracion.AutoSesion);
                }), () => D_SincronizacionOffline.ModificarConfiguracion(configuracion));
                D_SincronizacionOffline.ReflejarConfiguracionLocal(configuracion);
            }
            catch (ApiDatosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                // A conflict from the legacy stored procedure must not discard a
                // per-user preference update. Queue it by SyncId/IdUsuario; the
                // sync endpoint upserts Configuraciones by the authenticated user.
                GuardarConfiguracionLocal(configuracion);
            }
        }

        private static void GuardarConfiguracionLocal(E_Configuracion configuracion)
        {
            E_Configuracion local = D_SincronizacionOffline.BuscarConfiguracion(configuracion.IdUsuario);
            if (local == null)
            {
                D_SincronizacionOffline.InsertarConfiguracion(configuracion);
            }
            else
            {
                configuracion.IdConfiguracion = local.IdConfiguracion;
                configuracion.SyncId = local.SyncId;
                D_SincronizacionOffline.ModificarConfiguracion(configuracion);
            }
            System.Diagnostics.Trace.TraceInformation(
                "AutoSesion guardado en la cola local (IdUsuario=" + configuracion.IdUsuario + ").");
        }
    }
}
