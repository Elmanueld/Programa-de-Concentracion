using System;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class N_Configuracion
    {
        private readonly D_Configuracion Datos = new D_Configuracion();

        public void InsertarConfiguracion(E_Configuracion configuracion)
        {
            ValidarUsuario(configuracion);
            Datos.InsertarConfiguracion(configuracion);
        }

        public E_Configuracion BuscarConfiguracion(int idUsuario)
        {
            ValidarId(idUsuario, "usuario");
            return Datos.BuscarConfiguracion(idUsuario);
        }

        public void ModificarConfiguracion(E_Configuracion configuracion)
        {
            if (configuracion == null) throw new ArgumentNullException("configuracion");
            ValidarId(configuracion.IdUsuario, "usuario");
            // The local copy may not contain a server-created configuration yet
            // (for example, the first save after signing in on a different PC).
            // Treat that case as a per-user insert instead of attempting to update
            // a stale or missing local ID.
            E_Configuracion actual = Datos.BuscarConfiguracion(configuracion.IdUsuario);
            if (actual == null)
            {
                Datos.InsertarConfiguracion(configuracion);
                return;
            }
            configuracion.IdConfiguracion = actual.IdConfiguracion;
            Datos.ModificarConfiguracion(configuracion);
        }

        private static void ValidarUsuario(E_Configuracion configuracion)
        {
            if (configuracion == null) throw new ArgumentNullException("configuracion");
            ValidarId(configuracion.IdUsuario, "usuario");
        }

        private static void ValidarId(int id, string tipo)
        {
            if (id <= 0) throw new ArgumentException("El ID de " + tipo + " no es válido.");
        }
    }
}
