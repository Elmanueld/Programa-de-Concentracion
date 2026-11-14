using System;
using System.Collections.Generic;
using CapaEntidades;

namespace CapaDatos
{
    public class D_Web
    {
        public int InsertarWeb(int idUsuario, E_Web web)
        {
            if (String.IsNullOrEmpty(web.SyncId)) web.SyncId = Guid.NewGuid().ToString("D");
            return D_SincronizacionOffline.ConFallbackEscritura(idUsuario, () =>
            {
                object id = D_Comun.Escalar("sp_InsertarWeb", comando =>
                {
                    D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                    comando.Parameters.Add("@SyncId", System.Data.SqlDbType.UniqueIdentifier).Value = Guid.Parse(web.SyncId);
                    D_Comun.Texto(comando, "@Nombre", 100, web.Nombre);
                    D_Comun.Texto(comando, "@Link", 2048, web.Link);
                });
                web.IdWeb = System.Convert.ToInt32(id);
                web.IdUsuario = idUsuario;
                return web.IdWeb;
            }, () => D_SincronizacionOffline.InsertarWeb(idUsuario, web));
        }

        public List<E_Web> BuscarWeb(int idUsuario, string buscar)
        {
            return D_SincronizacionOffline.ConFallback(idUsuario, () => D_Comun.Consultar("sp_BuscarWeb", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Texto(comando, "@Buscar", 2048, buscar);
            }, lector => new E_Web
                {
                    IdWeb = D_Comun.LeerEntero(lector, "IdWeb"),
                    IdUsuario = idUsuario,
                    Nombre = D_Comun.LeerTexto(lector, "Nombre"),
                    Link = D_Comun.LeerTexto(lector, "Link")
                }), () => D_SincronizacionOffline.BuscarWebs(idUsuario, buscar));
        }

        public void ModificarWeb(int idUsuario, E_Web web)
        {
            D_SincronizacionOffline.ConFallbackEscritura(idUsuario, () => D_Comun.Ejecutar("sp_ModificarWeb", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Entero(comando, "@IdWeb", web.IdWeb);
                D_Comun.Texto(comando, "@Nombre", 100, web.Nombre);
                D_Comun.Texto(comando, "@Link", 2048, web.Link);
            }), () => D_SincronizacionOffline.ModificarWeb(idUsuario, web));
        }

        public void EliminarWeb(int idUsuario, int idWeb)
        {
            D_SincronizacionOffline.ConFallbackEscritura(idUsuario, () => D_Comun.Ejecutar("sp_EliminarWeb", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Entero(comando, "@IdWeb", idWeb);
            }), () => D_SincronizacionOffline.EliminarWeb(idUsuario, idWeb));
        }
    }
}
