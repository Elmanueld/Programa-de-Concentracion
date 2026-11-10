using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CapaEntidades;
using CapaNegocio;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CapaPresentacion
{
    public partial class frmSeleccionarWeb : Form
    {
        private readonly N_Web _negocioWeb = new N_Web();
        private readonly int _idUsuario;
        private readonly List<E_Web> _websCatalogo = new List<E_Web>();
        private readonly List<E_Web> _websSeleccionadas = new List<E_Web>();
        private readonly System.Windows.Forms.Timer _timerBusqueda =
            new System.Windows.Forms.Timer();

        private int _versionBusqueda;
        private string _ultimoTerminoProcesado;

        public frmSeleccionarWeb(int idUsuario)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El ID de usuario no es válido.");
            _idUsuario = idUsuario;
            InitializeComponent();
            Tipografia.Aplicar(this);
            ArrastreFormulario.Habilitar(this, pnlTop);

            btnAceptar.Click += btnAceptar_Click;
            btnCancelar.Click += btnCancelar_Click;
            txtLink.KeyDown += txtLink_KeyDown;
            txtBuscar.TextChanged += txtBuscar_TextChanged;
            txtBuscar.KeyUp += txtBuscar_KeyUp;
            _timerBusqueda.Interval = 500;

            _timerBusqueda.Tick += async (s, e) =>
            {
                _timerBusqueda.Stop();
                string termino = txtBuscar.Text.Trim();
                int version = _versionBusqueda;
                if (termino.Length >= 2)
                    MostrarEstadoBusqueda("Buscando páginas en internet...");
                await BuscarWebsAsync(termino, version);
            };

            MostrarResultados(_websCatalogo);

            CargarCatalogo();


        }

        public List<E_Web> WebsSeleccionadas
        {
            get { return _websSeleccionadas.ToList(); }
        }

        private void CargarCatalogo()
        {
            try
            {
                _websCatalogo.Clear();
                _websCatalogo.AddRange(_negocioWeb.BuscarWeb(_idUsuario, ""));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "No se pudo cargar el catálogo de webs: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            ActualizarBusqueda();
        }

        private void txtBuscar_KeyUp(object sender, KeyEventArgs e)
        {
            ActualizarBusqueda();
        }

        private void ActualizarBusqueda()
        {
            string termino = txtBuscar.Text.Trim();
            if (String.Equals(
                termino,
                _ultimoTerminoProcesado,
                StringComparison.Ordinal))
            {
                return;
            }

            _ultimoTerminoProcesado = termino;
            int version = ++_versionBusqueda;

            // Muestra primero las coincidencias que ya están guardadas.
            IEnumerable<E_Web> locales = _websCatalogo;

            if (!String.IsNullOrWhiteSpace(termino))
            {
                locales = FiltrarCatalogo(termino);
            }

            MostrarResultados(locales);

            _timerBusqueda.Stop();

            // Evita enviar consultas con texto demasiado corto.
            if (termino.Length >= 2)
                _timerBusqueda.Start();
        }

        private async Task BuscarWebsAsync(string termino, int version)
        {
            if (String.IsNullOrWhiteSpace(termino) || termino.Length < 2)
                return;

            try
            {
                // Fuerza TLS moderno en equipos donde .NET Framework conserva
                // una configuración de seguridad antigua del sistema.
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                string urlConsulta =
                    "https://html.duckduckgo.com/html/?q=" +
                    Uri.EscapeDataString(termino);

                string html;
                try
                {
                    html = await DescargarBusquedaAsync(urlConsulta);
                }
                catch
                {
                    if (IsDisposed || version != _versionBusqueda)
                        return;

                    string urlLigera =
                        "https://lite.duckduckgo.com/lite/?q=" +
                        Uri.EscapeDataString(termino);
                    try
                    {
                        html = await DescargarBusquedaAsync(urlLigera);
                    }
                    catch
                    {
                        html = String.Empty;
                    }
                }

                // Si mientras llegaba la respuesta cambió el texto, descarta
                // los resultados antiguos.
                if (IsDisposed ||
                    version != _versionBusqueda ||
                    !String.Equals(
                        txtBuscar.Text.Trim(),
                        termino,
                        StringComparison.Ordinal))
                    return;

                List<E_Web> resultadosOnline = ExtraerResultadosDuckDuckGo(html);

                // Si la versión HTML normal no entrega resultados, prueba su
                // versión ligera antes de recurrir solo al catálogo local.
                if (resultadosOnline.Count == 0)
                {
                    string urlLigera =
                        "https://lite.duckduckgo.com/lite/?q=" +
                        Uri.EscapeDataString(termino);
                    string htmlLigero;
                    try
                    {
                        htmlLigero = await DescargarBusquedaAsync(urlLigera);
                    }
                    catch
                    {
                        htmlLigero = String.Empty;
                    }

                    if (IsDisposed || version != _versionBusqueda)
                        return;

                    resultadosOnline = ExtraerResultadosDuckDuckGo(htmlLigero);
                }

                // DuckDuckGo puede devolver una página de verificación o una
                // respuesta distinta a la página HTML que espera el parser.
                // En ese caso consulta Bing como respaldo para no dejar la
                // búsqueda externa vacía.
                if (resultadosOnline.Count == 0)
                {
                    string urlRespaldo =
                        "https://www.bing.com/search?q=" +
                        Uri.EscapeDataString(termino);
                    string htmlRespaldo =
                        await DescargarBusquedaAsync(urlRespaldo);

                    if (IsDisposed || version != _versionBusqueda)
                        return;

                    resultadosOnline = ExtraerResultadosBing(htmlRespaldo);
                }

                IEnumerable<E_Web> locales = FiltrarCatalogo(termino);

                MostrarResultados(locales.Concat(resultadosOnline));

                if (resultadosOnline.Count == 0)
                {
                    MostrarEstadoBusqueda(
                        "No se encontraron resultados web para «" + termino +
                        "». Solo aparecen coincidencias guardadas; puedes pegar " +
                        "una dirección en el campo de enlace para añadirla.");
                }
            }
            catch (Exception ex)
            {
                if (IsDisposed || version != _versionBusqueda)
                    return;

                System.Diagnostics.Trace.TraceWarning(
                    "Falló la búsqueda web externa: " + ex);

                MostrarEstadoBusqueda(
                    "No se pudo consultar el buscador web. Las tarjetas visibles " +
                    "son coincidencias guardadas en la base de datos.");
            }
        }

        private static async Task<string> DescargarBusquedaAsync(string url)
        {
            var solicitud = (HttpWebRequest)WebRequest.Create(url);
            solicitud.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                "AppleWebKit/537.36 Chrome/120 Safari/537.36";
            solicitud.Accept =
                "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            solicitud.Headers[HttpRequestHeader.AcceptLanguage] =
                "es-ES,es;q=0.9,en;q=0.8";
            solicitud.Referer = "https://duckduckgo.com/";
            solicitud.Timeout = 15000;
            solicitud.ReadWriteTimeout = 15000;
            solicitud.AutomaticDecompression =
                DecompressionMethods.GZip |
                DecompressionMethods.Deflate;

            using (var respuesta =
                (HttpWebResponse)await solicitud.GetResponseAsync())
            using (var lector = new System.IO.StreamReader(
                respuesta.GetResponseStream()))
            {
                return await lector.ReadToEndAsync();
            }
        }

        private void MostrarEstadoBusqueda(string mensaje)
        {
            var estado = new Label
            {
                AutoSize = false,
                Width = Math.Max(500, flBusqueda.ClientSize.Width - 32),
                Height = 48,
                Margin = new Padding(8),
                Padding = new Padding(10),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                ForeColor = System.Drawing.Color.FromArgb(55, 70, 95),
                BackColor = System.Drawing.Color.Transparent,
                Font = new System.Drawing.Font("Inter", 9F),
                Text = mensaje
            };

            flBusqueda.Controls.Add(estado);
        }

        private IEnumerable<E_Web> FiltrarCatalogo(string termino)
        {
            return _websCatalogo.Where(web =>
                (web.Nombre ?? String.Empty).IndexOf(
                    termino,
                    StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private List<E_Web> ExtraerResultadosDuckDuckGo(string html)
        {
            var resultados = new List<E_Web>();
            var enlacesVistos = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            MatchCollection etiquetas = Regex.Matches(
                html ?? String.Empty,
                @"<a\b[^>]*class\s*=\s*[""'][^""']*\b(?:result__a|result-link)\b[^""']*[""'][^>]*>.*?</a>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match etiqueta in etiquetas)
            {
                Match matchHref = Regex.Match(
                    etiqueta.Value,
                    @"href\s*=\s*[""']([^""']+)[""']",
                    RegexOptions.IgnoreCase);

                if (!matchHref.Success)
                    continue;

                string href = WebUtility.HtmlDecode(matchHref.Groups[1].Value);
                string link = ObtenerEnlaceFinal(href);

                Uri uri;
                if (!Uri.TryCreate(link, UriKind.Absolute, out uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp &&
                     uri.Scheme != Uri.UriSchemeHttps))
                {
                    continue;
                }

                string texto = Regex.Replace(
                    etiqueta.Value,
                    @"<.*?>",
                    String.Empty,
                    RegexOptions.Singleline);

                string nombre = WebUtility.HtmlDecode(texto).Trim();

                if (String.IsNullOrWhiteSpace(nombre) ||
                    !enlacesVistos.Add(uri.AbsoluteUri))
                {
                    continue;
                }

                resultados.Add(new E_Web
                {
                    IdWeb = 0,
                    Nombre = nombre,
                    Link = uri.AbsoluteUri
                });
            }

            return resultados;
        }

        private List<E_Web> ExtraerResultadosBing(string html)
        {
            var resultados = new List<E_Web>();
            var enlacesVistos = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            // Bing presenta los resultados orgánicos como un enlace dentro de
            // un encabezado h2. Se decodifica el HTML antes de validar la URL.
            MatchCollection enlaces = Regex.Matches(
                html ?? String.Empty,
                @"<h2\b[^>]*>\s*<a\b[^>]*href\s*=\s*[""']([^""']+)[""'][^>]*>(.*?)</a>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match enlace in enlaces)
            {
                string link = WebUtility.HtmlDecode(enlace.Groups[1].Value);
                string nombre = WebUtility.HtmlDecode(Regex.Replace(
                    enlace.Groups[2].Value,
                    @"<.*?>",
                    String.Empty,
                    RegexOptions.Singleline)).Trim();

                link = ObtenerEnlaceFinalBing(link);

                Uri uri;
                if (String.IsNullOrWhiteSpace(nombre) ||
                    !Uri.TryCreate(link, UriKind.Absolute, out uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp &&
                     uri.Scheme != Uri.UriSchemeHttps) ||
                    uri.Host.IndexOf("bing.com", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    !enlacesVistos.Add(uri.AbsoluteUri))
                {
                    continue;
                }

                resultados.Add(new E_Web
                {
                    IdWeb = 0,
                    Nombre = nombre,
                    Link = uri.AbsoluteUri
                });
            }

            return resultados;
        }

        private string ObtenerEnlaceFinalBing(string link)
        {
            Uri uri;
            if (!Uri.TryCreate(link, UriKind.Absolute, out uri) ||
                uri.Host.IndexOf("bing.com", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return link;
            }

            string destino = ObtenerParametroQuery(uri.Query, "u");
            if (String.IsNullOrWhiteSpace(destino))
                return link;

            // Bing codifica el destino como "a1" + Base64 URL-safe.
            if (destino.StartsWith("a1", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string base64 = destino.Substring(2)
                        .Replace('-', '+')
                        .Replace('_', '/');
                    base64 = base64.PadRight(
                        base64.Length + (4 - base64.Length % 4) % 4,
                        '=');
                    destino = System.Text.Encoding.UTF8.GetString(
                        Convert.FromBase64String(base64));
                }
                catch (FormatException)
                {
                    return link;
                }
            }

            Uri destinoUri;
            return Uri.TryCreate(destino, UriKind.Absolute, out destinoUri) &&
                   (destinoUri.Scheme == Uri.UriSchemeHttp ||
                    destinoUri.Scheme == Uri.UriSchemeHttps)
                ? destinoUri.AbsoluteUri
                : link;
        }

        private string ObtenerEnlaceFinal(string href)
        {
            if (href.StartsWith("//"))
                href = "https:" + href;

            Uri uri;
            if (!Uri.TryCreate(href, UriKind.Absolute, out uri))
                return String.Empty;

            // DuckDuckGo puede envolver el destino en un enlace de redirección.
            if (uri.Host.IndexOf(
                "duckduckgo.com",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string destino = ObtenerParametroQuery(uri.Query, "uddg");

                if (!String.IsNullOrWhiteSpace(destino))
                    return destino;
            }

            return uri.AbsoluteUri;
        }

        private string ObtenerParametroQuery(string query, string nombre)
        {
            if (String.IsNullOrWhiteSpace(query))
                return null;

            string consulta = query.TrimStart('?');

            foreach (string parte in consulta.Split('&'))
            {
                string[] claveValor = parte.Split(new[] { '=' }, 2);

                if (claveValor.Length == 2 &&
                    String.Equals(
                        Uri.UnescapeDataString(claveValor[0]),
                        nombre,
                        StringComparison.OrdinalIgnoreCase))
                {
                    string valor = claveValor[1].Replace("+", "%20");
                    return Uri.UnescapeDataString(valor);
                }
            }

            return null;
        }

        private void MostrarResultados(IEnumerable<E_Web> webs)
        {
            flBusqueda.SuspendLayout();

            try
            {
                flBusqueda.Controls.Clear();

                foreach (E_Web web in webs
                    .Where(item => item != null &&
                                   !String.IsNullOrWhiteSpace(item.Link))
                    .GroupBy(
                        item => item.Link.TrimEnd('/'),
                        StringComparer.OrdinalIgnoreCase)
                    .Select(grupo => grupo.First()))
                {
                    var tarjeta = new ucWeb();
                    tarjeta.Configurar(web, false);
                    tarjeta.SeleccionarSolicitado +=
                        TarjetaBusqueda_SeleccionarSolicitado;

                    flBusqueda.Controls.Add(tarjeta);
                }
            }
            finally
            {
                flBusqueda.ResumeLayout(true);
            }
        }

        private void TarjetaBusqueda_SeleccionarSolicitado(
            object sender,
            EventArgs e)
        {
            var tarjeta = sender as ucWeb;

            if (tarjeta != null)
                AgregarSeleccion(tarjeta.Web);
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            AgregarLinkManual();
        }

        private void txtLink_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.SuppressKeyPress = true;
            AgregarLinkManual();
        }

        private void AgregarLinkManual()
        {
            string link;
            string mensaje;

            if (!IntentarNormalizarLink(txtLink.Text, out link, out mensaje))
            {
                MessageBox.Show(
                    this,
                    mensaje,
                    "Enlace no válido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtLink.Focus();
                return;
            }

            E_Web existente = BuscarPorLink(link);

            E_Web web = existente ?? new E_Web
            {
                IdWeb = 0,
                Link = link,
                Nombre = ObtenerNombreDesdeLink(link)
            };

            AgregarSeleccion(web);
            txtLink.Clear();
            txtLink.Focus();
        }

        private bool IntentarNormalizarLink(
            string valor,
            out string link,
            out string mensaje)
        {
            link = null;
            mensaje = null;

            valor = (valor ?? String.Empty).Trim();

            if (String.IsNullOrWhiteSpace(valor))
            {
                mensaje = "Escribe la dirección de la web.";
                return false;
            }

            if (!valor.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !valor.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                valor = "https://" + valor;
            }

            Uri uri;

            if (!Uri.TryCreate(valor, UriKind.Absolute, out uri) ||
                (uri.Scheme != Uri.UriSchemeHttp &&
                 uri.Scheme != Uri.UriSchemeHttps) ||
                String.IsNullOrWhiteSpace(uri.Host))
            {
                mensaje = "Escribe una dirección válida, por ejemplo: https://example.com";
                return false;
            }

            link = uri.GetLeftPart(UriPartial.Path).TrimEnd('/');

            if (link.Length > 2048)
            {
                mensaje = "El enlace supera el máximo permitido de 2048 caracteres.";
                return false;
            }

            return true;
        }

        private string ObtenerNombreDesdeLink(string link)
        {
            Uri uri = new Uri(link);
            string host = uri.Host;

            if (host.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                host = host.Substring(4);

            string nombre = host.Split('.').FirstOrDefault();

            if (String.IsNullOrWhiteSpace(nombre))
                nombre = host;

            return nombre;
        }

        private E_Web BuscarPorLink(string link)
        {
            return _websCatalogo
                .Concat(_websSeleccionadas)
                .FirstOrDefault(web =>
                    String.Equals(
                        web.Link.TrimEnd('/'),
                        link.TrimEnd('/'),
                        StringComparison.OrdinalIgnoreCase));
        }

        private void AgregarSeleccion(E_Web web)
        {
            if (web == null)
                return;

            bool yaSeleccionada = _websSeleccionadas.Any(actual =>
                String.Equals(
                    actual.Link.TrimEnd('/'),
                    web.Link.TrimEnd('/'),
                    StringComparison.OrdinalIgnoreCase));

            if (yaSeleccionada)
                return;

            // Copia para que editar el nombre de la selección no cambie
            // automáticamente el nombre de la tarjeta del catálogo.
            var copia = new E_Web
            {
                IdWeb = web.IdWeb,
                Nombre = web.Nombre,
                Link = web.Link
            };

            _websSeleccionadas.Add(copia);
            MostrarSeleccionadas();
        }

        private void MostrarSeleccionadas()
        {
            flWebSeleccionadas.SuspendLayout();

            try
            {
                flWebSeleccionadas.Controls.Clear();

                foreach (E_Web web in _websSeleccionadas)
                {
                    var tarjeta = new ucWeb();
                    tarjeta.Configurar(web, true);
                    tarjeta.QuitarSolicitado +=
                        TarjetaSeleccionada_QuitarSolicitado;

                    flWebSeleccionadas.Controls.Add(tarjeta);
                }
            }
            finally
            {
                flWebSeleccionadas.ResumeLayout(true);
            }
        }

        private void TarjetaSeleccionada_QuitarSolicitado(
            object sender,
            EventArgs e)
        {
            var tarjeta = sender as ucWeb;

            if (tarjeta == null)
                return;

            _websSeleccionadas.RemoveAll(web =>
                String.Equals(
                    web.Link.TrimEnd('/'),
                    tarjeta.Web.Link.TrimEnd('/'),
                    StringComparison.OrdinalIgnoreCase));

            MostrarSeleccionadas();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
