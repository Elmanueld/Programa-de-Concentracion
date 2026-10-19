using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class N_Web
    {
        private readonly D_Web Datos = new D_Web();

        public List<E_Web> BuscarWeb(string buscar)
        {
            return Datos.BuscarWeb(buscar ?? String.Empty);
        }

        public void InsertarWeb(E_Web web)
        {
            ValidarWeb(web);
            Datos.InsertarWeb(web);
        }

        public void ModificarWeb(E_Web web)
        {
            if (web == null) throw new ArgumentNullException("web");
            ValidarId(web.IdWeb);
            ValidarWeb(web);
            Datos.ModificarWeb(web);
        }

        public void EliminarWeb(int idWeb)
        {
            ValidarId(idWeb);
            Datos.EliminarWeb(idWeb);
        }

        private static void ValidarWeb(E_Web web)
        {
            if (web == null) throw new ArgumentNullException("web");
            if (String.IsNullOrWhiteSpace(web.Nombre))
                throw new ArgumentException("El nombre de la web es obligatorio.");
            if (web.Nombre.Length > 100)
                throw new ArgumentException("El nombre no puede exceder 100 caracteres.");
            if (String.IsNullOrWhiteSpace(web.Link))
                throw new ArgumentException("El enlace es obligatorio.");
            if (web.Link.Length > 2048)
                throw new ArgumentException("El enlace no puede exceder 2048 caracteres.");
        }

        private static void ValidarId(int id)
        {
            if (id <= 0) throw new ArgumentException("El ID de web no es válido.");
        }
    }
}