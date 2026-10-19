using System.Collections.Generic;
using CapaEntidades;

namespace CapaDatos
{
    public class D_Web
    {
        public void InsertarWeb(E_Web web)
        {
            D_Comun.Ejecutar("sp_InsertarWeb", comando =>
            {
                D_Comun.Texto(comando, "@Nombre", 100, web.Nombre);
                D_Comun.Texto(comando, "@Link", 2048, web.Link);
            });
        }

        public List<E_Web> BuscarWeb(string buscar)
        {
            return D_Comun.Consultar("sp_BuscarWeb", comando =>
                D_Comun.Texto(comando, "@Buscar", 2048, buscar), lector => new E_Web
                {
                    IdWeb = D_Comun.LeerEntero(lector, "IdWeb"),
                    Nombre = D_Comun.LeerTexto(lector, "Nombre"),
                    Link = D_Comun.LeerTexto(lector, "Link")
                });
        }

        public void ModificarWeb(E_Web web)
        {
            D_Comun.Ejecutar("sp_ModificarWeb", comando =>
            {
                D_Comun.Entero(comando, "@IdWeb", web.IdWeb);
                D_Comun.Texto(comando, "@Nombre", 100, web.Nombre);
                D_Comun.Texto(comando, "@Link", 2048, web.Link);
            });
        }

        public void EliminarWeb(int idWeb)
        {
            D_Comun.Ejecutar("sp_EliminarWeb", comando =>
                D_Comun.Entero(comando, "@IdWeb", idWeb));
        }
    }
}