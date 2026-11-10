using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaEntidades;
using System.Linq;
using System.Diagnostics;

namespace CapaPresentacion
{
    public partial class ucWeb : UserControl
    {
        private TextBox txtEditarNombre;
        private static readonly object _bloqueoCacheIconos = new object();

        private static readonly Dictionary<string, byte[]> _cacheIconos =
            new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        public E_Web Web { get; private set; }

        public event EventHandler SeleccionarSolicitado;
        public event EventHandler QuitarSolicitado;

        public ucWeb()
        {
            InitializeComponent();
            Tipografia.Aplicar(this);

            lblNombre.DoubleClick += lblNombre_DoubleClick;
            btnQuitar.Click += (s, e) =>
                QuitarSolicitado?.Invoke(this, EventArgs.Empty);

            Click += (s, e) =>
                SeleccionarSolicitado?.Invoke(this, EventArgs.Empty);

            lblNombre.Click += (s, e) =>
                SeleccionarSolicitado?.Invoke(this, EventArgs.Empty);

            pbIcono.Click += (s, e) =>
                SeleccionarSolicitado?.Invoke(this, EventArgs.Empty);

            pbIcono.SizeMode = PictureBoxSizeMode.Zoom;

        }

        private bool CargarIconoDesdeCache(string link)
        {
            string clave = ObtenerClaveCache(link);

            if (clave == null)
                return false;

            byte[] datos;

            lock (_bloqueoCacheIconos)
            {
                if (!_cacheIconos.TryGetValue(clave, out datos))
                    return false;
            }

            try
            {
                using (var memoria = new MemoryStream(datos))
                using (var imagenTemporal = Image.FromStream(memoria))
                {
                    ReemplazarImagen(new Bitmap(imagenTemporal));
                }

                return true;
            }
            catch
            {
                lock (_bloqueoCacheIconos)
                {
                    _cacheIconos.Remove(clave);
                }

                return false;
            }
        }

        private static string ObtenerClaveCache(string link)
        {
            Uri uri;

            if (!Uri.TryCreate(link, UriKind.Absolute, out uri))
                return null;

            return uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        }

        private void GuardarIconoEnCache(string link, byte[] datos)
        {
            string clave = ObtenerClaveCache(link);

            if (clave == null || datos == null || datos.Length == 0)
                return;

            lock (_bloqueoCacheIconos)
            {
                _cacheIconos[clave] = datos;
            }
        }

        private void ReemplazarImagen(Image nuevaImagen)
        {
            Image anterior = pbIcono.Image;
            pbIcono.Image = nuevaImagen;

            if (anterior != null)
                anterior.Dispose();
        }

        private static string FormatearLink(string link)
        {
            if (String.IsNullOrWhiteSpace(link))
                return String.Empty;

            string direccion = link.Trim();

            if (!direccion.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !direccion.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                direccion = "https://" + direccion;
            }

            Uri uri;

            if (!Uri.TryCreate(direccion, UriKind.Absolute, out uri))
                return link;

            string dominio = uri.Host;

            if (dominio.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                dominio = dominio.Substring(4);

            return dominio;
        }

        public void Configurar(E_Web web, bool mostrarBotonQuitar)
        {
            Web = web ?? throw new ArgumentNullException("web");

            lblNombre.Text = web.Nombre;
            lblLink.Text = FormatearLink(web.Link);
            btnQuitar.Visible = mostrarBotonQuitar;

            if (!CargarIconoDesdeCache(web.Link))
            {
                pbIcono.Image = SystemIcons.Application.ToBitmap();
                CargarIconoAsync(web.Link);
            }
        }

        private async void CargarIconoAsync(string link)
        {
            Uri sitio;

            if (!Uri.TryCreate(link, UriKind.Absolute, out sitio) ||
                (sitio.Scheme != Uri.UriSchemeHttp &&
                 sitio.Scheme != Uri.UriSchemeHttps))
            {
                return;
            }

            bool esIp = IPAddress.TryParse(sitio.Host, out _);
            bool esLocalhost = String.Equals(
                sitio.Host, "localhost", StringComparison.OrdinalIgnoreCase);

            // Evita intentar resolver entradas como "youtube".
            if (!esIp && !esLocalhost && !sitio.Host.Contains("."))
                return;

            var candidatos = new List<Tuple<int, Uri>>();

            try
            {
                string html;

                using (var cliente = new WebClientConTimeout())
                {
                    cliente.Headers[HttpRequestHeader.UserAgent] =
                        "Mozilla/5.0";

                    html = await cliente.DownloadStringTaskAsync(sitio);
                }

                foreach (Match etiqueta in Regex.Matches(
                    html ?? String.Empty,
                    @"<link\b[^>]*>",
                    RegexOptions.IgnoreCase))
                {
                    string tag = etiqueta.Value;
                    string rel = ObtenerAtributoHtml(tag, "rel");
                    string href = ObtenerAtributoHtml(tag, "href");
                    string sizes = ObtenerAtributoHtml(tag, "sizes");

                    if (String.IsNullOrWhiteSpace(href))
                        continue;

                    bool esIcono =
                        (!String.IsNullOrWhiteSpace(rel) &&
                         (rel.IndexOf("icon", StringComparison.OrdinalIgnoreCase) >= 0));

                    if (!esIcono)
                        continue;

                    Uri uriIcono;
                    if (!Uri.TryCreate(sitio, href, out uriIcono))
                        continue;

                    int tamano = 0;
                    Match matchTamano = Regex.Match(
                        sizes ?? String.Empty,
                        @"(\d+)\s*x\s*(\d+)",
                        RegexOptions.IgnoreCase);

                    if (matchTamano.Success)
                    {
                        int ancho;
                        int alto;

                        if (Int32.TryParse(matchTamano.Groups[1].Value, out ancho) &&
                            Int32.TryParse(matchTamano.Groups[2].Value, out alto))
                        {
                            tamano = Math.Min(ancho, alto);
                        }
                    }

                    candidatos.Add(Tuple.Create(tamano, uriIcono));
                }
            }
            catch (WebException)
            {
                // Sin internet o el sitio no responde; se prueban iconos estándar.
            }

            // El sitio puede ofrecer el icono táctil en mayor resolución.
            candidatos.Add(Tuple.Create(
                180,
                new Uri(sitio.GetLeftPart(UriPartial.Authority) +
                        "/apple-touch-icon.png")));

            candidatos.Add(Tuple.Create(
                16,
                new Uri(sitio.GetLeftPart(UriPartial.Authority) +
                        "/favicon.ico")));

            foreach (var candidato in candidatos
                .OrderByDescending(item => item.Item1))
            {
                try
                {
                    byte[] datos;

                    using (var cliente = new WebClientConTimeout())
                    {
                        cliente.Headers[HttpRequestHeader.UserAgent] =
                            "Mozilla/5.0";

                        datos = await cliente.DownloadDataTaskAsync(candidato.Item2);
                    }

                    // Guarda los bytes descargados para los próximos refresh del FlowLayout.
                    GuardarIconoEnCache(link, datos);

                    // Actualiza esta tarjeta solo si todavía corresponde a este enlace.
                    if (Web != null &&
                        String.Equals(
                            ObtenerClaveCache(Web.Link),
                            ObtenerClaveCache(link),
                            StringComparison.OrdinalIgnoreCase))
                    {
                        using (var memoria = new MemoryStream(datos))
                        using (var imagenTemporal = Image.FromStream(memoria))
                        {
                            ReemplazarImagen(new Bitmap(imagenTemporal));
                        }
                    }

                    return;
                }
                catch (WebException)
                {
                    // Intenta el siguiente icono declarado o estándar.
                }
                catch (ArgumentException)
                {
                    // El archivo no es una imagen compatible; intenta el siguiente.
                }
                catch (InvalidOperationException)
                {
                    // El control pudo cerrarse mientras se descargaba.
                    return;
                }
            }
        }

        private static string ObtenerAtributoHtml(string etiqueta, string atributo)
        {
            Match match = Regex.Match(
                etiqueta,
                atributo + @"\s*=\s*[""']([^""']*)[""']",
                RegexOptions.IgnoreCase);

            return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : null;
        }

        private sealed class WebClientConTimeout : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest solicitud = base.GetWebRequest(address);
                solicitud.Timeout = 5000;

                var solicitudHttp = solicitud as HttpWebRequest;
                if (solicitudHttp != null)
                    solicitudHttp.ReadWriteTimeout = 5000;

                return solicitud;
            }
        }


        private void lblNombre_DoubleClick(object sender, EventArgs e)
        {
            if (Web == null || txtEditarNombre != null)
                return;

            txtEditarNombre = new TextBox
            {
                Bounds = lblNombre.Bounds,
                Text = lblNombre.Text,
                MaxLength = 100
            };

            Controls.Add(txtEditarNombre);
            txtEditarNombre.BringToFront();
            txtEditarNombre.SelectAll();
            txtEditarNombre.KeyDown += txtEditarNombre_KeyDown;
            txtEditarNombre.LostFocus += txtEditarNombre_LostFocus;
            txtEditarNombre.Focus();
        }

        private void txtEditarNombre_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                GuardarNombreEditado();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                CancelarEdicionNombre();
            }
        }

        private void txtEditarNombre_LostFocus(object sender, EventArgs e)
        {
            GuardarNombreEditado();
        }

        private void GuardarNombreEditado()
        {
            if (txtEditarNombre == null)
                return;

            string nombre = txtEditarNombre.Text.Trim();

            if (!String.IsNullOrWhiteSpace(nombre))
            {
                Web.Nombre = nombre;
                lblNombre.Text = nombre;
            }

            CancelarEdicionNombre();
        }

        private void CancelarEdicionNombre()
        {
            if (txtEditarNombre == null)
                return;

            txtEditarNombre.KeyDown -= txtEditarNombre_KeyDown;
            txtEditarNombre.LostFocus -= txtEditarNombre_LostFocus;

            Controls.Remove(txtEditarNombre);
            txtEditarNombre.Dispose();
            txtEditarNombre = null;
        }
    }
}
