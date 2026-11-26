using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace CapaPresentacion
{
    /// <summary>Consulta las publicaciones oficiales del repositorio y ofrece instalar la más reciente.</summary>
    internal static class ActualizadorGithub
    {
        private const string ApiUltimaPublicacion =
            "https://api.github.com/repos/Elmanueld/Programa-de-Concentracion/releases/latest";
        private const string NombrePaquetePortable = "ProgramaConcentracion-Portatil.zip";
        private static int comprobacionIniciada;

        static ActualizadorGithub()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        public static async void ComprobarYOfrecer(IWin32Window propietario)
        {
            if (Interlocked.Exchange(ref comprobacionIniciada, 1) != 0) return;

            try
            {
                Publicacion publicada = await Task.Run(ConsultarUltimaPublicacion);
                if (publicada == null || propietario == null ||
                    (propietario is Form formulario && (formulario.IsDisposed || !formulario.IsHandleCreated)))
                    return;

                string mensaje = "Hay una versión nueva del Programa de Concentración (" + publicada.Version + ").\r\n\r\n" +
                    "¿Quieres instalarla ahora? El programa se cerrará y volverá a abrirse al terminar la actualización.";
                Trace.TraceInformation("Hay una actualización disponible: {0}.", publicada.Version);
                DialogResult respuesta = MostrarPreguntaActualizacion(propietario, mensaje);
                Trace.TraceInformation("Respuesta a la oferta de actualización: {0}.", respuesta);
                if (respuesta != DialogResult.Yes) return;

                string carpeta = PrepararPaquetePortable(publicada);
                string script = Path.Combine(carpeta, "ActualizarInstalacion.ps1");
                if (!File.Exists(script) || !File.Exists(Path.Combine(carpeta, "ProgramaConcentracion.exe")))
                    throw new InvalidDataException("El paquete de actualización está incompleto.");

                string argumentos = "-NoLogo -NoProfile -ExecutionPolicy Bypass -File \"" + script +
                    "\" -Origen \"" + carpeta + "\" -Destino \"" + Application.StartupPath +
                    "\" -Ejecutable \"" + Application.ExecutablePath + "\" -Reiniciar";

                var inicio = new ProcessStartInfo
                {
                    FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                        "WindowsPowerShell\\v1.0\\powershell.exe"),
                    Arguments = argumentos,
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = carpeta
                };

                Process.Start(inicio);
                Program.SalidaSolicitada = true;
                Application.Exit();
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                MessageBox.Show(propietario,
                    "No se autorizó la actualización. El programa seguirá abierto.",
                    "Actualización cancelada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ErroresAplicacion.Registrar(ex, "Comprobación o descarga de actualización desde GitHub");
                if (propietario is Form formulario && (formulario.IsDisposed || !formulario.IsHandleCreated)) return;
                MessageBox.Show(propietario,
                    "No se pudo preparar la actualización. El programa seguirá abierto; puedes intentarlo de nuevo más tarde.",
                    "No se pudo actualizar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static DialogResult MostrarPreguntaActualizacion(IWin32Window propietario, string mensaje)
        {
            using (var dialogo = new Form())
            {
                dialogo.Text = "Actualización disponible";
                dialogo.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialogo.StartPosition = FormStartPosition.CenterScreen;
                dialogo.ClientSize = new Size(470, 190);
                dialogo.MaximizeBox = false;
                dialogo.MinimizeBox = false;
                dialogo.ShowInTaskbar = false;
                dialogo.TopMost = true;
                dialogo.Font = new Font("Segoe UI", 10F);

                var texto = new Label
                {
                    AutoSize = false,
                    Location = new Point(22, 20),
                    Size = new Size(426, 100),
                    Text = mensaje,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                var actualizar = new Button
                {
                    DialogResult = DialogResult.Yes,
                    Location = new Point(230, 135),
                    Size = new Size(135, 34),
                    Text = "Actualizar ahora",
                    UseVisualStyleBackColor = true
                };
                var despues = new Button
                {
                    DialogResult = DialogResult.No,
                    Location = new Point(374, 135),
                    Size = new Size(74, 34),
                    Text = "Después",
                    UseVisualStyleBackColor = true
                };
                dialogo.Controls.Add(texto);
                dialogo.Controls.Add(actualizar);
                dialogo.Controls.Add(despues);
                dialogo.AcceptButton = actualizar;
                dialogo.CancelButton = despues;

                Form formularioPropietario = propietario as Form;
                if (formularioPropietario != null && !formularioPropietario.IsDisposed &&
                    formularioPropietario.Visible && formularioPropietario.WindowState != FormWindowState.Minimized &&
                    formularioPropietario.ShowInTaskbar)
                    return dialogo.ShowDialog(formularioPropietario);

                // Un diálogo independiente y siempre visible evita que la oferta
                // quede detrás de una ventana oculta en la bandeja.
                return dialogo.ShowDialog();
            }
        }

        private static Publicacion ConsultarUltimaPublicacion()
        {
            try
            {
                string json;
                var solicitud = (HttpWebRequest)WebRequest.Create(ApiUltimaPublicacion);
                solicitud.Method = "GET";
                // The web blocker installs a local Windows proxy; updater traffic
                // must reach GitHub directly or version checks fail silently.
                solicitud.Proxy = null;
                solicitud.Accept = "application/vnd.github+json";
                solicitud.UserAgent = "Programa-de-Concentracion-Updater/1.0";
                solicitud.Timeout = 7000;
                solicitud.ReadWriteTimeout = 7000;
                solicitud.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                using (var respuesta = (HttpWebResponse)solicitud.GetResponse())
                using (var lector = new StreamReader(respuesta.GetResponseStream()))
                    json = lector.ReadToEnd();

                var serializer = new JavaScriptSerializer();
                var release = serializer.Deserialize<Dictionary<string, object>>(json);
                object tagObjeto;
                if (release == null || !release.TryGetValue("tag_name", out tagObjeto)) return null;
                Version version = ParsearVersion(Convert.ToString(tagObjeto, CultureInfo.InvariantCulture));
                if (version == null) return null;

                Version versionActual = ParsearVersion(FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileVersion);
                Trace.TraceInformation("Comprobación de GitHub: versión instalada {0}; última versión publicada {1}.",
                    versionActual == null ? "desconocida" : versionActual.ToString(), version.ToString());
                if (versionActual == null || version.CompareTo(versionActual) <= 0) return null;

                object assetsObjeto;
                var assets = release.TryGetValue("assets", out assetsObjeto) ? assetsObjeto as object[] : null;
                if (assets == null) return null;
                foreach (object assetObjeto in assets)
                {
                    var asset = assetObjeto as Dictionary<string, object>;
                    if (asset == null) continue;
                    object nombre, url, digest;
                    if (asset.TryGetValue("name", out nombre) &&
                        String.Equals(Convert.ToString(nombre, CultureInfo.InvariantCulture), NombrePaquetePortable, StringComparison.Ordinal) &&
                        asset.TryGetValue("browser_download_url", out url) &&
                        asset.TryGetValue("digest", out digest))
                    {
                        string hash = Convert.ToString(digest, CultureInfo.InvariantCulture);
                        Uri uri;
                        string direccion = Convert.ToString(url, CultureInfo.InvariantCulture);
                        if (hash != null && hash.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) &&
                            hash.Length == 71 && Uri.TryCreate(direccion, UriKind.Absolute, out uri) &&
                            uri.Scheme == Uri.UriSchemeHttps && String.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase))
                            return new Publicacion(version, direccion, hash.Substring(7));
                    }
                }
                return null;
            }
            catch (WebException ex)
            {
                var respuesta = ex.Response as HttpWebResponse;
                if (respuesta != null && respuesta.StatusCode != HttpStatusCode.NotFound)
                    Trace.TraceWarning("No se pudo consultar GitHub para buscar actualizaciones: HTTP " + (int)respuesta.StatusCode);
                else if (respuesta == null)
                    Trace.TraceInformation("No se pudo consultar GitHub; se continuará sin actualización en línea.");
                return null;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("No se pudo comprobar la última versión publicada: " + ex.GetType().Name);
                return null;
            }
        }

        private static string PrepararPaquetePortable(Publicacion publicacion)
        {
            string raiz = Path.Combine(Path.GetTempPath(), "ProgramaConcentracion-Actualizacion-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(raiz);
            string archivoZip = Path.Combine(raiz, NombrePaquetePortable);
            var solicitud = (HttpWebRequest)WebRequest.Create(publicacion.Url);
            solicitud.Method = "GET";
            solicitud.Proxy = null;
            solicitud.UserAgent = "Programa-de-Concentracion-Updater/1.0";
            solicitud.Timeout = 30000;
            solicitud.ReadWriteTimeout = 30000;
            using (var respuesta = (HttpWebResponse)solicitud.GetResponse())
            using (Stream entrada = respuesta.GetResponseStream())
            using (var salida = new FileStream(archivoZip, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                entrada.CopyTo(salida);

            string hashReal;
            using (var sha = SHA256.Create())
            using (var archivo = File.OpenRead(archivoZip))
                hashReal = BitConverter.ToString(sha.ComputeHash(archivo)).Replace("-", String.Empty);
            if (!String.Equals(hashReal, publicacion.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("La verificación de seguridad del paquete no coincide.");

            using (ZipArchive zip = ZipFile.OpenRead(archivoZip))
            {
                string[] encontrados = zip.Entries.Select(e => e.FullName).ToArray();
                string prefijo = "ProgramaConcentracion/";
                if (!encontrados.Contains(prefijo + "ProgramaConcentracion.exe", StringComparer.OrdinalIgnoreCase) ||
                    !encontrados.Contains(prefijo + "ActualizarInstalacion.ps1", StringComparer.OrdinalIgnoreCase) ||
                    encontrados.Any(e => Path.IsPathRooted(e) || e.Replace('\\', '/').Split('/').Any(p => p == "..")))
                    throw new InvalidDataException("El ZIP portátil no tiene el contenido esperado.");
                zip.ExtractToDirectory(raiz);
            }
            return Path.Combine(raiz, "ProgramaConcentracion");
        }

        private static Version ParsearVersion(string texto)
        {
            if (String.IsNullOrWhiteSpace(texto)) return null;
            texto = texto.Trim();
            if (texto.StartsWith("v", StringComparison.OrdinalIgnoreCase)) texto = texto.Substring(1);
            string[] partes = texto.Split('.');
            if (partes.Length < 3 || partes.Length > 4) return null;
            int[] numeros = new int[4];
            for (int i = 0; i < partes.Length; i++)
                if (!Int32.TryParse(partes[i], NumberStyles.None, CultureInfo.InvariantCulture, out numeros[i]) || numeros[i] < 0)
                    return null;
            return new Version(numeros[0], numeros[1], numeros[2], numeros[3]);
        }

        private sealed class Publicacion
        {
            public Publicacion(Version version, string url, string sha256)
            {
                Version = version;
                Url = url;
                Sha256 = sha256;
            }
            public Version Version { get; private set; }
            public string Url { get; private set; }
            public string Sha256 { get; private set; }
        }
    }
}
