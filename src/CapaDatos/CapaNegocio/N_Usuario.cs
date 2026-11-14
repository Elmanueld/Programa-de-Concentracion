using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class N_Usuario
    {
        private readonly D_Usuario Datos = new D_Usuario();
        private int intentosFallidos;
        private DateTime bloqueadoHasta = DateTime.MinValue;

        public static bool ModoDemostracion
        {
            get
            {
                bool activo;
                string valor = Environment.GetEnvironmentVariable("PC_MODO_DEMOSTRACION") ??
                    ConfigurationManager.AppSettings["ModoDemostracion"];
                return Boolean.TryParse(valor, out activo) && activo;
            }
        }

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
            E_Usuario usuario;
            try
            {
                usuario = Datos.IniciarSesion(correo, ModoDemostracion);
            }
            catch (SqlException ex)
            {
                if (!EsFalloDeConexion(ex)) throw;
                usuario = CredencialOfflineLocal.IniciarSesion(correo, contrasena);
                if (usuario != null && D_SincronizacionOffline.TieneCopiaCompleta(usuario.IdUsuario))
                {
                    D_SincronizacionOffline.ActivarModoOffline(usuario.IdUsuario);
                    return usuario;
                }
                throw new InvalidOperationException(
                    "No hay conexión con la base de datos y este equipo no tiene una copia offline completa de la cuenta.", ex);
            }
            if (usuario == null || !SeguridadContrasena.Verificar(contrasena, usuario.ContrasenaHash))
            {
                intentosFallidos++;
                if (intentosFallidos >= 5)
                {
                    bloqueadoHasta = DateTime.UtcNow.AddSeconds(30);
                    intentosFallidos = 0;
                }
                throw new ArgumentException("Credenciales incorrectas o cuenta pendiente de verificación.");
            }
            if (String.IsNullOrEmpty(usuario.Nombre))
            {
                try { usuario.Nombre = Datos.BuscarNombreUsuario(usuario.IdUsuario); }
                catch (SqlException ex)
                {
                    if (!EsFalloDeConexion(ex)) throw;
                    usuario.Nombre = String.Empty;
                }
            }
            intentosFallidos = 0;
            D_SincronizacionOffline.ActivarModoOnline();
            try
            {
                new D_SincronizacionOffline().CapturarCopiaCompleta(
                    usuario.IdUsuario, usuario.Correo, usuario.Nombre);
            }
            catch (SqlException) { }
            catch (InvalidOperationException) { }
            try { new D_ProcesadorSincronizacionOffline().Sincronizar(usuario.IdUsuario); }
            catch (SqlException) { }
            catch (InvalidOperationException) { }
            try { CredencialOfflineLocal.Guardar(usuario, usuario.ContrasenaHash); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            catch (CryptographicException) { }
            usuario.ContrasenaHash = null;
            return usuario;
        }

        public E_Usuario IniciarSesionAutomatico()
        {
            string hashGuardado;
            E_Usuario cuenta = CredencialOfflineLocal.ObtenerSesionRecordada(out hashGuardado);
            if (cuenta == null || String.IsNullOrWhiteSpace(hashGuardado)) return null;
            if (CredencialOfflineLocal.InicioAutomaticoDeshabilitado(cuenta.IdUsuario)) return null;

            E_Configuracion configuracion;
            try
            {
                configuracion = new N_Configuracion().BuscarConfiguracion(cuenta.IdUsuario);
            }
            catch (SqlException ex)
            {
                if (!EsFalloDeConexion(ex) || !D_SincronizacionOffline.TieneCopiaCompleta(cuenta.IdUsuario)) return null;
                D_SincronizacionOffline.ActivarModoOffline(cuenta.IdUsuario);
                configuracion = new N_Configuracion().BuscarConfiguracion(cuenta.IdUsuario);
            }
            if (configuracion == null || !configuracion.AutoSesion) return null;

            try
            {
                E_Usuario remoto = Datos.IniciarSesion(cuenta.Correo, ModoDemostracion);
                if (remoto == null || remoto.IdUsuario != cuenta.IdUsuario ||
                    !String.Equals(remoto.ContrasenaHash, hashGuardado, StringComparison.Ordinal)) return null;
                remoto.Nombre = String.IsNullOrEmpty(remoto.Nombre) ? cuenta.Nombre : remoto.Nombre;
                remoto.ContrasenaHash = null;
                D_SincronizacionOffline.ActivarModoOnline();
                return remoto;
            }
            catch (SqlException ex)
            {
                if (!EsFalloDeConexion(ex) || !D_SincronizacionOffline.TieneCopiaCompleta(cuenta.IdUsuario)) return null;
                D_SincronizacionOffline.ActivarModoOffline(cuenta.IdUsuario);
                E_Configuracion local = new N_Configuracion().BuscarConfiguracion(cuenta.IdUsuario);
                if (local == null || !local.AutoSesion) return null;
                cuenta.SesionSinConexion = true;
                return cuenta;
            }
        }

        public void CerrarSesion(int idUsuario)
        {
            ValidarId(idUsuario);
            CredencialOfflineLocal.DeshabilitarInicioAutomatico(idUsuario);
        }

        private static bool EsFalloDeConexion(SqlException excepcion)
        {
            foreach (SqlError error in excepcion.Errors)
            {
                switch (error.Number)
                {
                    case -2: case 2: case 53: case 64: case 121: case 233:
                    case 258: case 10053: case 10054: case 10060: case 11001:
                    case 4060:
                        return true;
                }
            }
            return false;
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
            try { CredencialOfflineLocal.ActualizarCuenta(usuario); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            catch (CryptographicException) { }
        }

        public void EliminarUsuario(int idUsuario)
        {
            ValidarId(idUsuario);
            Datos.EliminarUsuario(idUsuario);
            try { CredencialOfflineLocal.Eliminar(idUsuario); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
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
            try { CredencialOfflineLocal.ActualizarContrasena(idUsuario, contrasenaHashNueva); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            catch (CryptographicException) { }
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

        internal static void ValidarCorreo(string correo)
        {
            if (String.IsNullOrWhiteSpace(correo))
                throw new ArgumentException("El correo es obligatorio.");

            System.Net.Mail.MailAddress direccion;
            try
            {
                direccion = new System.Net.Mail.MailAddress(correo);
            }
            catch (FormatException)
            {
                throw new ArgumentException("El correo no tiene un formato válido.");
            }
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
