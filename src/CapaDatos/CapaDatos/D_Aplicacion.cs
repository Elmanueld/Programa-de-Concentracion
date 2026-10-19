using System.Collections.Generic;
using CapaEntidades;

namespace CapaDatos
{
    public class D_Aplicacion
    {
        public void InsertarAplicacion(E_Aplicacion aplicacion)
        {
            D_Comun.Ejecutar("sp_InsertarAplicacion", comando =>
            {
                D_Comun.Texto(comando, "@Nombre", 100, aplicacion.Nombre);
                D_Comun.Texto(comando, "@Identificador", 255, aplicacion.Identificador);
                D_Comun.Booleano(comando, "@EsCritica", aplicacion.EsCritica);
            });
        }

        public List<E_Aplicacion> BuscarAplicacion(string buscar)
        {
            return D_Comun.Consultar("sp_BuscarAplicacion", comando =>
                D_Comun.Texto(comando, "@Buscar", 255, buscar), lector => new E_Aplicacion
                {
                    IdAplicacion = D_Comun.LeerEntero(lector, "IdAplicacion"),
                    Nombre = D_Comun.LeerTexto(lector, "Nombre"),
                    Identificador = D_Comun.LeerTexto(lector, "Identificador"),
                    EsCritica = D_Comun.LeerBooleano(lector, "EsCritica")
                });
        }

        public void ModificarAplicacion(E_Aplicacion aplicacion)
        {
            D_Comun.Ejecutar("sp_ModificarAplicacion", comando =>
            {
                D_Comun.Entero(comando, "@IdAplicacion", aplicacion.IdAplicacion);
                D_Comun.Texto(comando, "@Nombre", 100, aplicacion.Nombre);
                D_Comun.Texto(comando, "@Identificador", 255, aplicacion.Identificador);
            });
        }

        public void EliminarAplicacion(int idAplicacion)
        {
            D_Comun.Ejecutar("sp_EliminarAplicacion", comando =>
                D_Comun.Entero(comando, "@IdAplicacion", idAplicacion));
        }
    }
}