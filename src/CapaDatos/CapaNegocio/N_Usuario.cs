using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class N_Usuario
    {
        private readonly D_Usuario Datos = new D_Usuario();

        public List<E_Usuario> BuscarUsuario(string buscar)
        {
            return Datos.BuscarUsuario(buscar ?? String.Empty);
        }

        public void RegistrarUsuario(E_Usuario usuario)
        {
            ValidarDatosUsuario(usuario, true);
            Datos.RegistrarUsuario(usuario);
        }

        public void ModificarUsuario(E_Usuario usuario)
        {
            ValidarId(usuario == null ? 0 : usuario.IdUsuario);
            ValidarDatosUsuario(usuario, false);
            Datos.ModificarUsuario(usuario);
        }

        public void EliminarUsuario(int idUsuario)
        {
            ValidarId(idUsuario);
            Datos.EliminarUsuario(idUsuario);
        }

        public void VerificarCorreo(int idUsuario)
        {
            ValidarId(idUsuario);
            Datos.VerificarCorreo(idUsuario);
        }

        public void CambiarContrasena(int idUsuario, string contrasenaHashNueva)
        {
            ValidarId(idUsuario);
            ValidarHash(contrasenaHashNueva);
            Datos.CambiarContrasena(idUsuario, contrasenaHashNueva);
        }

        public void RecuperarCuenta(int idUsuario)
        {
            ValidarId(idUsuario);
            Datos.RecuperarCuenta(idUsuario);
        }

        private static void ValidarDatosUsuario(E_Usuario usuario, bool incluirHash)
        {
            if (usuario == null) throw new ArgumentNullException("usuario");
            if (String.IsNullOrWhiteSpace(usuario.Nombre))
                throw new ArgumentException("El nombre es obligatorio.");
            if (usuario.Nombre.Length > 50)
                throw new ArgumentException("El nombre no puede exceder 50 caracteres.");
            ValidarCorreo(usuario.Correo);
            if (incluirHash) ValidarHash(usuario.ContrasenaHash);
        }

        private static void ValidarCorreo(string correo)
        {
            if (String.IsNullOrWhiteSpace(correo))
                throw new ArgumentException("El correo es obligatorio.");

            int arroba = correo.IndexOf('@');
            int punto = correo.LastIndexOf('.');
            if (correo.Length > 254 || arroba <= 0 || punto <= arroba + 1 || punto == correo.Length - 1)
                throw new ArgumentException("El correo no tiene un formato válido.");
        }

        private static void ValidarHash(string hash)
        {
            if (String.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("Debe proporcionar el hash de la contraseña.");
            if (hash.Length > 255)
                throw new ArgumentException("El hash no puede exceder 255 caracteres.");
        }

        private static void ValidarId(int id)
        {
            if (id <= 0) throw new ArgumentException("El ID de usuario no es válido.");
        }
    }
}