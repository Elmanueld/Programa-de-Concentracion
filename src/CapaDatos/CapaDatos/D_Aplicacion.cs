using System;
using System.Collections.Generic;
using CapaEntidades;

namespace CapaDatos
{
    public class D_Aplicacion
    {
        public int InsertarAplicacion(int idUsuario, E_Aplicacion aplicacion)
        {
            object id = D_Comun.Escalar("sp_InsertarAplicacion", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Texto(comando, "@Nombre", 100, aplicacion.Nombre);
                D_Comun.Texto(comando, "@Identificador", 255, aplicacion.Identificador);
                D_Comun.Booleano(comando, "@EsCritica", aplicacion.EsCritica);
            });

            return Convert.ToInt32(id);
        }

        public List<E_Aplicacion> BuscarAplicacion(int idUsuario, string buscar)
        {
            return D_Comun.Consultar("sp_BuscarAplicacion", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Texto(comando, "@Buscar", 255, buscar);
            }, lector => new E_Aplicacion
                {
                    IdAplicacion = D_Comun.LeerEntero(lector, "IdAplicacion"),
                    IdUsuario = idUsuario,
                    Nombre = D_Comun.LeerTexto(lector, "Nombre"),
                    Identificador = D_Comun.LeerTexto(lector, "Identificador"),
                    EsCritica = D_Comun.LeerBooleano(lector, "EsCritica")
                });
        }

        public void ModificarAplicacion(int idUsuario, E_Aplicacion aplicacion)
        {
            D_Comun.Ejecutar("sp_ModificarAplicacion", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Entero(comando, "@IdAplicacion", aplicacion.IdAplicacion);
                D_Comun.Texto(comando, "@Nombre", 100, aplicacion.Nombre);
                D_Comun.Texto(comando, "@Identificador", 255, aplicacion.Identificador);
            });
        }

        public void EliminarAplicacion(int idUsuario, int idAplicacion)
        {
            D_Comun.Ejecutar("sp_EliminarAplicacion", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Entero(comando, "@IdAplicacion", idAplicacion);
            });
        }
    }
}
