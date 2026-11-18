using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class D_Tarea
    {
        public int InsertarTarea(E_Tarea tarea)
        {
            if (String.IsNullOrEmpty(tarea.SyncId)) tarea.SyncId = Guid.NewGuid().ToString("D");
            return D_SincronizacionOffline.ConFallbackEscritura(tarea.IdUsuario, () =>
            {
                object id = D_Comun.Escalar("sp_InsertarTarea", comando =>
                {
                    D_Comun.Entero(comando, "@IdUsuario", tarea.IdUsuario);
                    comando.Parameters.Add("@SyncId", SqlDbType.UniqueIdentifier).Value = Guid.Parse(tarea.SyncId);
                    D_Comun.Texto(comando, "@Nombre", 70, tarea.Nombre);
                    D_Comun.Fecha(comando, "@FechaInicio", tarea.FechaInicio);
                    D_Comun.Fecha(comando, "@FechaFin", tarea.FechaFin);
                    D_Comun.Booleano(comando, "@EsPlantilla", tarea.EsPlantilla);
                    D_Comun.Booleano(comando, "@NoBloquearApps", tarea.NoBloquearApps);
                    D_Comun.Booleano(comando, "@NoBloquearWebs", tarea.NoBloquearWebs);
                });
                tarea.IdTarea = Convert.ToInt32(id);
                return tarea.IdTarea;
            }, () => D_SincronizacionOffline.InsertarTarea(tarea));
        }

        public List<E_Tarea> BuscarTarea(int idUsuario, string buscar)
        {
            return D_SincronizacionOffline.ConFallback(idUsuario, () => D_Comun.Consultar("sp_BuscarTarea", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Texto(comando, "@Buscar", 70, buscar);
            }, lector => new E_Tarea
            {
                IdTarea = D_Comun.LeerEntero(lector, "IdTarea"),
                IdUsuario = idUsuario,
                Nombre = D_Comun.LeerTexto(lector, "Nombre"),
                FechaInicio = D_Comun.LeerFecha(lector, "FechaInicio"),
                FechaFin = D_Comun.LeerFecha(lector, "FechaFin"),
                Estado = D_Comun.LeerCaracter(lector, "Estado"),
                EsPlantilla = false,
                NoBloquearApps = D_Comun.LeerBooleano(lector, "NoBloquearApps"),
                NoBloquearWebs = D_Comun.LeerBooleano(lector, "NoBloquearWebs")
            }), () => D_SincronizacionOffline.BuscarTareas(idUsuario, buscar));
        }

        public void ModificarTarea(E_Tarea tarea)
        {
            D_SincronizacionOffline.ConFallbackEscritura(tarea.IdUsuario,
            () => EjecutarCambio("sp_ModificarTarea", tarea.IdUsuario, tarea.IdTarea, comando =>
            {
                D_Comun.Entero(comando, "@IdTarea", tarea.IdTarea);
                D_Comun.Texto(comando, "@Nombre", 70, tarea.Nombre);
                D_Comun.Fecha(comando, "@FechaInicio", tarea.FechaInicio);
                D_Comun.Fecha(comando, "@FechaFin", tarea.FechaFin);
                D_Comun.Booleano(comando, "@NoBloquearApps", tarea.NoBloquearApps);
                D_Comun.Booleano(comando, "@NoBloquearWebs", tarea.NoBloquearWebs);
            }), () => D_SincronizacionOffline.ModificarTarea(tarea));
        }

        public void EliminarTarea(int idUsuario, int idTarea)
        {
            D_SincronizacionOffline.ConFallbackEscritura(idUsuario,
            () => EjecutarCambio("sp_EliminarTarea", idUsuario, idTarea, comando => {
                D_Comun.Entero(comando, "@IdTarea", idTarea);
            }), () => D_SincronizacionOffline.EliminarTarea(idUsuario, idTarea));
        }

        public void CambiarEstadoTarea(int idUsuario, int idTarea, char estado)
        {
            D_SincronizacionOffline.ConFallbackEscritura(idUsuario,
            () => EjecutarCambio("sp_CambiarEstadoTarea", idUsuario, idTarea, comando =>
            {
                D_Comun.Entero(comando, "@IdTarea", idTarea);
                D_Comun.Caracter(comando, "@Estado", estado);
            }), () => D_SincronizacionOffline.CambiarEstadoTarea(idUsuario, idTarea, estado));
        }

        public int? BuscarPlantilla(int idUsuario)
        {
            return D_SincronizacionOffline.ConFallback(idUsuario, () =>
            {
            object valor = D_Comun.Escalar("sp_BuscarPlantilla", comando =>
                D_Comun.Entero(comando, "@IdUsuario", idUsuario));
            return D_Comun.EnteroNullable(valor);
            }, () => D_SincronizacionOffline.BuscarPlantilla(idUsuario));
        }

        public E_Tarea ObtenerPlantilla(int idUsuario)
        {
            return D_SincronizacionOffline.ConFallback(idUsuario, () =>
            {
                List<E_Tarea> resultados = D_Comun.Consultar("sp_ObtenerPlantilla", comando =>
                {
                    D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                }, lector => new E_Tarea
                {
                    IdTarea = D_Comun.LeerEntero(lector, "IdTarea"),
                    IdUsuario = idUsuario,
                    Nombre = D_Comun.LeerTexto(lector, "Nombre"),
                    FechaInicio = D_Comun.LeerFecha(lector, "FechaInicio"),
                    FechaFin = D_Comun.LeerFecha(lector, "FechaFin"),
                    Estado = D_Comun.LeerCaracter(lector, "Estado"),
                    EsPlantilla = true,
                    NoBloquearApps = D_Comun.LeerBooleano(lector, "NoBloquearApps"),
                    NoBloquearWebs = D_Comun.LeerBooleano(lector, "NoBloquearWebs"),
                    SyncId = D_Comun.LeerTexto(lector, "SyncId")
                });
                return resultados.Count == 0 ? null : resultados[0];
            }, () => D_SincronizacionOffline.ObtenerPlantilla(idUsuario));
        }

        private void EjecutarCambio(string procedimiento, int idUsuario, int idTarea, Action<SqlCommand> parametros)
        {
            D_Comun.Ejecutar(procedimiento, comando =>
            {
                parametros(comando);
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
            });
        }

    }
}
