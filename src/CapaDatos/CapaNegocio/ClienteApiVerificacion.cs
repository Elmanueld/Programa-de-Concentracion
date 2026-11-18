using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;

namespace CapaNegocio
{
    internal static class ClienteApiVerificacion
    {
        private static readonly HttpClient Cliente = CrearCliente();

        public static bool EstaConfigurado()
        {
            try { ObtenerBaseUri(); return true; }
            catch (InvalidOperationException) { return false; }
        }

        public static int RegistrarCuenta(string nombre, string correo, string hashContrasena)
        {
            using (HttpResponseMessage respuesta = Post("api/accounts/register", new
            {
                name = nombre,
                email = correo,
                passwordHash = hashContrasena
            }))
            {
                AsegurarExito(respuesta);
                var resultado = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(
                    respuesta.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                object id;
                if (resultado == null || !resultado.TryGetValue("idUsuario", out id))
                    throw new InvalidOperationException("La API no devolvió el identificador de la cuenta.");
                int idUsuario = Convert.ToInt32(id);
                if (idUsuario <= 0)
                    throw new InvalidOperationException("La API devolvió un identificador de cuenta no válido.");
                return idUsuario;
            }
        }

        public static void SolicitarCodigo(string correo)
        {
            using (HttpResponseMessage respuesta = Post("api/email-verification/send", new { email = correo }))
                AsegurarStatus(respuesta, HttpStatusCode.Accepted);
        }

        public static void ConfirmarCodigo(string correo, string codigo)
        {
            using (HttpResponseMessage respuesta = Post("api/email-verification/confirm", new
            {
                email = correo,
                code = codigo
            }))
                AsegurarStatus(respuesta, HttpStatusCode.NoContent);
        }

        public static void SolicitarCodigoRecuperacion(string correo)
        {
            using (HttpResponseMessage respuesta = Post("api/password-recovery/send", new { email = correo }))
                AsegurarStatus(respuesta, HttpStatusCode.Accepted);
        }

        public static void ConfirmarRecuperacion(string correo, string codigo, string hashContrasena)
        {
            using (HttpResponseMessage respuesta = Post("api/password-recovery/confirm", new
            {
                email = correo,
                code = codigo,
                passwordHash = hashContrasena
            }))
                AsegurarStatus(respuesta, HttpStatusCode.NoContent);
        }

        private static HttpClient CrearCliente()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            return new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        }

        private static HttpResponseMessage Post(string ruta, object solicitud)
        {
            string json = new JavaScriptSerializer().Serialize(solicitud);
            using (var contenido = new StringContent(json, Encoding.UTF8, "application/json"))
                return Cliente.PostAsync(new Uri(ObtenerBaseUri(), ruta), contenido)
                    .GetAwaiter().GetResult();
        }

        private static Uri ObtenerBaseUri()
        {
            string valor = Environment.GetEnvironmentVariable("PC_API_BASE_URL");
            if (String.IsNullOrWhiteSpace(valor))
                valor = ConfigurationManager.AppSettings["ApiBaseUrl"];
            Uri uri;
            if (!Uri.TryCreate(valor, UriKind.Absolute, out uri) || uri.Scheme != Uri.UriSchemeHttps)
                throw new InvalidOperationException("Configure una URL HTTPS válida en PC_API_BASE_URL o ApiBaseUrl.");
            string baseConBarra = uri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal)
                ? uri.AbsoluteUri
                : uri.AbsoluteUri + "/";
            return new Uri(baseConBarra, UriKind.Absolute);
        }

        private static void AsegurarExito(HttpResponseMessage respuesta)
        {
            if (!respuesta.IsSuccessStatusCode)
                throw CrearError(respuesta);
        }

        private static void AsegurarStatus(HttpResponseMessage respuesta, HttpStatusCode esperado)
        {
            if (respuesta.StatusCode != esperado)
                throw CrearError(respuesta);
        }

        private static Exception CrearError(HttpResponseMessage respuesta)
        {
            HttpStatusCode status = respuesta.StatusCode;
            switch (status)
            {
                case HttpStatusCode.Conflict:
                    return new ApiVerificacionException(status,
                        "Ya existe una cuenta con ese correo, o el registro anterior no coincide. Inicia sesión o usa «¿Olvidaste tu contraseña?».");
                case (HttpStatusCode)429:
                    return new ApiVerificacionException(status,
                        "La solicitud fue limitada. Espera un momento y vuelve a intentarlo.");
                case HttpStatusCode.ServiceUnavailable:
                    return new ApiVerificacionException(status,
                        "No se pudo completar la solicitud. Inténtalo de nuevo más tarde." +
                        FormatearReferencia(LeerReferencia(respuesta)));
                case HttpStatusCode.BadRequest:
                    return new ApiVerificacionException(status,
                        LeerMensajeSeguro(respuesta) ??
                        "Los datos enviados no son válidos o el código es incorrecto.");
                default:
                    string referencia = LeerReferencia(respuesta);
                    return new ApiVerificacionException(status,
                        "No se pudo completar la solicitud. Inténtalo más tarde o contacta con soporte." +
                        FormatearReferencia(referencia));
            }
        }

        private static string FormatearReferencia(string referencia)
        {
            return String.IsNullOrWhiteSpace(referencia) ? String.Empty : " Código de ayuda: " + referencia + ".";
        }

        private static string LeerMensajeSeguro(HttpResponseMessage respuesta)
        {
            try
            {
                string cuerpo = respuesta.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var datos = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(cuerpo);
                object mensaje;
                return datos != null && datos.TryGetValue("message", out mensaje)
                    ? Convert.ToString(mensaje)
                    : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("No se pudo leer el mensaje seguro del error remoto: " + ex.GetType().Name);
                return null;
            }
        }

        private static string LeerReferencia(HttpResponseMessage respuesta)
        {
            try
            {
                IEnumerable<string> referencias;
                if (respuesta.Headers.TryGetValues("X-Error-Reference", out referencias))
                    return referencias.FirstOrDefault();

                string cuerpo = respuesta.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var datos = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(cuerpo);
                object referencia;
                return datos != null && datos.TryGetValue("reference", out referencia)
                    ? Convert.ToString(referencia)
                    : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("No se pudo leer la referencia del error remoto: " + ex.GetType().Name);
                return null;
            }
        }
    }

    public sealed class ApiVerificacionException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public ApiVerificacionException(HttpStatusCode statusCode, string mensaje)
            : base(mensaje)
        {
            StatusCode = statusCode;
        }
    }
}
