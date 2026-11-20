using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CapaDatos
{
    internal static class ClienteApiDatos
    {
        private static readonly HttpClient Cliente = CrearCliente();
        private static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = 10 * 1024 * 1024 };
        private static string token;
        private static DateTime tokenValidoHastaUtc;

        public static bool EstaConfigurado()
        {
            try { ObtenerBaseUri(); return true; }
            catch (InvalidOperationException) { return false; }
        }

        public static void CerrarSesion() { token = null; tokenValidoHastaUtc = DateTime.MinValue; }
        public static bool TieneSesionValida => !String.IsNullOrWhiteSpace(token) && tokenValidoHastaUtc > DateTime.UtcNow.AddMinutes(1);

        public static string BuscarNombreUsuario(int idUsuario)
        {
            string contenido = Enviar("api/accounts/me", null, true, HttpMethod.Get);
            var datos = Json.Deserialize<Dictionary<string, object>>(contenido);
            return Texto(datos, "name");
        }

        public static DataSet ObtenerCopiaCompleta()
        {
            string contenido = Enviar("api/sync/snapshot", null, true, HttpMethod.Get);
            var resultado = Json.Deserialize<Dictionary<string, object>>(contenido);
            var conjunto = new DataSet();
            foreach (var par in resultado)
            {
                var filas = par.Value as object[] ?? new object[0];
                var tabla = new DataTable(par.Key);
                var nombres = new List<string>();
                foreach (var fila in filas)
                    if (fila is Dictionary<string, object> registro)
                        foreach (var nombre in registro.Keys)
                            if (!nombres.Contains(nombre, StringComparer.OrdinalIgnoreCase))
                            {
                                nombres.Add(nombre);
                                tabla.Columns.Add(nombre, typeof(object));
                            }
                foreach (var fila in filas)
                {
                    var registro = fila as Dictionary<string, object>;
                    if (registro == null) continue;
                    var row = tabla.NewRow();
                    foreach (var campo in registro)
                        if (tabla.Columns.Contains(campo.Key)) row[campo.Key] = campo.Value ?? DBNull.Value;
                    tabla.Rows.Add(row);
                }
                conjunto.Tables.Add(tabla);
            }
            return conjunto;
        }

        public static void SincronizarCambios(int idUsuario, Guid dispositivo, IList<CambioOfflineApi> cambios)
        {
            Enviar("api/sync/changes", new
            {
                deviceId = dispositivo,
                changes = cambios.Select(c => new
                {
                    id = c.Id, type = c.Type, syncId = c.SyncId, operation = c.Operation,
                    payload = c.Payload, createdUtc = c.CreatedUtc.ToString("o", CultureInfo.InvariantCulture),
                    relatedSyncIds = c.RelatedSyncIds
                }).ToList()
            }, true, HttpMethod.Post);
        }

        public static E_UsuarioRespuesta IniciarSesion(string correo, string contrasena, string hashGuardado = null)
        {
            CerrarSesion();
            var solicitud = new Dictionary<string, object>
            {
                ["email"] = correo,
                ["password"] = hashGuardado == null ? contrasena : null,
                ["passwordHash"] = hashGuardado
            };
            string cuerpo = Enviar("api/accounts/login", solicitud, false, HttpMethod.Post);
            var datos = Json.Deserialize<Dictionary<string, object>>(cuerpo);
            token = Texto(datos, "accessToken");
            tokenValidoHastaUtc = DateTime.UtcNow.AddHours(12);
            int id = Convert.ToInt32(datos["idUsuario"], CultureInfo.InvariantCulture);
            return new E_UsuarioRespuesta
            {
                IdUsuario = id,
                Nombre = Texto(datos, "name"),
                Correo = Texto(datos, "email")
            };
        }

        public static void Ejecutar(string procedimiento, SqlCommand comando)
        {
            EnviarProcedimiento(procedimiento, comando, "execute");
        }

        public static object Escalar(string procedimiento, SqlCommand comando)
        {
            string contenido = EnviarProcedimiento(procedimiento, comando, "scalar");
            return String.IsNullOrWhiteSpace(contenido) ? null : Json.DeserializeObject(contenido);
        }

        public static DataTable Consultar(string procedimiento, SqlCommand comando)
        {
            string contenido = EnviarProcedimiento(procedimiento, comando, "query");
            var filas = Json.DeserializeObject(contenido) as object[] ?? new object[0];
            var tabla = new DataTable();
            var columnas = new List<string>();
            foreach (var fila in filas)
            {
                var registro = fila as Dictionary<string, object>;
                if (registro == null) continue;
                foreach (string columna in registro.Keys)
                    if (!columnas.Contains(columna, StringComparer.OrdinalIgnoreCase))
                    {
                        columnas.Add(columna);
                        tabla.Columns.Add(columna, typeof(object));
                    }
            }
            foreach (var fila in filas)
            {
                var registro = fila as Dictionary<string, object>;
                if (registro == null) continue;
                var row = tabla.NewRow();
                foreach (var par in registro)
                    if (tabla.Columns.Contains(par.Key)) row[par.Key] = par.Value ?? DBNull.Value;
                tabla.Rows.Add(row);
            }
            return tabla;
        }

        private static string EnviarProcedimiento(string procedimiento, SqlCommand comando, string modo)
        {
            var parametros = new Dictionary<string, object>();
            foreach (SqlParameter parameter in comando.Parameters)
                parametros[parameter.ParameterName] = ValorJson(parameter.Value);
            return Enviar("api/data/procedure", new
            {
                name = procedimiento,
                mode = modo,
                parameters = parametros
            }, true, HttpMethod.Post);
        }

        private static object ValorJson(object valor)
        {
            if (valor == null || valor == DBNull.Value) return null;
            if (valor is DateTime fecha) return fecha.ToString("o", CultureInfo.InvariantCulture);
            if (valor is DateTimeOffset fechaOffset) return fechaOffset.ToString("o", CultureInfo.InvariantCulture);
            if (valor is Guid guid) return guid.ToString("D");
            if (valor is char caracter) return caracter.ToString();
            if (valor is byte[] bytes) return Convert.ToBase64String(bytes);
            return valor;
        }

        private static string Enviar(string ruta, object solicitud, bool requiereSesion, HttpMethod metodo)
        {
            try
            {
                var mensaje = new HttpRequestMessage(metodo, new Uri(ObtenerBaseUri(), ruta));
                if (solicitud != null)
                    mensaje.Content = new StringContent(Json.Serialize(solicitud), Encoding.UTF8, "application/json");
                if (requiereSesion && !String.IsNullOrWhiteSpace(token))
                    mensaje.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                using (mensaje)
                using (var respuesta = Cliente.SendAsync(mensaje).GetAwaiter().GetResult())
                {
                    string contenido = respuesta.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (respuesta.StatusCode == HttpStatusCode.ServiceUnavailable ||
                        respuesta.StatusCode == HttpStatusCode.BadGateway ||
                        respuesta.StatusCode == HttpStatusCode.GatewayTimeout)
                        throw new ApiRemotaNoDisponibleException(LeerMensaje(contenido));
                    if (!respuesta.IsSuccessStatusCode)
                    {
                        string tipoContenido = respuesta.Content.Headers.ContentType == null
                            ? String.Empty
                            : respuesta.Content.Headers.ContentType.MediaType ?? String.Empty;
                        bool respuestaJson = EsContenidoJson(tipoContenido) && PareceJson(contenido);

                        // This API never returns an HTML 403. Hosting firewalls
                        // can return one before the request reaches the API; treat
                        // that as remote unavailability so the per-user offline
                        // snapshot/outbox can keep the app usable.
                        if (respuesta.StatusCode == HttpStatusCode.Forbidden && !respuestaJson)
                            throw new ApiRemotaNoDisponibleException(
                                "El servidor de alojamiento rechazó temporalmente la solicitud (HTTP 403, respuesta no JSON). Se intentará usar la copia local.");

                        string referencia = null;
                        IEnumerable<string> valores;
                        if (respuesta.Headers.TryGetValues("X-Error-Reference", out valores))
                            referencia = valores.FirstOrDefault();
                        if (respuestaJson && String.IsNullOrWhiteSpace(referencia))
                        {
                            try
                            {
                                var cuerpo = Json.Deserialize<Dictionary<string, object>>(contenido);
                                if (cuerpo != null && cuerpo.ContainsKey("reference"))
                                    referencia = Convert.ToString(cuerpo["reference"], CultureInfo.InvariantCulture);
                            }
                            catch (Exception) { }
                        }
                        string mensajeErrorApi = respuestaJson
                            ? LeerMensaje(contenido)
                            : "El servidor remoto respondió con HTTP " + (int)respuesta.StatusCode +
                                " y un contenido que no corresponde a la API.";
                        throw new ApiDatosException(respuesta.StatusCode, mensajeErrorApi, referencia);
                    }
                    return contenido;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException ||
                ex is UriFormatException || ex is SocketException || ex is AuthenticationException)
            {
                throw new ApiRemotaNoDisponibleException("No se pudo conectar con el servidor remoto. Comprueba Internet e inténtalo de nuevo.", ex);
            }
        }

        private static string LeerMensaje(string contenido)
        {
            try
            {
                var respuesta = Json.Deserialize<Dictionary<string, object>>(contenido);
                return respuesta != null && respuesta.ContainsKey("message")
                    ? Convert.ToString(respuesta["message"], CultureInfo.InvariantCulture)
                    : "El servicio remoto no pudo completar la operación.";
            }
            catch { return "El servicio remoto no pudo completar la operación."; }
        }

        private static bool EsContenidoJson(string tipoContenido)
        {
            return !String.IsNullOrWhiteSpace(tipoContenido) &&
                (tipoContenido.Equals("application/json", StringComparison.OrdinalIgnoreCase) ||
                 tipoContenido.EndsWith("+json", StringComparison.OrdinalIgnoreCase));
        }

        private static bool PareceJson(string contenido)
        {
            if (String.IsNullOrWhiteSpace(contenido)) return false;
            int indice = 0;
            if (contenido.Length > 0 && contenido[0] == '\uFEFF') indice++;
            while (indice < contenido.Length && Char.IsWhiteSpace(contenido[indice])) indice++;
            return indice < contenido.Length &&
                (contenido[indice] == '{' || contenido[indice] == '[');
        }

        private static string Texto(IDictionary<string, object> datos, string clave) =>
            datos != null && datos.ContainsKey(clave) ? Convert.ToString(datos[clave], CultureInfo.InvariantCulture) : null;

        private static Uri ObtenerBaseUri()
        {
            string valor = Environment.GetEnvironmentVariable("PC_API_BASE_URL");
            if (String.IsNullOrWhiteSpace(valor)) valor = System.Configuration.ConfigurationManager.AppSettings["ApiBaseUrl"];
            if (!Uri.TryCreate(valor, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                throw new InvalidOperationException("Configure una URL HTTPS válida en PC_API_BASE_URL o ApiBaseUrl.");
            return new Uri(uri.AbsoluteUri.TrimEnd('/') + "/", UriKind.Absolute);
        }

        private static HttpClient CrearCliente()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            // The web blocker changes the Windows system proxy to its local
            // filter. App-to-API requests must bypass it to avoid self-blocking.
            return new HttpClient(new HttpClientHandler { UseProxy = false })
            {
                Timeout = TimeSpan.FromSeconds(8)
            };
        }
    }

    public static class EstadoApiRemota
    {
        public static bool EstaConfigurado { get { return ClienteApiDatos.EstaConfigurado(); } }
        public static bool TieneSesionValida { get { return ClienteApiDatos.TieneSesionValida; } }
    }

    internal sealed class CambioOfflineApi
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string SyncId { get; set; }
        public string Operation { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedUtc { get; set; }
        public string[] RelatedSyncIds { get; set; }
    }

    internal sealed class E_UsuarioRespuesta
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
    }

    public sealed class ApiDatosException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }
        public string Referencia { get; private set; }
        public ApiDatosException(HttpStatusCode status, string mensaje, string referencia = null) : base(mensaje)
        {
            StatusCode = status;
            Referencia = referencia;
        }

        public override string ToString()
        {
            string detalle = base.ToString() + Environment.NewLine + "HTTP status: " + (int)StatusCode;
            if (!String.IsNullOrWhiteSpace(Referencia))
                detalle += Environment.NewLine + "Remote reference: " + Referencia;
            return detalle;
        }
    }

    public sealed class ApiRemotaNoDisponibleException : Exception
    {
        public ApiRemotaNoDisponibleException(string mensaje, Exception interior = null) : base(mensaje, interior) { }
    }
}
