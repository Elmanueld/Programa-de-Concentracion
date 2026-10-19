using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class N_Aplicacion
    {
        private readonly D_Aplicacion Datos = new D_Aplicacion();

        public List<E_Aplicacion> BuscarAplicacion(string buscar)
        {
            return Datos.BuscarAplicacion(buscar ?? String.Empty);
        }

        public void InsertarAplicacion(E_Aplicacion aplicacion)
        {
            ValidarAplicacion(aplicacion);
            Datos.InsertarAplicacion(aplicacion);
        }

        public void ModificarAplicacion(E_Aplicacion aplicacion)
        {
            if (aplicacion == null) throw new ArgumentNullException("aplicacion");
            ValidarId(aplicacion.IdAplicacion);
            ValidarAplicacion(aplicacion);
            Datos.ModificarAplicacion(aplicacion);
        }

        public void EliminarAplicacion(int idAplicacion)
        {
            ValidarId(idAplicacion);
            Datos.EliminarAplicacion(idAplicacion);
        }

        private static void ValidarAplicacion(E_Aplicacion aplicacion)
        {
            if (aplicacion == null) throw new ArgumentNullException("aplicacion");
            if (String.IsNullOrWhiteSpace(aplicacion.Nombre))
                throw new ArgumentException("El nombre de la aplicación es obligatorio.");
            if (aplicacion.Nombre.Length > 100)
                throw new ArgumentException("El nombre no puede exceder 100 caracteres.");
            if (String.IsNullOrWhiteSpace(aplicacion.Identificador))
                throw new ArgumentException("El identificador de la aplicación es obligatorio.");
            if (aplicacion.Identificador.Length > 255)
                throw new ArgumentException("El identificador no puede exceder 255 caracteres.");
        }

        private static void ValidarId(int id)
        {
            if (id <= 0) throw new ArgumentException("El ID de aplicación no es válido.");
        }
    }
}