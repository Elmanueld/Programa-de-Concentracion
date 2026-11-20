using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class N_Usuario
    {
        private readonly D_Usuario Datos = new D_Usuario();
        private readonly D_RegistroPendiente RegistrosPendientes = new D_RegistroPendiente();
        private int intentosFallidos;
        private DateTime bloqueadoHasta = DateTime.MinValue;
        public Exception AdvertenciaCacheLocal { get; private set; }

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

        public static bool ConexionRemotaConfigurada
        {
            get { return EstadoApiRemota.EstaConfigurado; }
        }

        public E_Usuario IniciarSesionOffline(string correo, string contrasena)
        {
            correo = (correo ?? "").Trim();
            ValidarCorreo(correo);
            E_Usuario usuario = CredencialOfflineLocal.IniciarSesion(correo, contrasena);
            if (usuario == null || !D_SincronizacionOffline.TieneCopiaCompleta(usuario.IdUsuario))
                return null;

            D_SincronizacionOffline.ActivarModoOffline(usuario.IdUsuario);
            usuario.SesionSinConexion = true;
            usuario.ContrasenaHash = null;
            return usuario;
        }

        public void RegistrarUsuario(string nombre, string correo, string contrasena)
        {
            var usuario = new E_Usuario { Nombre = (nombre ?? "").Trim(), Correo = (correo ?? "").Trim() };
            ValidarDatosUsuario(usuario, false);
            if (ModoDemostracion)
            {
                usuario.ContrasenaHash = SeguridadContrasena.CrearHash(contrasena);
                Datos.RegistrarUsuario(usuario);
                return;
            }

            var pendiente = RegistrosPendientes.BuscarPorCorreo(usuario.Correo);
            if (pendiente != null)
            {
                string hashGuardado = DesprotegerHash(pendiente.HashProtegido);
                if (SeguridadContrasena.Verificar(contrasena, hashGuardado)) return;
                throw new InvalidOperationException("Ya existe un registro local pendiente con ese correo.");
            }

            string hash = SeguridadContrasena.CrearHash(contrasena);
            RegistrosPendientes.Crear(usuario.Correo, usuario.Nombre, ProtegerHash(hash));
        }

        public E_Usuario IniciarSesion(string correo, string contrasena)
        {
            AdvertenciaCacheLocal = null;
            if (DateTime.UtcNow < bloqueadoHasta)
                throw new ArgumentException("Demasiados intentos. Espere 30 segundos.");
            correo = (correo ?? "").Trim();
            ValidarCorreo(correo);
            E_Usuario usuario;
            try
            {
                usuario = Datos.IniciarSesion(correo, contrasena);
            }
            catch (ApiRemotaNoDisponibleException ex)
            {
                usuario = CredencialOfflineLocal.IniciarSesion(correo, contrasena);
                if (usuario != null && D_SincronizacionOffline.TieneCopiaCompleta(usuario.IdUsuario))
                {
                    D_SincronizacionOffline.ActivarModoOffline(usuario.IdUsuario);
                    return usuario;
                }
                throw new InvalidOperationException(
                    "No se pudo conectar con tu cuenta en línea y este equipo no tiene información suficiente para entrar sin conexión. Comprueba Internet e inténtalo de nuevo.", ex);
            }
            catch (ApiDatosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                intentosFallidos++;
                if (intentosFallidos >= 5)
                {
                    bloqueadoHasta = DateTime.UtcNow.AddSeconds(30);
                    intentosFallidos = 0;
                }
                throw new ArgumentException("Credenciales incorrectas o cuenta pendiente de verificación.");
            }
            if (usuario == null)
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
                catch (ApiRemotaNoDisponibleException)
                {
                    usuario.Nombre = String.Empty;
                }
            }
            intentosFallidos = 0;
            D_SincronizacionOffline.ActivarModoOnline();
            // Persist the newly authenticated account before cache capture or
            // outbox processing; either can fail independently of the login.
            try { CredencialOfflineLocal.Guardar(usuario, SeguridadContrasena.CrearHash(contrasena), contrasena); }
            catch (IOException ex) { AgregarAdvertenciaCacheLocal(ex); }
            catch (UnauthorizedAccessException ex) { AgregarAdvertenciaCacheLocal(ex); }
            catch (CryptographicException ex) { AgregarAdvertenciaCacheLocal(ex); }
            try
            {
                new D_SincronizacionOffline().CapturarCopiaCompleta(
                    usuario.IdUsuario, usuario.Correo, usuario.Nombre);
            }
            catch (ApiRemotaNoDisponibleException ex) { AgregarAdvertenciaCacheLocal(ex); }
            catch (SqlException ex) { AgregarAdvertenciaCacheLocal(ex); }
            catch (InvalidOperationException ex) { AgregarAdvertenciaCacheLocal(ex); }
            try { new D_ProcesadorSincronizacionOffline().Sincronizar(usuario.IdUsuario); }
            catch (ApiRemotaNoDisponibleException ex) { AgregarAdvertenciaCacheLocal(ex); }
            catch (SqlException ex) { AgregarAdvertenciaCacheLocal(ex); }
            catch (InvalidOperationException ex) { AgregarAdvertenciaCacheLocal(ex); }
            usuario.ContrasenaHash = null;
            return usuario;
        }

        public bool InicioAutomaticoConfiguradoLocalmente()
        {
            string contrasena;
            E_Usuario cuenta = CredencialOfflineLocal.ObtenerSesionRecordada(out contrasena);
            contrasena = null;
            if (cuenta == null)
            {
                System.Diagnostics.Trace.TraceInformation("Inicio automático omitido: no hay una sesión recordada en este equipo.");
                return false;
            }
            System.Diagnostics.Trace.TraceInformation(
                "Inicio automático: se evaluará la última cuenta guardada (IdUsuario=" + cuenta.IdUsuario + ").");
            if (CredencialOfflineLocal.InicioAutomaticoDeshabilitado(cuenta.IdUsuario))
            {
                System.Diagnostics.Trace.TraceInformation("Inicio automático omitido: está deshabilitado localmente para la cuenta recordada.");
                return false;
            }
            // En línea, la configuración puede estar en el servicio remoto aunque
            // este perfil aún no tenga una copia local completa. Solo se exige
            // la copia para poder iniciar sesión automáticamente sin conexión.
            if (!ConexionRemotaConfigurada &&
                !D_SincronizacionOffline.TieneCopiaCompleta(cuenta.IdUsuario))
            {
                System.Diagnostics.Trace.TraceInformation("Inicio automático omitido: falta la copia local completa y el servicio en línea no está configurado.");
                return false;
            }

            // En línea, no descartes el intento usando una copia local que puede
            // estar desactualizada. IniciarSesionAutomatico autentica esta única
            // cuenta recordada y luego valida AutoSesion en el servicio remoto.
            if (ConexionRemotaConfigurada)
            {
                System.Diagnostics.Trace.TraceInformation("Inicio automático: se verificará la preferencia remota de la cuenta recordada.");
                return true;
            }

            E_Configuracion configuracionLocal =
                D_SincronizacionOffline.BuscarConfiguracion(cuenta.IdUsuario);
            bool habilitado = configuracionLocal != null && configuracionLocal.AutoSesion;
            System.Diagnostics.Trace.TraceInformation("Inicio automático " +
                (habilitado ? "habilitado" : "omitido: la configuración local no está habilitada") +
                " (IdUsuario=" + cuenta.IdUsuario + ").");
            return habilitado;
        }

        public bool TieneCredencialRecordada(int idUsuario)
        {
            ValidarId(idUsuario);
            string contrasena;
            E_Usuario cuenta = CredencialOfflineLocal.ObtenerSesionRecordada(out contrasena);
            bool disponible = cuenta != null && cuenta.IdUsuario == idUsuario &&
                !String.IsNullOrEmpty(contrasena);
            contrasena = null;
            return disponible;
        }

        public E_Usuario IniciarSesionAutomatico()
        {
            string contrasena;
            E_Usuario cuenta = CredencialOfflineLocal.ObtenerSesionRecordada(out contrasena);
            if (cuenta == null) return null;
            if (CredencialOfflineLocal.InicioAutomaticoDeshabilitado(cuenta.IdUsuario)) return null;
            if (!ConexionRemotaConfigurada)
                return ObtenerSesionAutomaticaOffline(cuenta);

            if (String.IsNullOrEmpty(contrasena))
                return ObtenerSesionAutomaticaOffline(cuenta);
            try
            {
                E_Usuario remoto = Datos.IniciarSesion(cuenta.Correo, contrasena);
                if (remoto == null || remoto.IdUsuario != cuenta.IdUsuario)
                {
                    Datos.CerrarSesionRemota();
                    return null;
                }
                E_Configuracion configuracion = new N_Configuracion().BuscarConfiguracion(cuenta.IdUsuario);
                System.Diagnostics.Trace.TraceInformation("Inicio automático: AutoSesion=" +
                    (configuracion != null && configuracion.AutoSesion ? "true" : "false") +
                    " para IdUsuario=" + cuenta.IdUsuario + ".");
                if (configuracion == null || !configuracion.AutoSesion)
                {
                    Datos.CerrarSesionRemota();
                    return null;
                }
                remoto.Nombre = String.IsNullOrEmpty(remoto.Nombre) ? cuenta.Nombre : remoto.Nombre;
                remoto.ContrasenaHash = null;
                D_SincronizacionOffline.ActivarModoOnline();
                System.Diagnostics.Trace.TraceInformation("Inicio automático completado en línea.");
                return remoto;
            }
            catch (ApiDatosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                CredencialOfflineLocal.DeshabilitarInicioAutomatico(cuenta.IdUsuario);
                throw new InvalidOperationException(
                    "No se pudo iniciar sesión automáticamente. Inicia sesión con tu correo y contraseña para actualizar el acceso.", ex);
            }
            catch (ApiRemotaNoDisponibleException)
            {
                return ObtenerSesionAutomaticaOffline(cuenta);
            }
            finally { contrasena = null; }
        }

        private static E_Usuario ObtenerSesionAutomaticaOffline(E_Usuario cuenta)
        {
            if (!D_SincronizacionOffline.TieneCopiaCompleta(cuenta.IdUsuario))
            {
                System.Diagnostics.Trace.TraceInformation("Inicio automático sin conexión omitido: falta la copia local completa.");
                return null;
            }
            D_SincronizacionOffline.ActivarModoOffline(cuenta.IdUsuario);
            E_Configuracion local = new N_Configuracion().BuscarConfiguracion(cuenta.IdUsuario);
            if (local == null || !local.AutoSesion)
            {
                System.Diagnostics.Trace.TraceInformation("Inicio automático sin conexión omitido: la preferencia no está activa en la copia local.");
                return null;
            }
            cuenta.SesionSinConexion = true;
            System.Diagnostics.Trace.TraceInformation("Inicio automático completado sin conexión.");
            return cuenta;
        }

        public void CerrarSesion(int idUsuario)
        {
            ValidarId(idUsuario);
            CredencialOfflineLocal.DeshabilitarInicioAutomatico(idUsuario);
            Datos.CerrarSesionRemota();
        }

        public void HabilitarInicioAutomatico(int idUsuario)
        {
            ValidarId(idUsuario);
            CredencialOfflineLocal.HabilitarInicioAutomatico(idUsuario);
        }

        public void RegistrarUsuario(E_Usuario usuario)
        {
            ValidarDatosUsuario(usuario, true);
            var pendiente = RegistrosPendientes.BuscarPorCorreo(usuario.Correo);
            if (pendiente != null)
            {
                string hashGuardado = DesprotegerHash(pendiente.HashProtegido);
                if (String.Equals(usuario.ContrasenaHash, hashGuardado, StringComparison.Ordinal)) return;
                throw new InvalidOperationException("Ya existe un registro local pendiente con ese correo.");
            }

            RegistrosPendientes.Crear(usuario.Correo, usuario.Nombre, ProtegerHash(usuario.ContrasenaHash));
        }

        public bool HayRegistroPendiente(string correo)
        {
            return RegistrosPendientes.BuscarPorCorreo((correo ?? "").Trim()) != null;
        }

        internal int? PrepararRegistroRemoto(string correo)
        {
            correo = (correo ?? "").Trim();
            ValidarCorreo(correo);
            RegistroPendiente pendiente = RegistrosPendientes.BuscarPorCorreo(correo);
            if (pendiente == null) return null;

            int idRemoto = pendiente.IdUsuarioRemoto ?? ClienteApiVerificacion.RegistrarCuenta(
                pendiente.Nombre, pendiente.Correo, DesprotegerHash(pendiente.HashProtegido));
            if (!pendiente.IdUsuarioRemoto.HasValue)
                RegistrosPendientes.AsociarUsuarioRemoto(pendiente.IdUsuarioLocal, idRemoto);

            return pendiente.IdUsuarioLocal;
        }

        internal void CompletarRegistroRemoto(int idUsuarioLocal)
        {
            RegistrosPendientes.Eliminar(idUsuarioLocal);
        }

        private static string ProtegerHash(string hash)
        {
            byte[] contenido = Encoding.UTF8.GetBytes(hash);
            try
            {
                return Convert.ToBase64String(ProtectedData.Protect(
                    contenido, null, DataProtectionScope.CurrentUser));
            }
            finally { Array.Clear(contenido, 0, contenido.Length); }
        }

        private static string DesprotegerHash(string hashProtegido)
        {
            if (String.IsNullOrWhiteSpace(hashProtegido))
                throw new InvalidOperationException("El registro local protegido no está disponible en esta sesión de Windows.");
            byte[] protegido = Convert.FromBase64String(hashProtegido);
            byte[] contenido = ProtectedData.Unprotect(protegido, null, DataProtectionScope.CurrentUser);
            try { return Encoding.UTF8.GetString(contenido); }
            finally
            {
                Array.Clear(protegido, 0, protegido.Length);
                Array.Clear(contenido, 0, contenido.Length);
            }
        }

        public void ModificarUsuario(E_Usuario usuario)
        {
            AdvertenciaCacheLocal = null;
            ValidarId(usuario == null ? 0 : usuario.IdUsuario);
            ValidarDatosUsuario(usuario, false);
            Datos.ModificarUsuario(usuario);
            try { CredencialOfflineLocal.ActualizarCuenta(usuario); }
            catch (IOException ex) { AgregarAdvertenciaCacheLocal(ex); }
            catch (UnauthorizedAccessException ex) { AgregarAdvertenciaCacheLocal(ex); }
            catch (CryptographicException ex) { AgregarAdvertenciaCacheLocal(ex); }
        }

        public void EliminarUsuario(int idUsuario)
        {
            AdvertenciaCacheLocal = null;
            ValidarId(idUsuario);
            Datos.EliminarUsuario(idUsuario);
            try { CredencialOfflineLocal.Eliminar(idUsuario); }
            catch (IOException ex) { AgregarAdvertenciaCacheLocal(ex); }
            catch (UnauthorizedAccessException ex) { AgregarAdvertenciaCacheLocal(ex); }
        }

        public void VerificarCorreo(int idUsuario)
        {
            ValidarId(idUsuario);
            Datos.VerificarCorreo(idUsuario);
        }

        public void CambiarContrasena(int idUsuario, string contrasenaHashNueva, string contrasenaNueva)
        {
            AdvertenciaCacheLocal = null;
            ValidarId(idUsuario);
            ValidarHash(contrasenaHashNueva);
            Datos.CambiarContrasena(idUsuario, contrasenaHashNueva);
            try { CredencialOfflineLocal.ActualizarContrasena(idUsuario, contrasenaHashNueva, contrasenaNueva); }
            catch (IOException ex) { AgregarAdvertenciaCacheLocal(ex); }
            catch (UnauthorizedAccessException ex) { AgregarAdvertenciaCacheLocal(ex); }
            catch (CryptographicException ex) { AgregarAdvertenciaCacheLocal(ex); }
        }

        public void RecuperarCuenta(int idUsuario)
        {
            ValidarId(idUsuario);
            Datos.RecuperarCuenta(idUsuario);
        }

        private static void ValidarDatosUsuario(E_Usuario usuario, bool incluirHash)
        {
            if (usuario == null) throw new ArgumentNullException("usuario");
            if (usuario.Nombre != null)
                usuario.Nombre = usuario.Nombre.Trim().Normalize(NormalizationForm.FormC);
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

        private void AgregarAdvertenciaCacheLocal(Exception error)
        {
            AdvertenciaCacheLocal = AdvertenciaCacheLocal == null
                ? error
                : new AggregateException("Fallaron varias actualizaciones de la copia local.",
                    AdvertenciaCacheLocal, error);
        }

        private static void ValidarId(int id)
        {
            if (id <= 0) throw new ArgumentException("El ID de usuario no es válido.");
        }
    }
}
