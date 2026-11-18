using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CapaPresentacion
{
    internal static class ErroresAplicacion
    {
        private static readonly object Bloqueo = new object();
        private static string rutaRegistroActual;

        public static string Registrar(Exception excepcion, string origen)
        {
            string referencia = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpperInvariant();
            try
            {
                string carpeta = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Programa-de-Concentracion", "Logs");
                Directory.CreateDirectory(carpeta);
                string archivo = Path.Combine(carpeta, "errores.log");
                string detalle = Sanitizar(excepcion == null ? "Excepción desconocida." : excepcion.ToString());
                lock (Bloqueo)
                    File.AppendAllText(archivo,
                        "[" + DateTime.UtcNow.ToString("O") + "] [" + referencia + "] [" + origen + "]" +
                        Environment.NewLine + detalle + Environment.NewLine + Environment.NewLine,
                        Encoding.UTF8);
                rutaRegistroActual = archivo;
            }
            catch (Exception errorRegistro)
            {
                try
                {
                    string carpetaAlternativa = Path.Combine(Path.GetTempPath(),
                        "Programa-de-Concentracion", "Logs");
                    Directory.CreateDirectory(carpetaAlternativa);
                    string archivoAlternativo = Path.Combine(carpetaAlternativa, "errores.log");
                    string detalleAlternativo = Sanitizar(excepcion == null
                        ? "Excepción desconocida."
                        : excepcion.ToString());
                    lock (Bloqueo)
                        File.AppendAllText(archivoAlternativo,
                            "[" + DateTime.UtcNow.ToString("O") + "] [" + referencia + "] [" +
                            origen + "] [LOG LOCAL NO DISPONIBLE: " + errorRegistro.GetType().Name + "]" +
                            Environment.NewLine + detalleAlternativo + Environment.NewLine + Environment.NewLine,
                            Encoding.UTF8);
                    rutaRegistroActual = archivoAlternativo;
                }
                catch (Exception errorAlternativo)
                {
                    Trace.TraceError("No se pudo escribir el registro de errores. Local: " +
                        errorRegistro.GetType().Name + "; alternativo: " + errorAlternativo.GetType().Name);
                }
            }
            return referencia;
        }

        public static string RutaRegistro
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(rutaRegistroActual)) return rutaRegistroActual;
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Programa-de-Concentracion", "Logs", "errores.log");
            }
        }

        private static string Sanitizar(string detalle)
        {
            string[] valoresSecretos =
            {
                Environment.GetEnvironmentVariable("Smtp__Password"),
                Environment.GetEnvironmentVariable("Verification__HashKey")
            };

            foreach (string secreto in valoresSecretos)
                if (!String.IsNullOrEmpty(secreto))
                    detalle = detalle.Replace(secreto, "[REDACTADO]");

            detalle = Regex.Replace(detalle,
                @"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", "[CORREO REDACTADO]",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            detalle = Regex.Replace(detalle,
                @"(?<!\d)\d{6}(?!\d)", "[CÓDIGO REDACTADO]", RegexOptions.CultureInvariant);
            detalle = Regex.Replace(detalle,
                @"PBKDF2-SHA256\$[^$\s]+\$[^$\s]+\$[^\s,;\]}]+",
                "[HASH REDACTADO]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            detalle = Regex.Replace(detalle,
                @"(?i)(Bearer\s+)[A-Za-z0-9._~+/-]+=*", "$1[TOKEN REDACTADO]",
                RegexOptions.CultureInvariant);
            detalle = Regex.Replace(detalle,
                @"(?i)(passwordHash|hashContrasena|access_token|refresh_token|token|hashKey)\s*[=:]\s*[^\s,;\]}]+",
                "$1=[REDACTADO]", RegexOptions.CultureInvariant);

            return detalle;
        }
    }
}
