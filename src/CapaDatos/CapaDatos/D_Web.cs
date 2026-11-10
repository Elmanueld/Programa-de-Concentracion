using System.Collections.Generic;
using CapaEntidades;

namespace CapaDatos
{
    public class D_Web
    {
        public int InsertarWeb(int idUsuario, E_Web web)
        {
            object id = D_Comun.Escalar("sp_InsertarWeb", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Texto(comando, "@Nombre", 100, web.Nombre);
                D_Comun.Texto(comando, "@Link", 2048, web.Link);
            });
            return System.Convert.ToInt32(id);
        }

        public List<E_Web> BuscarWeb(int idUsuario, string buscar)
        {
            return D_Comun.Consultar("sp_BuscarWeb", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Texto(comando, "@Buscar", 2048, buscar);
            }, lector => new E_Web
                {
                    IdWeb = D_Comun.LeerEntero(lector, "IdWeb"),
                    IdUsuario = idUsuario,
                    Nombre = D_Comun.LeerTexto(lector, "Nombre"),
                    Link = D_Comun.LeerTexto(lector, "Link")
                });
        }

        public void ModificarWeb(int idUsuario, E_Web web)
        {
            D_Comun.Ejecutar("sp_ModificarWeb", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Entero(comando, "@IdWeb", web.IdWeb);
                D_Comun.Texto(comando, "@Nombre", 100, web.Nombre);
                D_Comun.Texto(comando, "@Link", 2048, web.Link);
            });
        }

        public void EliminarWeb(int idUsuario, int idWeb)
        {
            D_Comun.Ejecutar("sp_EliminarWeb", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Entero(comando, "@IdWeb", idWeb);
            });
        }
    }
}
