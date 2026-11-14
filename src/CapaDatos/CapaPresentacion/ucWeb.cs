using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Net;
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
        private readonly Image imagenIconoDisenador;
        private bool seleccionada;
        private static readonly object _bloqueoCacheIconos = new object();

        private static readonly Dictionary<string, byte[]> _cacheIconos =
            new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, Lazy<Task<byte[]>>> _descargasIconos =
            new ConcurrentDictionary<string, Lazy<Task<byte[]>>>(StringComparer.OrdinalIgnoreCase);
        public E_Web Web { get; private set; }
        public bool Seleccionada { get { return seleccionada; } }

        public event EventHandler SeleccionarSolicitado;
        public event EventHandler QuitarSolicitado;

        public ucWeb()
        {
            InitializeComponent();
            if (FormularioSuave.EnModoDiseno) return;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Color.Transparent;
            lblNombre.BackColor = Color.Transparent;
            lblLink.BackColor = Color.Transparent;
            TemaAplicacion.Preparar(this);
            imagenIconoDisenador = pbIcono.Image;

            lblNombre.DoubleClick += lblNombre_DoubleClick;
            btnQuitar.Click += (s, e) =>
                QuitarSolicitado?.Invoke(this, EventArgs.Empty);

            Click += (s, e) =>
                SeleccionarSolicitado?.Invoke(this, EventArgs.Empty);

            lblNombre.Click += (s, e) =>
                SeleccionarSolicitado?.Invoke(this, EventArgs.Empty);

            pbIcono.Click += (s, e) =>
                SeleccionarSolicitado?.Invoke(this, EventArgs.Empty);


        }

        public void EstablecerSeleccionada(bool valor)
        {
            seleccionada = valor;
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (seleccionada)
            {
                using (var pincel = new SolidBrush(PaletaAplicacion.AzulHover))
                    e.Graphics.FillRectangle(pincel, ClientRectangle);
            }
            else base.OnPaintBackground(e);
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
            if (anterior != null && !ReferenceEquals(anterior, imagenIconoDisenador) &&
                !ReferenceEquals(anterior, nuevaImagen))
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
            btnQuitar.Visible = mostrarBotonQuitar;

            lblNombre.Text = web.Nombre;
            lblLink.Text = FormatearLink(web.Link);
            ReemplazarImagen(imagenIconoDisenador);
            if (!CargarIconoDesdeCache(web.Link))
                CargarIconoAsync(web.Link);

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

            string clave = ObtenerClaveCache(link);
            if (clave == null) return;
            Uri icono = new Uri(clave + "/favicon.ico");
            Lazy<Task<byte[]>> descarga = _descargasIconos.GetOrAdd(clave,
                _ => new Lazy<Task<byte[]>>(() => DescargarIconoAltaResolucionAsync(icono, clave)));
            try
            {
                byte[] datos = await descarga.Value;
                if (datos == null || datos.Length == 0 || IsDisposed) return;
                GuardarIconoEnCache(link, datos);
                if (Web == null || !String.Equals(ObtenerClaveCache(Web.Link), clave,
                    StringComparison.OrdinalIgnoreCase)) return;
                using (var memoria = new MemoryStream(datos))
                using (var imagenTemporal = Image.FromStream(memoria))
                    ReemplazarImagen(new Bitmap(imagenTemporal));
            }
            catch (WebException) { }
            catch (ArgumentException) { }
            catch (InvalidOperationException) { }
        }

        private static async Task<byte[]> DescargarIconoAsync(Uri icono)
        {
            try
            {
                using (var cliente = new WebClientConTimeout())
                {
                    cliente.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0";
                    return await cliente.DownloadDataTaskAsync(icono).ConfigureAwait(false);
                }
            }
            catch (WebException) { return null; }
        }

        private static async Task<byte[]> DescargarIconoAltaResolucionAsync(Uri icono, string origen)
        {
            byte[] datos = await DescargarIconoAsync(icono).ConfigureAwait(false);
            if (TieneResolucionUtil(datos)) return datos;

            try
            {
                var altaResolucion = new Uri("https://www.google.com/s2/favicons?domain_url=" +
                    Uri.EscapeDataString(origen) + "&sz=128");
                byte[] alternativa = await DescargarIconoAsync(altaResolucion).ConfigureAwait(false);
                return TieneResolucionUtil(alternativa) ? alternativa : datos ?? alternativa;
            }
            catch (UriFormatException)
            {
                return datos;
            }
        }

        private static bool TieneResolucionUtil(byte[] datos)
        {
            if (datos == null || datos.Length == 0) return false;
            try
            {
                using (var memoria = new MemoryStream(datos))
                using (var imagen = Image.FromStream(memoria))
                    return imagen.Width >= 64 && imagen.Height >= 64;
            }
            catch (ArgumentException) { return false; }
            catch (OutOfMemoryException) { return false; }
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
