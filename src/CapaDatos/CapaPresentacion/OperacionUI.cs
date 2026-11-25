using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaNegocio;

namespace CapaPresentacion
{
    internal static class OperacionUI
    {
        public static bool Ejecutar(Form ventana, Action accion)
        {
            ventana.UseWaitCursor = true;
            ventana.Enabled = false;
            try
            {
                accion();
                return true;
            }
            catch (Exception ex)
            {
                ReportarError(ventana, ex, "No se pudo completar la operación");
                return false;
            }
            finally
            {
                RestaurarEstado(ventana);
            }
        }

        public static async Task<bool> EjecutarAsync(Form ventana, Func<Task> accion)
        {
            ventana.UseWaitCursor = true;
            ventana.Enabled = false;
            try
            {
                await accion();
                return true;
            }
            catch (Exception ex)
            {
                if (!ventana.IsDisposed)
                    ReportarError(ventana, ex, "No se pudo completar la operación");
                else
                    ErroresAplicacion.Registrar(ex, "Operación asíncrona de una ventana cerrada");
                return false;
            }
            finally
            {
                RestaurarEstado(ventana);
            }
        }

        public static async Task EjecutarEventoAsync(Form ventana, Func<Task> accion, string titulo)
        {
            try
            {
                await accion();
            }
            catch (Exception ex)
            {
                if (ventana == null || ventana.IsDisposed || ventana.Disposing)
                    ErroresAplicacion.Registrar(ex, titulo ?? "Evento asíncrono de una ventana cerrada");
                else
                    ReportarError(ventana, ex, titulo ?? "No se pudo completar la operación");
            }
        }

        public static void ReportarError(Form ventana, Exception ex, string titulo)
        {
            ReportarError(ventana, ex, titulo, null, false);
        }

        public static void ReportarError(Form ventana, Exception ex, string titulo,
            string contextoRecuperacion, bool recuperable)
        {
            bool estadoPersistenciaIncierto = RequiereCierreControlado(ex);
            var causa = ObtenerCausa(ex);
            string referencia = ErroresAplicacion.Registrar(ex, titulo ?? "Operación de usuario");
            string mensaje = estadoPersistenciaIncierto
                ? "No se pudieron guardar los cambios de forma segura. La aplicación se cerrará para proteger tu información. Vuelve a abrirla e inténtalo otra vez."
                : MensajeParaUsuario(causa);
            if (!String.IsNullOrWhiteSpace(contextoRecuperacion))
                mensaje += Environment.NewLine + Environment.NewLine + contextoRecuperacion;
            if (!(causa is ArgumentException))
            {
                mensaje += Environment.NewLine + Environment.NewLine +
                    "Código de ayuda: " + referencia + ". Si necesitas soporte, compártelo con quien te ayude.";
            }

            MostrarMensajeSeguro(ventana, mensaje, titulo ?? "Programa de Concentración",
                recuperable || causa is ArgumentException ? MessageBoxIcon.Warning : MessageBoxIcon.Error);
            if (estadoPersistenciaIncierto)
            {
                Program.CierreControladoPorErrorCritico = true;
                Application.Exit();
            }
        }

        private static void MostrarMensajeSeguro(Form ventana, string mensaje, string titulo, MessageBoxIcon icono)
        {
            try
            {
                if (ventana != null)
                {
                    // A message box owned by a hidden/disposed form can become an
                    // unexpected visible application window while the app is in the tray.
                    if (ventana.IsDisposed || ventana.Disposing || !ventana.Visible) return;
                    MessageBox.Show(ventana, mensaje, titulo, MessageBoxButtons.OK, icono);
                }
                else
                    MessageBox.Show(mensaje, titulo, MessageBoxButtons.OK, icono);
            }
            catch (Exception error)
            {
                ErroresAplicacion.Registrar(error, "No se pudo mostrar el aviso de error");
                try { MessageBox.Show(mensaje, titulo, MessageBoxButtons.OK, MessageBoxIcon.Error); }
                catch { }
            }
        }

        private static void RestaurarEstado(Form ventana)
        {
            try
            {
                if (ventana == null || ventana.IsDisposed || ventana.Disposing) return;
                ventana.UseWaitCursor = false;
                ventana.Enabled = true;
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException ex)
            {
                ErroresAplicacion.Registrar(ex, "No se pudo restaurar el estado de la ventana");
            }
        }

        private static string MensajeParaUsuario(Exception excepcion)
        {
            var errorApi = ApiErrorDatos.Desde(excepcion);
            if (errorApi != null)
            {
                string referenciaApi = String.IsNullOrWhiteSpace(errorApi.Referencia)
                    ? String.Empty
                    : " Código de ayuda: " + errorApi.Referencia + ".";
                switch (errorApi.CodigoHttp)
                {
                    case 400:
                        return "No se pudieron validar los datos. Revísalos e inténtalo de nuevo." + referenciaApi;
                    case 401:
                        return "No se pudo iniciar sesión automáticamente. Escribe tu correo y contraseña para continuar.";
                    case 403:
                        return "Tu cuenta no puede realizar esta acción. Comprueba que hayas iniciado sesión con la cuenta correcta." + referenciaApi;
                    case 404:
                        return "Esta función no está disponible ahora. Inténtalo más tarde o contacta con soporte." + referenciaApi;
                    case 409:
                        return String.IsNullOrWhiteSpace(errorApi.Mensaje)
                            ? "La información cambió y no se pudo completar la acción. Actualízala e inténtalo de nuevo." + referenciaApi
                            : errorApi.Mensaje + referenciaApi;
                    case 429:
                        return "Has realizado varias solicitudes seguidas. Espera un momento y vuelve a intentarlo." + referenciaApi;
                    default:
                        if (errorApi.CodigoHttp >= 500)
                            return "No se pudo completar la acción por un problema del servicio. Inténtalo más tarde." + referenciaApi;
                        return "No se pudo completar la acción. Revísala e inténtalo de nuevo." + referenciaApi;
                }
            }

            var sql = excepcion as SqlException;
            if (sql != null)
            {
                if (sql.Number == 8144)
                    return "No se pudo completar esta acción. Inténtalo más tarde o contacta con soporte.";
                if (sql.Number == 2601 || sql.Number == 2627)
                    return "Ya existe un registro con esos datos. Comprueba el correo o inicia sesión con la cuenta existente.";
                if (sql.Number == -2 || sql.Number == 2 || sql.Number == 53 || sql.Number == 64 ||
                    sql.Number == 121 || sql.Number == 258 || sql.Number == 10053 || sql.Number == 10054 ||
                    sql.Number == 10060 || sql.Number == 11001 || sql.Number == 4060)
                    return "No se pudo conectar con tu cuenta en línea. Comprueba Internet e inténtalo de nuevo.";
                if (sql.Number == 50001)
                    return "Esta cuenta no está disponible para realizar la acción.";
                if (sql.Number == 50002 || sql.Number == 50003)
                    return "La tarea ya no está disponible o no se puede modificar en su estado actual.";
                if (sql.Number >= 50001 && sql.Number <= 50019)
                    return "No se pudo completar esta acción. Revisa la información e inténtalo de nuevo.";
                return "No se pudo completar la acción por un problema al guardar la información. Inténtalo más tarde.";
            }

            if (excepcion is CapaNegocio.ApiVerificacionException apiError)
                return ContieneDetalleTecnico(apiError.Message)
                    ? "No se pudo completar la solicitud. Inténtalo de nuevo más tarde."
                    : apiError.Message;
            if (excepcion is TaskCanceledException || excepcion is TimeoutException)
                return "La solicitud tardó demasiado. Comprueba Internet y vuelve a intentarlo.";
            if (excepcion is HttpRequestException || excepcion is WebException || excepcion is SocketException)
                return "No se pudo conectar con el servicio en línea. Comprueba Internet y vuelve a intentarlo.";
            if (excepcion is SmtpException)
                return "No se pudo enviar el correo. Comprueba la dirección e inténtalo más tarde.";
            if (excepcion is DllNotFoundException || excepcion is EntryPointNotFoundException)
                return "No se pudo acceder a la información guardada en este equipo. Reinicia la aplicación y, si continúa, contacta con soporte.";
            if (excepcion is UnauthorizedAccessException)
                return "Windows denegó el acceso necesario. Comprueba los permisos de la cuenta.";
            if (excepcion is Win32Exception errorWindows)
                return "Windows no pudo completar la acción. Comprueba los permisos e inténtalo de nuevo.";
            if (excepcion is CryptographicException)
                return "Windows no pudo leer los datos protegidos de esta cuenta. Usa la misma cuenta de Windows con la que se guardaron.";
            if (excepcion is IOException)
            {
                var archivo = excepcion as FileNotFoundException;
                if (archivo != null && !String.IsNullOrWhiteSpace(archivo.FileName))
                    return "No se encontró el archivo requerido «" + Path.GetFileName(archivo.FileName) +
                        ". Comprueba que siga instalado o vuelve a instalar la aplicación.";
                return "No se pudo leer o guardar un archivo local. Comprueba el espacio disponible y los permisos.";
            }
            if (excepcion is ArgumentException || excepcion is InvalidOperationException)
            {
                if (ContieneDetalleTecnico(excepcion.Message))
                    return "No se pudo completar la acción con la información actual. Revísala e inténtalo de nuevo.";
                if (excepcion is InvalidOperationException &&
                    (excepcion.Message.IndexOf("SQLite", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     excepcion.Message.IndexOf("base offline", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     excepcion.Message.IndexOf("esquema offline", StringComparison.OrdinalIgnoreCase) >= 0))
                    return "No se pudo acceder a la información guardada en este equipo. Comprueba el espacio disponible y vuelve a intentarlo.";
                return String.IsNullOrWhiteSpace(excepcion.Message)
                    ? "Los datos o el estado actual no permiten completar la operación."
                    : excepcion.Message;
            }
            if (excepcion is FormatException)
                return "Uno de los datos tiene un formato incorrecto. Revísalo e inténtalo de nuevo.";

            return "Ocurrió un problema y no se pudo completar la acción. La aplicación seguirá abierta; inténtalo otra vez.";
        }

        internal static string DescribirError(Exception excepcion)
        {
            return MensajeParaUsuario(ObtenerCausa(excepcion));
        }

        private static bool ContieneDetalleTecnico(string mensaje)
        {
            if (String.IsNullOrWhiteSpace(mensaje)) return false;
            string[] terminos = { "SQLite", "SQL Server", "ConnectionStrings", "ApiBaseUrl", "Verification__",
                "Smtp__", "GUID", "procedure", "procedimiento", "HTTP", "endpoint", "backend", "SqlException",
                "PBKDF2", "DPAPI", "HMAC", "stack trace", "inner exception", "JSON", "token", "assembly" };
            return terminos.Any(termino => mensaje.IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        internal static bool RequiereCierreControlado(Exception excepcion)
        {
            if (excepcion == null) return false;
            if (excepcion.Message.StartsWith(
                "Falló la operación local y SQLite tampoco pudo confirmar su reversión",
                StringComparison.OrdinalIgnoreCase)) return true;
            var agregado = excepcion as AggregateException;
            return agregado != null && agregado.InnerExceptions.Any(RequiereCierreControlado);
        }

        private static Exception ObtenerCausa(Exception excepcion)
        {
            while (excepcion is AggregateException aggregate && aggregate.InnerExceptions.Count > 0)
                excepcion = aggregate.InnerExceptions[0];
            return excepcion;
        }
    }
}
