using System.Collections.Generic;
using CapaEntidades;

namespace CapaDatos
{
    public class D_Configuracion
    {
        public void InsertarConfiguracion(E_Configuracion configuracion)
        {
            D_Comun.Ejecutar("sp_InsertarConfiguracion", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", configuracion.IdUsuario);
                D_Comun.Booleano(comando, "@AutoSesion", configuracion.AutoSesion);
            });
        }

        public E_Configuracion BuscarConfiguracion(int idUsuario)
        {
            List<E_Configuracion> resultados = D_Comun.Consultar("sp_BuscarConfiguracion", comando =>
                D_Comun.Entero(comando, "@IdUsuario", idUsuario), lector => new E_Configuracion
                {
                    IdConfiguracion = D_Comun.LeerEntero(lector, "IdConfiguracion"),
                    IdUsuario = D_Comun.LeerEntero(lector, "IdUsuario"),
                    AutoSesion = D_Comun.LeerBooleano(lector, "AutoSesion")
                });

            return resultados.Count == 0 ? null : resultados[0];
        }

        public void ModificarConfiguracion(E_Configuracion configuracion)
        {
            D_Comun.Ejecutar("sp_ModificarConfiguracion", comando =>
            {
                D_Comun.Entero(comando, "@IdConfiguracion", configuracion.IdConfiguracion);
                D_Comun.Booleano(comando, "@AutoSesion", configuracion.AutoSesion);
            });
        }
    }
}