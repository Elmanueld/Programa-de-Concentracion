using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class N_Web
    {
        private readonly D_Web Datos = new D_Web();

        public List<E_Web> BuscarWeb(int idUsuario, string buscar)
        {
            ValidarIdUsuario(idUsuario);
            return Datos.BuscarWeb(idUsuario, buscar ?? String.Empty);
        }

        public int InsertarWeb(int idUsuario, E_Web web)
        {
            ValidarIdUsuario(idUsuario);
            ValidarWeb(web);
            ValidarPropietario(idUsuario, web.IdUsuario);
            web.IdUsuario = idUsuario;
            return Datos.InsertarWeb(idUsuario, web);
        }

        public void ModificarWeb(int idUsuario, E_Web web)
        {
            ValidarIdUsuario(idUsuario);
            if (web == null) throw new ArgumentNullException("web");
            ValidarPropietario(idUsuario, web.IdUsuario);
            ValidarId(web.IdWeb);
            ValidarWeb(web);
            Datos.ModificarWeb(idUsuario, web);
        }

        public void EliminarWeb(int idUsuario, int idWeb)
        {
            ValidarIdUsuario(idUsuario);
            ValidarId(idWeb);
            Datos.EliminarWeb(idUsuario, idWeb);
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

        private static void ValidarIdUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El ID de usuario no es válido.");
        }

        private static void ValidarPropietario(int idUsuario, int propietario)
        {
            if (propietario > 0 && propietario != idUsuario)
                throw new ArgumentException("La web pertenece a otro usuario.");
        }
    }
}
