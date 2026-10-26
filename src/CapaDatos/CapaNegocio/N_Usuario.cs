using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class N_Usuario
    {
        private readonly D_Usuario Datos = new D_Usuario();
        private int intentosFallidos;
        private DateTime bloqueadoHasta = DateTime.MinValue;

        public void RegistrarUsuario(string nombre, string correo, string contrasena)
        {
            var usuario = new E_Usuario { Nombre = (nombre ?? "").Trim(), Correo = (correo ?? "").Trim() };
            ValidarDatosUsuario(usuario, false);
            usuario.ContrasenaHash = SeguridadContrasena.CrearHash(contrasena);
            Datos.RegistrarUsuario(usuario);
        }

        public E_Usuario IniciarSesion(string correo, string contrasena)
        {
            if (DateTime.UtcNow < bloqueadoHasta)
                throw new ArgumentException("Demasiados intentos. Espere 30 segundos.");
            correo = (correo ?? "").Trim();
            ValidarCorreo(correo);
            var usuario = Datos.IniciarSesion(correo);
            if (usuario == null || !SeguridadContrasena.Verificar(contrasena, usuario.ContrasenaHash))
            {
                intentosFallidos++;
                if (intentosFallidos >= 5) { bloqueadoHasta = DateTime.UtcNow.AddSeconds(30); intentosFallidos = 0; }
                throw new ArgumentException("Credenciales incorrectas o cuenta pendiente de verificación.");
            }
            intentosFallidos = 0;
            usuario.ContrasenaHash = null;
            return usuario;
        }

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

            System.Net.Mail.MailAddress direccion;
            try { direccion = new System.Net.Mail.MailAddress(correo); }
            catch (FormatException) { throw new ArgumentException("El correo no tiene un formato válido."); }
            if (correo.Length > 254 || direccion.Address != correo || correo.IndexOf(' ') >= 0 ||
                correo.LastIndexOf('.') < correo.IndexOf('@') + 2)
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
