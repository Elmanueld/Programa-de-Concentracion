using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CapaPresentacion
{
    /// <summary>Base común para dibujar formularios y sus controles con doble buffer.</summary>
    public class FormularioSuave : Form
    {
        private const int WmDisplayChange = 0x007E;
        private const float AnchoBorde = 1f;
        private bool esquinasNativas;
        private bool bordeNativo;

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd, int atributo, ref int valor, int tamano);

        public FormularioSuave()
        {
            if (EnModoDiseno) return;
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            ControlesPresentacion.Optimizar(this);
            AjustePantalla.Preparar(this);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!EnModoDiseno && !esquinasNativas) AplicarEsquinasRedondeadas();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WmDisplayChange && !EnModoDiseno)
                AjustePantalla.NotificarCambioResolucion(this);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (EnModoDiseno) return;
            AplicarDecoracionNativa();
            if (!esquinasNativas) AplicarEsquinasRedondeadas();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (EnModoDiseno || bordeNativo || ClientSize.Width <= 2 || ClientSize.Height <= 2) return;

            float escala = AjustePantalla.EscalaActual(this);
            float anchoBorde = AnchoBorde * escala;
            RectangleF limites = new RectangleF(
                anchoBorde,
                anchoBorde,
                ClientSize.Width - anchoBorde * 2f,
                ClientSize.Height - anchoBorde * 2f);
            float radio = RadioEsquinaEscalado();
            using (GraphicsPath trazado = CrearTrazadoRedondeado(
                limites, Math.Max(1f, radio - anchoBorde)))
            using (Pen borde = new Pen(Color.FromArgb(96, 96, 96), anchoBorde))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                e.Graphics.DrawPath(borde, trazado);
            }
        }

        private void AplicarEsquinasRedondeadas()
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
            using (GraphicsPath trazado = CrearTrazadoRedondeado(
                new RectangleF(0, 0, ClientSize.Width, ClientSize.Height), RadioEsquinaEscalado()))
            {
                Region regionAnterior = Region;
                Region = new Region(trazado);
                if (regionAnterior != null) regionAnterior.Dispose();
            }
            Invalidate();
        }

        private void AplicarDecoracionNativa()
        {
            esquinasNativas = false;
            bordeNativo = false;
            try
            {
                // Windows 11 dibuja el radio y el borde en DWM con antialiasing
                // nativo, evitando la máscara Region pixelada de WinForms.
                int redondeo = 2; // DWMWCP_ROUND
                esquinasNativas = DwmSetWindowAttribute(Handle, 33, ref redondeo,
                    sizeof(int)) == 0; // DWMWA_WINDOW_CORNER_PREFERENCE
                if (!esquinasNativas) return;

                Region regionAnterior = Region;
                Region = null;
                if (regionAnterior != null) regionAnterior.Dispose();

                int gris = 0x00606060; // COLORREF RGB(96,96,96)
                bordeNativo = DwmSetWindowAttribute(Handle, 34, ref gris,
                    sizeof(int)) == 0; // DWMWA_BORDER_COLOR
            }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
        }

        private float RadioEsquinaEscalado()
        {
            return 8f * AjustePantalla.EscalaActual(this);
        }

        private static GraphicsPath CrearTrazadoRedondeado(RectangleF rectangulo, float radio)
        {
            var trazado = new GraphicsPath();
            float diametro = Math.Min(radio * 2f,
                Math.Min(rectangulo.Width, rectangulo.Height));
            if (diametro <= 0)
            {
                trazado.AddRectangle(rectangulo);
                return trazado;
            }
            trazado.AddArc(rectangulo.X, rectangulo.Y, diametro, diametro, 180, 90);
            trazado.AddArc(rectangulo.Right - diametro, rectangulo.Y, diametro, diametro, 270, 90);
            trazado.AddArc(rectangulo.Right - diametro, rectangulo.Bottom - diametro, diametro, diametro, 0, 90);
            trazado.AddArc(rectangulo.X, rectangulo.Bottom - diametro, diametro, diametro, 90, 90);
            trazado.CloseFigure();
            return trazado;
        }

        internal static bool EnModoDiseno
        {
            get
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return true;
                try
                {
                    string proceso = Process.GetCurrentProcess().ProcessName;
                    return proceso.Equals("devenv", StringComparison.OrdinalIgnoreCase) ||
                        proceso.IndexOf("DesignToolsServer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        proceso.Equals("XDesProc", StringComparison.OrdinalIgnoreCase);
                }
                catch { return false; }
            }
        }

    }

    /// <summary>FlowLayoutPanel con doble buffer para listas y cuadrículas dinámicas.</summary>
    public sealed class BufferedFlowLayoutPanel : FlowLayoutPanel
    {
        /// <summary>Permite listas con scroll real sin convertirlas en paginadores.</summary>
        public bool PaginacionHabilitada { get; set; } = true;

        public BufferedFlowLayoutPanel()
        {
            DoubleBuffered = true;
        }

    }

    /// <summary>Presenta solo una página de las tarjetas de un FlowLayoutPanel.</summary>
    internal sealed class PaginadorFlowLayout
    {
        private const int AltoBarraDiseno = 56;
        private static readonly Font FuenteBotonesDiseno = new Font("Segoe UI Symbol", 18F);
        private readonly BufferedFlowLayoutPanel lista;
        private readonly Control contenedor;
        private readonly Padding margenOriginal;
        private readonly Panel barra;
        private readonly AntdUI.Button anterior;
        private readonly AntdUI.Button siguiente;
        private readonly Label pagina;
        private Control encabezado;
        private readonly List<Control> elementos = new List<Control>();
        private bool aplicandoPagina;
        private bool actualizacionPendiente;
        private bool recalculoPendiente;
        private int indicePagina;
        private int elementosPorPagina = 1;

        public PaginadorFlowLayout(BufferedFlowLayoutPanel lista)
        {
            this.lista = lista;
            contenedor = lista.Parent;
            if (contenedor == null) return;

            margenOriginal = new Padding(
                Math.Max(0, lista.Left), Math.Max(0, lista.Top),
                Math.Max(0, contenedor.ClientSize.Width - lista.Right),
                Math.Max(0, contenedor.ClientSize.Height - lista.Bottom));
            lista.AutoScroll = false;

            barra = new Panel
            {
                Height = AltoBarraActual(),
                Visible = false,
                BackColor = Color.Transparent,
                Padding = Padding.Empty
            };
            anterior = CrearBoton("‹");
            siguiente = CrearBoton("›");
            pagina = new Label
            {
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Font = lista.Font
            };
            anterior.Size = new Size(48, 42);
            siguiente.Size = new Size(48, 42);
            barra.Controls.Add(pagina);
            barra.Controls.Add(siguiente);
            barra.Controls.Add(anterior);
            contenedor.Controls.Add(barra);
            AplicarBounds();

            anterior.Click += (s, e) => CambiarPagina(indicePagina - 1);
            siguiente.Click += (s, e) => CambiarPagina(indicePagina + 1);
            lista.ControlAdded += Lista_ControlChanged;
            lista.ControlRemoved += Lista_ControlChanged;
            lista.HandleCreated += (s, e) =>
            {
                actualizacionPendiente = false;
                ProgramarActualizacion();
            };
            lista.Resize += (s, e) => ProgramarRecalculo();
            contenedor.Resize += (s, e) => ProgramarRecalculo();
            lista.Disposed += Lista_Disposed;
            CapturarElementosActuales();
            ProgramarActualizacion();
        }

        private static AntdUI.Button CrearBoton(string texto)
        {
            return new AntdUI.Button
            {
                Text = texto,
                BackColor = Color.FromArgb(71, 104, 180),
                BackHover = Color.FromArgb(89, 133, 225),
                BackActive = Color.FromArgb(89, 133, 225),
                DefaultBack = Color.FromArgb(71, 104, 180),
                DefaultBorderColor = Color.Transparent,
                BorderWidth = 0F,
                Radius = 14,
                Icon = null,
                AutoSize = false,
                Padding = Padding.Empty,
                Font = FuenteBotonesDiseno,
                Margin = Padding.Empty,
                TabStop = false
            };
        }

        private void Lista_ControlChanged(object sender, ControlEventArgs e)
        {
            if (aplicandoPagina) return;
            if (lista.Controls.Contains(e.Control))
            {
                if (EsEncabezadoCategoria(e.Control))
                {
                    lista.Controls.Remove(e.Control);
                    encabezado = e.Control;
                    if (!barra.Controls.Contains(encabezado)) barra.Controls.Add(encabezado);
                }
                else if (!elementos.Contains(e.Control)) elementos.Add(e.Control);
            }
            else elementos.Remove(e.Control);
            ProgramarRecalculo();
        }

        private void Lista_Disposed(object sender, EventArgs e)
        {
            foreach (Control control in elementos)
                if (control != null && !control.IsDisposed) control.Dispose();
            elementos.Clear();
        }

        private void ProgramarActualizacion()
        {
            if (lista.IsDisposed || actualizacionPendiente) return;
            if (!lista.IsHandleCreated) return;
            actualizacionPendiente = true;
            EjecutarEnUI(() =>
            {
                actualizacionPendiente = false;
                if (lista.IsDisposed) return;
                Recalcular(true);
            });
        }

        private void ProgramarRecalculo()
        {
            if (lista.IsDisposed || recalculoPendiente) return;
            if (!lista.IsHandleCreated) return;
            recalculoPendiente = true;
            EjecutarEnUI(() =>
            {
                recalculoPendiente = false;
                if (!lista.IsDisposed) Recalcular(false);
            });
        }

        private void EjecutarEnUI(Action accion)
        {
            if (!lista.IsHandleCreated) return;
            try { lista.BeginInvoke(accion); }
            catch (InvalidOperationException) { }
        }

        private void CapturarElementosActuales()
        {
            elementos.Clear();
            foreach (Control control in lista.Controls)
            {
                if (EsEncabezadoCategoria(control))
                {
                    encabezado = control;
                    if (!barra.Controls.Contains(encabezado)) barra.Controls.Add(encabezado);
                }
                else elementos.Add(control);
            }
        }

        public void Reemplazar(Control[] controlesNuevos)
        {
            var nuevos = new List<Control>();
            Control nuevoEncabezado = null;
            if (controlesNuevos != null)
                foreach (Control control in controlesNuevos)
                {
                    if (control == null) continue;
                    if (EsEncabezadoCategoria(control)) nuevoEncabezado = control;
                    else nuevos.Add(control);
                }

            aplicandoPagina = true;
            lista.SuspendLayout();
            try
            {
                var conservar = new HashSet<Control>(nuevos);
                if (nuevoEncabezado != null) conservar.Add(nuevoEncabezado);
                foreach (Control anterior in elementos)
                    if (anterior != null && !anterior.IsDisposed && !conservar.Contains(anterior)) anterior.Dispose();
                if (encabezado != null && !encabezado.IsDisposed && !conservar.Contains(encabezado))
                {
                    barra.Controls.Remove(encabezado);
                    encabezado.Dispose();
                }
                lista.Controls.Clear();
                if (encabezado != null && !ReferenceEquals(encabezado, nuevoEncabezado))
                    barra.Controls.Remove(encabezado);
                encabezado = nuevoEncabezado;
                if (encabezado != null && !barra.Controls.Contains(encabezado)) barra.Controls.Add(encabezado);
                elementos.Clear();
                elementos.AddRange(nuevos);
                indicePagina = 0;
                Recalcular(false);
            }
            finally
            {
                lista.ResumeLayout(true);
                aplicandoPagina = false;
            }
        }

        private static bool EsEncabezadoCategoria(Control control)
        {
            Label etiqueta = control as Label;
            if (etiqueta == null) return false;
            string texto = (etiqueta.Text ?? String.Empty).Trim();
            return texto.Equals("Mostrando aplicaciones", StringComparison.OrdinalIgnoreCase) ||
                texto.Equals("Mostrando páginas web", StringComparison.OrdinalIgnoreCase);
        }

        private void Recalcular(bool restablecerPagina)
        {
            if (restablecerPagina) indicePagina = 0;
            elementosPorPagina = CalcularCapacidad();
            int paginas = Math.Max(1, (elementos.Count + elementosPorPagina - 1) / elementosPorPagina);
            indicePagina = Math.Max(0, Math.Min(indicePagina, paginas - 1));
            barra.Visible = elementos.Count > 0 || encabezado != null;
            AplicarBounds();
            elementosPorPagina = CalcularCapacidad();
            paginas = Math.Max(1, (elementos.Count + elementosPorPagina - 1) / elementosPorPagina);
            indicePagina = Math.Max(0, Math.Min(indicePagina, paginas - 1));
            pagina.Text = (indicePagina + 1).ToString() + " / " + paginas.ToString();
            anterior.Enabled = true;
            siguiente.Enabled = true;
            AplicarPagina();
            ActualizarColores();
        }

        private int CalcularCapacidad()
        {
            if (elementos.Count == 0) return 1;
            int anchoItem = 1;
            int altoItem = 1;
            foreach (Control muestra in elementos)
            {
                anchoItem = Math.Max(anchoItem,
                    Math.Max(muestra.Width, muestra.PreferredSize.Width) + muestra.Margin.Horizontal);
                altoItem = Math.Max(altoItem,
                    Math.Max(muestra.Height, muestra.PreferredSize.Height) + muestra.Margin.Vertical);
            }
            int columnas = Math.Max(1, lista.ClientSize.Width / Math.Max(1, anchoItem));
            int filas = Math.Max(1, lista.ClientSize.Height / Math.Max(1, altoItem));
            if (lista.FlowDirection == FlowDirection.TopDown)
                return Math.Max(1, filas);
            return Math.Max(1, columnas * filas);
        }

        private void AplicarBounds()
        {
            if (contenedor == null || lista.IsDisposed) return;
            float escala = EscalaBarra();
            int altoBarra = AltoBarraActual();
            int margen = Math.Max(1, (int)Math.Round(4 * escala));
            // Las flechas conservan el mínimo táctil incluso cuando el
            // formulario se reduce unos píxeles para caber en el área visible.
            int anchoBoton = Math.Max(48, (int)Math.Round(48 * escala));
            int altoBoton = Math.Max(42, (int)Math.Round(42 * escala));
            anterior.Size = new Size(anchoBoton, altoBoton);
            siguiente.Size = new Size(anchoBoton, altoBoton);
            int margenIzquierdo = (int)Math.Round(margenOriginal.Left * escala);
            int margenSuperior = (int)Math.Round(margenOriginal.Top * escala);
            int margenHorizontal = (int)Math.Round(margenOriginal.Horizontal * escala);
            int margenVertical = (int)Math.Round(margenOriginal.Vertical * escala);
            int ancho = Math.Max(0, contenedor.ClientSize.Width - margenHorizontal);
            int alto = Math.Max(0, contenedor.ClientSize.Height - margenVertical);
            int espacioBarra = barra.Visible ? altoBarra : 0;
            lista.SetBounds(margenIzquierdo, margenSuperior, ancho,
                Math.Max(0, alto - espacioBarra));
            barra.SetBounds(margenIzquierdo,
                margenSuperior + Math.Max(0, alto - altoBarra), ancho, altoBarra);
            anterior.SetBounds(margen, Math.Max(0, (altoBarra - altoBoton) / 2), anchoBoton, altoBoton);
            int anchoBarra = barra.ClientSize.Width;
            if (encabezado != null && !encabezado.IsDisposed)
            {
                // Reserve the controls on the right while allowing the category
                // label to use the remaining width. The old fixed 220x26 box
                // clipped both the rounded label and its text on frmTareas.
                int siguienteX = Math.Max(margen, anchoBarra - anchoBoton - margen);
                int anteriorX = Math.Max(margen, siguienteX - anchoBoton - margen);
                int anchoContadorMinimo = Math.Max(1, (int)Math.Round(32 * escala));
                int anchoEncabezado = Math.Max(0,
                    anteriorX - 4 * margen - anchoContadorMinimo);
                encabezado.SetBounds(margen, Math.Max(0, (altoBarra - (int)Math.Round(38 * escala)) / 2),
                    anchoEncabezado, Math.Max(1, (int)Math.Round(38 * escala)));
                anterior.SetBounds(anteriorX, Math.Max(0, (altoBarra - altoBoton) / 2),
                    anchoBoton, altoBoton);
                siguiente.SetBounds(siguienteX, Math.Max(0, (altoBarra - altoBoton) / 2),
                    anchoBoton, altoBoton);
                int contadorX = encabezado.Right + margen;
                int contadorDerecha = Math.Max(contadorX + 1, anterior.Left - margen);
                pagina.SetBounds(contadorX, 0, contadorDerecha - contadorX, altoBarra);
            }
            else
            {
                siguiente.SetBounds(Math.Max(margen, anchoBarra - anchoBoton - margen),
                    Math.Max(0, (altoBarra - altoBoton) / 2), anchoBoton, altoBoton);
                pagina.SetBounds(anterior.Right + margen, 0,
                    Math.Max(1, siguiente.Left - anterior.Right - 2 * margen), altoBarra);
            }
        }

        private float EscalaBarra()
        {
            return AjustePantalla.EscalaActual(lista);
        }

        private int AltoBarraActual()
        {
            return Math.Max(42, (int)Math.Round(AltoBarraDiseno * EscalaBarra()));
        }

        private void AplicarPagina()
        {
            if (lista.IsDisposed) return;
            int inicio = indicePagina * elementosPorPagina;
            int cantidad = Math.Max(0, Math.Min(elementosPorPagina, elementos.Count - inicio));
            lista.SuspendLayout();
            aplicandoPagina = true;
            try
            {
                lista.Controls.Clear();
                if (cantidad > 0)
                {
                    var paginaActual = new Control[cantidad];
                    elementos.CopyTo(inicio, paginaActual, 0, cantidad);
                    lista.Controls.AddRange(paginaActual);
                }
            }
            finally
            {
                aplicandoPagina = false;
                lista.ResumeLayout(true);
            }
        }

        private void CambiarPagina(int indice)
        {
            int paginas = Math.Max(1, (elementos.Count + elementosPorPagina - 1) / elementosPorPagina);
            if (indice < 0 || indice >= paginas || indice == indicePagina) return;
            indicePagina = indice;
            pagina.Text = (indicePagina + 1).ToString() + " / " + paginas.ToString();
            anterior.Enabled = true;
            siguiente.Enabled = true;
            AplicarPagina();
        }

        private void ActualizarColores()
        {
            Color fondo = contenedor.BackColor;
            for (Control actual = contenedor; actual != null && fondo.A < 255; actual = actual.Parent)
                fondo = actual.BackColor;
            Color tinta = PaletaAplicacion.ColorTextoContraste(fondo);
            pagina.ForeColor = tinta;
            Color hover = ControlPaint.Light(fondo, 0.12f);
            foreach (AntdUI.Button boton in new[] { anterior, siguiente })
            {
                boton.BackColor = fondo;
                boton.DefaultBack = fondo;
                boton.BackHover = hover;
                boton.BackActive = hover;
                boton.ForeColor = tinta;
                boton.DefaultBorderColor = Color.Transparent;
                boton.BorderWidth = 0F;
                boton.Radius = 14;
            }
        }
    }

    /// <summary>Indicador pequeño y doblemente almacenado; solo invalida su propio rectángulo.</summary>
    internal sealed class IndicadorCarga : Control
    {
        private readonly Timer temporizador;
        private float angulo;
        private string mensaje = String.Empty;

        public IndicadorCarga()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(440, 48);
            Margin = new Padding(8);
            temporizador = new Timer { Interval = 16 };
            temporizador.Tick += (s, e) =>
            {
                angulo = (angulo + 7.2f) % 360f;
                Invalidate(new Rectangle(0, 0, Math.Min(44, Width), Height));
            };
        }

        public string Mensaje
        {
            get { return mensaje; }
            set
            {
                mensaje = value ?? String.Empty;
                Invalidate();
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible && IsHandleCreated) temporizador.Start();
            else temporizador.Stop();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (Visible) temporizador.Start();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            temporizador.Stop();
            base.OnHandleDestroyed(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            float escala = AjustePantalla.EscalaActual(this);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float lado = Math.Max(18 * escala, Math.Min(28 * escala, Height - 12 * escala));
            var circulo = new RectangleF(8 * escala, (Height - lado) / 2f, lado, lado);
            Color tinta = PaletaAplicacion.ColorTextoContraste(this);
            using (var lapizFondo = new Pen(Color.FromArgb(55, tinta), 3f * escala))
                g.DrawEllipse(lapizFondo, circulo);
            using (var lapiz = new Pen(PaletaAplicacion.AzulPrimario, 3f * escala))
            {
                lapiz.StartCap = LineCap.Round;
                lapiz.EndCap = LineCap.Round;
                g.DrawArc(lapiz, circulo, angulo, 105f);
            }

            var areaTexto = new Rectangle((int)(circulo.Right + 12 * escala), 0,
                Math.Max(0, Width - (int)circulo.Right - (int)(16 * escala)), Height);
            TextRenderer.DrawText(g, mensaje, Font, areaTexto, tinta,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) temporizador.Dispose();
            base.Dispose(disposing);
        }
    }

    internal static class ControlesPresentacion
    {
        private static readonly System.Reflection.PropertyInfo DobleBuffer =
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        private static readonly ConditionalWeakTable<Control, object> ControlesEnganchados =
            new ConditionalWeakTable<Control, object>();
        private static readonly ConditionalWeakTable<BufferedFlowLayoutPanel, PaginadorFlowLayout> Paginadores =
            new ConditionalWeakTable<BufferedFlowLayoutPanel, PaginadorFlowLayout>();
        private const int WmSetRedraw = 0x000B;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public static void Optimizar(Control control)
        {
            if (control == null) return;
            BufferedFlowLayoutPanel lista = control as BufferedFlowLayoutPanel;
            if (lista != null && lista.AutoScroll && lista.PaginacionHabilitada && !FormularioSuave.EnModoDiseno)
                Paginadores.GetValue(lista, panel => new PaginadorFlowLayout(panel));

            bool necesitaBuffer = control is Form || control is UserControl ||
                control is ScrollableControl ||
                (control is ListBox && ((ListBox)control).DrawMode != DrawMode.Normal);
            if (necesitaBuffer)
            {
                try { if (DobleBuffer != null) DobleBuffer.SetValue(control, true, null); }
                catch { }
            }

            object marcador;
            if (ControlesEnganchados.TryGetValue(control, out marcador)) return;
            try { ControlesEnganchados.Add(control, new object()); }
            catch (ArgumentException) { return; }
            control.ControlAdded += (s, e) => Optimizar(e.Control);

            foreach (Control hijo in control.Controls)
            {
                Optimizar(hijo);
            }
        }

        public static void PrepararPaginacion(BufferedFlowLayoutPanel lista)
        {
            if (lista == null || FormularioSuave.EnModoDiseno) return;
            Paginadores.GetValue(lista, panel => new PaginadorFlowLayout(panel));
        }

        public static void IniciarActualizacionVisual(Control control)
        {
            if (control != null && control.IsHandleCreated)
                SendMessage(control.Handle, WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
        }

        public static void FinalizarActualizacionVisual(Control control)
        {
            if (control == null || control.IsDisposed) return;
            if (control.IsHandleCreated)
                SendMessage(control.Handle, WmSetRedraw, new IntPtr(1), IntPtr.Zero);
            control.Invalidate(true);
        }

        internal static void SuspenderRedibujadoArbol(Control control, List<Control> suspendidos)
        {
            if (control == null || control.IsDisposed || suspendidos == null) return;
            // No desactivar WM_SETREDRAW en el formulario de nivel superior:
            // Windows puede quitar WS_VISIBLE mientras procesa ese mensaje.
            foreach (Control hijo in control.Controls)
            {
                if (!hijo.IsDisposed && hijo.IsHandleCreated && hijo.Visible)
                {
                    SendMessage(hijo.Handle, WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
                    suspendidos.Add(hijo);
                }
                SuspenderRedibujadoArbol(hijo, suspendidos);
            }
        }

        internal static void ReanudarRedibujadoArbol(List<Control> suspendidos)
        {
            if (suspendidos == null) return;
            for (int i = suspendidos.Count - 1; i >= 0; i--)
            {
                Control control = suspendidos[i];
                if (control != null && !control.IsDisposed && control.IsHandleCreated)
                    SendMessage(control.Handle, WmSetRedraw, new IntPtr(1), IntPtr.Zero);
            }
        }

        public static void Reemplazar(Control contenedor, params Control[] nuevos)
        {
            if (contenedor == null) return;
            BufferedFlowLayoutPanel lista = contenedor as BufferedFlowLayoutPanel;
            PaginadorFlowLayout paginador;
            if (lista != null && Paginadores.TryGetValue(lista, out paginador))
            {
                paginador.Reemplazar(nuevos);
                return;
            }
            contenedor.SuspendLayout();
            bool redibujar = false;
            try
            {
                if (contenedor.IsHandleCreated)
                {
                    SendMessage(contenedor.Handle, WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
                    redibujar = true;
                }
                Control[] anteriores = new Control[contenedor.Controls.Count];
                contenedor.Controls.CopyTo(anteriores, 0);
                contenedor.Controls.Clear();
                foreach (Control anterior in anteriores)
                    anterior.Dispose();
                if (nuevos != null && nuevos.Length > 0)
                    contenedor.Controls.AddRange(nuevos);
            }
            finally
            {
                contenedor.ResumeLayout(false);
                if (redibujar)
                    SendMessage(contenedor.Handle, WmSetRedraw, new IntPtr(1), IntPtr.Zero);
                contenedor.PerformLayout();
                contenedor.Invalidate(true);
            }
        }
    }

    /// <summary>Escala formularios existentes como una composición proporcional al tamaño disponible.</summary>
    internal static class AjustePantalla
    {
        // Lienzo común usado al diseñar los formularios, expresado a 96 DPI.
        private const int AnchoLienzoDiseno = 1946;
        private const int AltoLienzoDiseno = 1050;
        private const int DpiDiseno = 96;
        private const uint RedrawInvalidate = 0x0001;
        private const uint RedrawErase = 0x0004;
        private const uint RedrawAllChildren = 0x0080;
        private const uint RedrawUpdateNow = 0x0100;
        private const uint RedrawFrame = 0x0400;
        private const uint SwpNoZOrder = 0x0004;
        private const uint SwpNoActivate = 0x0010;
        private const uint SwpNoOwnerZOrder = 0x0200;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate,
            IntPtr hrgnUpdate, uint flags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int x, int y, int width, int height, uint flags);

        private sealed class ControlBase
        {
            public Rectangle Bounds;
            public Font Font;
            public Padding Margin;
            public Padding Padding;
            public Size MinimumSize;
            public Size MaximumSize;
            public int? AltoElementoLista;
            public int? AltoElementoArbol;
            public readonly Dictionary<ColumnHeader, int> AnchosColumnasListView =
                new Dictionary<ColumnHeader, int>();
            public Size? TamanoImagenesToolStrip;
            public Size? TamanoElementoTab;
            public Font FuenteGrid;
            public Font FuenteEncabezadoGrid;
            public Font FuenteEncabezadoFilaGrid;
            public readonly Dictionary<ToolStripItem, Font> FuentesItemsBarra =
                new Dictionary<ToolStripItem, Font>();
            public readonly Dictionary<ToolStripItem, Padding> MargenesItemsBarra =
                new Dictionary<ToolStripItem, Padding>();
            public readonly Dictionary<ToolStripItem, Padding> RellenosItemsBarra =
                new Dictionary<ToolStripItem, Padding>();
            public readonly Dictionary<ImageList, Size> TamanosImageList =
                new Dictionary<ImageList, Size>();
            public Padding? RellenoGrid;
            public Padding? RellenoEncabezadoGrid;
            public Padding? RellenoEncabezadoFilaGrid;
            public int? AltoFilaGrid;
            public readonly Dictionary<DataGridViewColumn, int> AnchosColumnasGrid =
                new Dictionary<DataGridViewColumn, int>();
            public readonly Dictionary<DataGridViewRow, int> AltosFilasGrid =
                new Dictionary<DataGridViewRow, int>();
            public readonly Dictionary<string, float> PropiedadesVisuales =
                new Dictionary<string, float>(StringComparer.Ordinal);
        }

        private sealed class EstiloTabla
        {
            public TableLayoutPanel Tabla;
            public float[] AnchosColumnas;
            public float[] AltosFilas;
        }

        private sealed class Registro
        {
            public bool Preparado;
            public bool Ajustando;
            public bool ReajusteCompletoEnCurso;
            public bool Capturado;
            public bool EventosRegistrados;
            public Size ClienteDiseno;
            public float EscalaX = float.NaN;
            public float EscalaY = float.NaN;
            public int DpiActual;
            public int DpiAnterior;
            public bool PosicionDisenoCapturada;
            public bool CentrarEnPropietario;
            public PointF DesplazamientoCentroDiseno;
            public readonly Dictionary<Control, ControlBase> Controles = new Dictionary<Control, ControlBase>();
            public readonly List<EstiloTabla> Tablas = new List<EstiloTabla>();
            public readonly Dictionary<Control, Font> FuentesAplicadas = new Dictionary<Control, Font>();
            public readonly Dictionary<Control, List<Font>> FuentesEspecialesAplicadas =
                new Dictionary<Control, List<Font>>();
            public readonly List<Font> FuentesBase = new List<Font>();
            public readonly List<Font> FuentesCreadas = new List<Font>();
            public readonly HashSet<Control> ContenedoresRegistrados = new HashSet<Control>();
            public EventHandler CambioResolucion;
            public DpiChangedEventHandler CambioDpi;
            public EventHandler CambioTamano;
            public EventHandler CambioUbicacion;
            public System.Windows.Forms.Timer TemporizadorReajuste;
            public int DpiPendiente;
            public Rectangle? RectanguloSugeridoPendiente;
            public string EtapaPendiente;
            public string Monitor;
        }

        private static readonly ConditionalWeakTable<Form, Registro> Formularios =
            new ConditionalWeakTable<Form, Registro>();

        public static void Preparar(Form formulario)
        {
            if (formulario == null || formulario.IsDisposed) return;
            formulario.AutoScaleMode = AutoScaleMode.None;
            Registro registro = Formularios.GetValue(formulario, _ => new Registro());
            if (!registro.Preparado)
            {
                // El constructor de FormularioSuave llama antes de que exista el
                // árbol del Designer. Load queda como respaldo para esos casos.
                registro.Preparado = true;
                formulario.Load += (s, e) => AplicarAntesDeMostrar(formulario, registro);
                // StartPosition y el monitor real ya están resueltos en Shown.
                formulario.Shown += (s, e) => AplicarAlMostrar(formulario, registro);
            }

            // Los llamadores vuelven a llamar Preparar después de
            // InitializeComponent (o antes de Show). Capturar aquí preserva el
            // ClientSize original antes de que Windows lo limite al área visible.
            if (!registro.Capturado && formulario.Controls.Count > 0)
                CapturarYRegistrar(formulario, registro);
        }

        private static void AplicarAntesDeMostrar(Form formulario, Registro registro)
        {
            if (formulario == null || formulario.IsDisposed || registro.Capturado)
                return;
            try
            {
                CapturarYRegistrar(formulario, registro);
            }
            catch (Exception ex) { DesactivarRegistro(formulario, registro, ex, "escalado inicial"); }
        }

        private static void CapturarYRegistrar(Form formulario, Registro registro)
        {
            if (formulario == null || formulario.IsDisposed) return;
            if (!registro.Capturado) CapturarDiseno(formulario, registro);
            if (registro.EventosRegistrados) return;
            RegistrarControlesDinamicos(formulario, registro);
            RegistrarEventos(formulario, registro);
            registro.EventosRegistrados = true;
        }

        private static void AplicarAlMostrar(Form formulario, Registro registro)
        {
            if (formulario == null || formulario.IsDisposed) return;
            try
            {
                if (!registro.Capturado) AplicarAntesDeMostrar(formulario, registro);
                if (registro.Capturado)
                    AjustarAlAreaVisible(formulario, registro, formulario.DeviceDpi, null);
            }
            catch (Exception ex) { DesactivarRegistro(formulario, registro, ex, "escalado al mostrar"); }
        }

        public static float EscalaActual(Control control)
        {
            Form formulario = control as Form ?? (control == null ? null : control.FindForm());
            Registro registro;
            if (formulario == null || !Formularios.TryGetValue(formulario, out registro) ||
                !registro.Capturado || registro.ClienteDiseno.Width <= 0 || registro.ClienteDiseno.Height <= 0)
                return 1f;
            return Math.Max(0.01f, Math.Min(
                (float)formulario.ClientSize.Width / registro.ClienteDiseno.Width,
                (float)formulario.ClientSize.Height / registro.ClienteDiseno.Height));
        }

        private static void RegistrarEventos(Form formulario, Registro registro)
        {
            registro.CambioResolucion = (s, e) => ProgramarReajuste(
                formulario, registro, formulario.DeviceDpi, null, "cambio de resolución");
            registro.CambioDpi = (s, e) =>
            {
                // DpiChanged es la notificación administrada de WM_DPICHANGED.
                // DeviceDpiNew se usa contra el diseño base, nunca contra el
                // tamaño que tenía la ventana antes del cambio.
                Trace.TraceInformation(String.Format(
                    "Cambio DPI en {0}: {1} -> {2}; rectángulo sugerido {3}.",
                    formulario.Name, e.DeviceDpiOld, e.DeviceDpiNew, e.SuggestedRectangle));
                ProgramarReajuste(formulario, registro, e.DeviceDpiNew,
                    e.SuggestedRectangle, "cambio de DPI");
            };
            registro.CambioTamano = (s, e) =>
            {
                if (registro.Ajustando || registro.ReajusteCompletoEnCurso ||
                    !registro.Capturado || formulario.IsDisposed ||
                    formulario.WindowState != FormWindowState.Normal) return;
                ProgramarReajuste(formulario, registro, formulario.DeviceDpi, null,
                    "cambio de tamaño");
            };
            registro.CambioUbicacion = (s, e) =>
            {
                if (registro.Ajustando || registro.ReajusteCompletoEnCurso ||
                    !registro.Capturado || formulario.IsDisposed ||
                    !formulario.IsHandleCreated) return;
                Screen pantallaActual = Screen.FromControl(formulario);
                if (String.Equals(registro.Monitor, pantallaActual.DeviceName, StringComparison.OrdinalIgnoreCase))
                    return;
                ProgramarReajuste(formulario, registro, formulario.DeviceDpi, null,
                    "cambio de monitor");
            };
            registro.TemporizadorReajuste = new System.Windows.Forms.Timer { Interval = 250 };
            registro.TemporizadorReajuste.Tick += (s, e) => AplicarReajustePendiente(formulario, registro);
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += registro.CambioResolucion;
            formulario.DpiChanged += registro.CambioDpi;
            formulario.SizeChanged += registro.CambioTamano;
            formulario.LocationChanged += registro.CambioUbicacion;
            formulario.FormClosed += (s, e) =>
            {
                LiberarEventos(formulario, registro);
            };
        }

        internal static void NotificarCambioResolucion(Form formulario)
        {
            Registro registro;
            if (formulario == null || !Formularios.TryGetValue(formulario, out registro) || !registro.Capturado)
                return;
            ProgramarReajuste(formulario, registro, formulario.DeviceDpi, null,
                "WM_DISPLAYCHANGE");
        }

        private static void ProgramarReajuste(Form formulario, Registro registro,
            int dpi, Rectangle? rectanguloSugerido, string etapa)
        {
            if (formulario == null || formulario.IsDisposed || !formulario.IsHandleCreated) return;
            try
            {
                Action programar = () =>
                {
                    if (formulario.IsDisposed || registro.TemporizadorReajuste == null) return;
                    // Windows puede emitir varios de estos avisos por un solo
                    // cambio de pantalla. Conserva los últimos valores y espera
                    // a que termine la ráfaga antes de tocar el árbol de controles.
                    registro.DpiPendiente = Math.Max(1, dpi);
                    // WM_DPICHANGED identifica el monitor de destino. Un aviso
                    // posterior de resolución no trae rectángulo: conserva el
                    // último destino concreto recibido durante esta ráfaga.
                    if (rectanguloSugerido.HasValue)
                        registro.RectanguloSugeridoPendiente = rectanguloSugerido;
                    registro.EtapaPendiente = etapa;
                    registro.TemporizadorReajuste.Stop();
                    registro.TemporizadorReajuste.Start();
                };
                if (formulario.InvokeRequired) formulario.BeginInvoke(programar);
                else programar();
            }
            catch (InvalidOperationException ex)
            {
                Trace.TraceError("No se pudo ajustar el formulario durante " + etapa + ": " + ex);
            }
        }

        private static void AplicarReajustePendiente(Form formulario, Registro registro)
        {
            if (registro.TemporizadorReajuste != null) registro.TemporizadorReajuste.Stop();
            if (formulario == null || formulario.IsDisposed || !registro.Capturado) return;
            // DeviceDpi puede ir un mensaje detrás de DpiChanged mientras
            // Windows termina la transición. El temporizador deja que concluya.
            int dpi = formulario.DeviceDpi > 0 ? formulario.DeviceDpi :
                (registro.DpiPendiente > 0 ? registro.DpiPendiente : DpiDiseno);
            Rectangle? sugerido = registro.RectanguloSugeridoPendiente;
            string etapa = registro.EtapaPendiente ?? "cambio de pantalla";
            registro.DpiPendiente = 0;
            registro.RectanguloSugeridoPendiente = null;
            registro.EtapaPendiente = null;
            try { AjustarAlAreaVisible(formulario, registro, dpi, sugerido); }
            catch (Exception ex) { DesactivarRegistro(formulario, registro, ex, etapa); }
        }

        private static void LiberarEventos(Form formulario, Registro registro)
        {
            if (registro.CambioResolucion != null)
                Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= registro.CambioResolucion;
            if (registro.CambioDpi != null)
                formulario.DpiChanged -= registro.CambioDpi;
            if (registro.CambioTamano != null)
                formulario.SizeChanged -= registro.CambioTamano;
            if (registro.CambioUbicacion != null)
                formulario.LocationChanged -= registro.CambioUbicacion;
            if (registro.TemporizadorReajuste != null)
            {
                registro.TemporizadorReajuste.Stop();
                registro.TemporizadorReajuste.Dispose();
                registro.TemporizadorReajuste = null;
            }
            foreach (Font fuente in registro.FuentesCreadas)
                fuente.Dispose();
            registro.FuentesCreadas.Clear();
            foreach (Font fuente in registro.FuentesBase)
                fuente.Dispose();
            registro.FuentesBase.Clear();
            registro.FuentesAplicadas.Clear();
            registro.FuentesEspecialesAplicadas.Clear();
        }

        private static void DesactivarRegistro(Form formulario, Registro registro, Exception ex, string etapa)
        {
            Trace.TraceError("Se desactivó el escalado del formulario " + formulario.Name + " durante " + etapa + ": " + ex);
            if (registro.CambioResolucion != null)
            {
                Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= registro.CambioResolucion;
                registro.CambioResolucion = null;
            }
        }

        private static void CapturarDiseno(Control raiz, Registro registro)
        {
            if (registro.Capturado || raiz.IsDisposed) return;
            registro.ClienteDiseno = raiz.ClientSize;
            Trace.TraceInformation(String.Format(
                "Diseño capturado para {0}: ClientSize original {1}x{2}.",
                raiz.Name, registro.ClienteDiseno.Width, registro.ClienteDiseno.Height));
            Form formularioBase = raiz as Form;
            if (formularioBase != null)
            {
                registro.DpiActual = formularioBase.DeviceDpi;
                registro.DpiAnterior = formularioBase.DeviceDpi;
            }
            CapturarArbol(raiz, registro, 1f, 1f);
            DesactivarAutoescaladoWinForms(raiz);
            registro.Capturado = true;
        }

        private static void CapturarArbol(Control control, Registro registro, float escalaXBase, float escalaYBase)
        {
            ContainerControl contenedorEscalable = control as ContainerControl;
            if (contenedorEscalable != null)
                contenedorEscalable.AutoScaleMode = AutoScaleMode.None;

            if (!registro.Controles.ContainsKey(control))
            {
                float escalaFuenteBase = (escalaXBase + escalaYBase) / 2f;
                Font fuente = control.Font == null ? null : new Font(control.Font.FontFamily,
                    Math.Max(0.1f, control.Font.Size / Math.Max(0.001f, escalaFuenteBase)),
                    control.Font.Style, control.Font.Unit, control.Font.GdiCharSet,
                    control.Font.GdiVerticalFont);
                if (fuente != null) registro.FuentesBase.Add(fuente);
                Rectangle bounds = control.Bounds;
                registro.Controles.Add(control, new ControlBase
                {
                    Bounds = new Rectangle(
                        (int)Math.Round(bounds.X / Math.Max(0.001f, escalaXBase)),
                        (int)Math.Round(bounds.Y / Math.Max(0.001f, escalaYBase)),
                        Math.Max(1, (int)Math.Round(bounds.Width / Math.Max(0.001f, escalaXBase))),
                        Math.Max(1, (int)Math.Round(bounds.Height / Math.Max(0.001f, escalaYBase)))),
                    Font = fuente,
                    Margin = EscalarPadding(control.Margin, 1f / escalaXBase, 1f / escalaYBase),
                    Padding = EscalarPadding(control.Padding, 1f / escalaXBase, 1f / escalaYBase),
                    MinimumSize = EscalarTamano(control.MinimumSize, 1f / escalaXBase, 1f / escalaYBase),
                    MaximumSize = EscalarTamano(control.MaximumSize, 1f / escalaXBase, 1f / escalaYBase),
                    AltoElementoLista = control is ListBox ? (int?)((ListBox)control).ItemHeight : null,
                    AltoElementoArbol = control is TreeView ? (int?)((TreeView)control).ItemHeight : null,
                    TamanoImagenesToolStrip = control is ToolStrip ? (Size?)((ToolStrip)control).ImageScalingSize : null
                });
                CapturarPropiedadesVisuales(control, registro.Controles[control]);
                CapturarMetricasGrid(control, registro.Controles[control]);
                CapturarMetricasEspeciales(control, registro.Controles[control], registro);
                ListView listaVista = control as ListView;
                if (listaVista != null)
                    foreach (ColumnHeader columna in listaVista.Columns)
                        if (columna.Width > 0)
                            registro.Controles[control].AnchosColumnasListView[columna] = columna.Width;
            }

            TableLayoutPanel tabla = control as TableLayoutPanel;
            if (tabla != null && !registro.Tablas.Exists(t => ReferenceEquals(t.Tabla, tabla)))
            {
                var estilo = new EstiloTabla
                {
                    Tabla = tabla,
                    AnchosColumnas = new float[tabla.ColumnStyles.Count],
                    AltosFilas = new float[tabla.RowStyles.Count]
                };
                for (int i = 0; i < tabla.ColumnStyles.Count; i++)
                    estilo.AnchosColumnas[i] = tabla.ColumnStyles[i].Width / Math.Max(0.001f, escalaXBase);
                for (int i = 0; i < tabla.RowStyles.Count; i++)
                    estilo.AltosFilas[i] = tabla.RowStyles[i].Height / Math.Max(0.001f, escalaYBase);
                registro.Tablas.Add(estilo);
            }

            foreach (Control hijo in control.Controls)
                CapturarArbol(hijo, registro, escalaXBase, escalaYBase);
        }

        private static void CapturarMetricasEspeciales(Control control, ControlBase baseControl, Registro registro)
        {
            List<ImageList> listasImagenes = new List<ImageList>();
            ListView listaVista = control as ListView;
            if (listaVista != null)
            {
                if (listaVista.SmallImageList != null) listasImagenes.Add(listaVista.SmallImageList);
                if (listaVista.LargeImageList != null) listasImagenes.Add(listaVista.LargeImageList);
                if (listaVista.StateImageList != null) listasImagenes.Add(listaVista.StateImageList);
            }
            TreeView arbol = control as TreeView;
            if (arbol != null && arbol.ImageList != null) listasImagenes.Add(arbol.ImageList);
            foreach (ImageList lista in listasImagenes)
                if (!baseControl.TamanosImageList.ContainsKey(lista))
                    baseControl.TamanosImageList.Add(lista, lista.ImageSize);
            TabControl pestañas = control as TabControl;
            if (pestañas != null) baseControl.TamanoElementoTab = pestañas.ItemSize;

            DataGridView grid = control as DataGridView;
            if (grid != null)
            {
                baseControl.FuenteGrid = CopiarFuente(grid.DefaultCellStyle.Font ?? grid.Font, registro);
                baseControl.FuenteEncabezadoGrid = CopiarFuente(
                    grid.ColumnHeadersDefaultCellStyle.Font ?? grid.Font, registro);
                baseControl.FuenteEncabezadoFilaGrid = CopiarFuente(
                    grid.RowHeadersDefaultCellStyle.Font ?? grid.Font, registro);
                baseControl.RellenoGrid = grid.DefaultCellStyle.Padding;
                baseControl.RellenoEncabezadoGrid = grid.ColumnHeadersDefaultCellStyle.Padding;
                baseControl.RellenoEncabezadoFilaGrid = grid.RowHeadersDefaultCellStyle.Padding;
            }
            ToolStrip barra = control as ToolStrip;
            if (barra != null) CapturarFuentesItems(barra.Items, baseControl, registro);
            ContextMenuStrip menu = control.ContextMenuStrip;
            if (menu != null) CapturarFuentesItems(menu.Items, baseControl, registro);
        }

        private static void CapturarFuentesItems(ToolStripItemCollection items, ControlBase baseControl, Registro registro)
        {
            foreach (ToolStripItem item in items)
            {
                Font fuente = CopiarFuente(item.Font, registro);
                if (fuente != null) baseControl.FuentesItemsBarra[item] = fuente;
                baseControl.MargenesItemsBarra[item] = item.Margin;
                baseControl.RellenosItemsBarra[item] = item.Padding;
                ToolStripDropDownItem desplegable = item as ToolStripDropDownItem;
                if (desplegable != null) CapturarFuentesItems(desplegable.DropDownItems, baseControl, registro);
            }
        }

        private static Font CopiarFuente(Font fuente, Registro registro)
        {
            if (fuente == null) return null;
            Font copia = new Font(fuente.FontFamily, fuente.Size, fuente.Style,
                fuente.Unit, fuente.GdiCharSet, fuente.GdiVerticalFont);
            registro.FuentesBase.Add(copia);
            return copia;
        }

        private static void CapturarMetricasGrid(Control control, ControlBase baseControl)
        {
            DataGridView rejilla = control as DataGridView;
            if (rejilla == null) return;
            baseControl.AltoFilaGrid = rejilla.RowTemplate.Height;
            foreach (DataGridViewColumn columna in rejilla.Columns)
                if (columna.AutoSizeMode == DataGridViewAutoSizeColumnMode.NotSet ||
                    columna.AutoSizeMode == DataGridViewAutoSizeColumnMode.None)
                    baseControl.AnchosColumnasGrid[columna] = columna.Width;
            foreach (DataGridViewRow fila in rejilla.Rows)
                if (!fila.IsNewRow && fila.Resizable != DataGridViewTriState.False)
                    baseControl.AltosFilasGrid[fila] = fila.Height;
        }

        private static void DesactivarAutoescaladoWinForms(Control raiz)
        {
            ContainerControl raizContenedora = raiz as ContainerControl;
            if (raizContenedora != null) raizContenedora.AutoScaleMode = AutoScaleMode.None;
            foreach (Control control in raiz.Controls)
            {
                // Se conservan Dock/Anchor/AutoSize para que WinForms resuelva
                // el layout tras el escalado. Solo se evita doble DPI scaling.
                DesactivarAutoescaladoWinForms(control);
            }
        }

        private static void AjustarAlAreaVisible(Form formulario, Registro registro)
        {
            AjustarAlAreaVisible(formulario, registro, formulario.DeviceDpi, null);
        }

        private static void AjustarAlAreaVisible(Form formulario, Registro registro,
            int dpi, Rectangle? rectanguloSugerido)
        {
            if (registro == null || registro.ReajusteCompletoEnCurso) return;
            registro.ReajusteCompletoEnCurso = true;
            try
            {
                AjustarAlAreaVisibleInterno(formulario, registro, dpi, rectanguloSugerido);
            }
            finally
            {
                registro.ReajusteCompletoEnCurso = false;
            }
        }

        private static void AjustarAlAreaVisibleInterno(Form formulario, Registro registro,
            int dpi, Rectangle? rectanguloSugerido)
        {
            if (formulario == null || formulario.IsDisposed || !registro.Capturado) return;
            if (registro.TemporizadorReajuste != null)
                registro.TemporizadorReajuste.Stop();
            // No normalizar ni reposicionar una ventana minimizada: hacerlo la
            // haría aparecer mientras el usuario está en otra aplicación.
            if (formulario.WindowState == FormWindowState.Minimized) return;

            Screen pantalla = rectanguloSugerido.HasValue
                ? Screen.FromRectangle(rectanguloSugerido.Value)
                : Screen.FromControl(formulario);
            Rectangle area = pantalla.WorkingArea;
            // El factor depende del lienzo común, no del tamaño del formulario.
            // Así se conserva también el espacio vacío que tenía en el diseño.
            float escalaUniforme = Math.Max(0.01f, Math.Min(
                Math.Min((float)Math.Max(1, dpi) / DpiDiseno,
                    (float)area.Width / AnchoLienzoDiseno),
                (float)area.Height / AltoLienzoDiseno));
            Size objetivo = new Size(
                Math.Max(1, (int)Math.Round(registro.ClienteDiseno.Width * escalaUniforme)),
                Math.Max(1, (int)Math.Round(registro.ClienteDiseno.Height * escalaUniforme)));
            bool mismoEscalado = !float.IsNaN(registro.EscalaX) &&
                Math.Abs(registro.EscalaX - escalaUniforme) < 0.0001f &&
                Math.Abs(registro.EscalaY - escalaUniforme) < 0.0001f &&
                formulario.ClientSize == objetivo && registro.DpiActual == formulario.DeviceDpi &&
                String.Equals(registro.Monitor, pantalla.DeviceName, StringComparison.OrdinalIgnoreCase) &&
                area.Contains(formulario.Bounds);
            if (mismoEscalado && formulario.WindowState == FormWindowState.Normal) return;

            FormWindowState estadoAnterior = formulario.WindowState;
            if (!registro.PosicionDisenoCapturada)
            {
                registro.CentrarEnPropietario = formulario.StartPosition == FormStartPosition.CenterParent &&
                    formulario.Owner != null && !formulario.Owner.IsDisposed;
                Rectangle anclaInicial = registro.CentrarEnPropietario
                    ? formulario.Owner.Bounds : area;
                float centroAnclaX = anclaInicial.Left + anclaInicial.Width / 2f;
                float centroAnclaY = anclaInicial.Top + anclaInicial.Height / 2f;
                registro.DesplazamientoCentroDiseno = new PointF(
                    formulario.Left + formulario.Width / 2f - centroAnclaX,
                    formulario.Top + formulario.Height / 2f - centroAnclaY);
                registro.PosicionDisenoCapturada = true;
            }
            // Una ventana maximizada ya recibe del sistema el nuevo tamaño de
            // pantalla. Mantener su estado evita restaurarla y mostrarla encima
            // de Configuración durante el cambio de DPI/resolución.
            if (estadoAnterior == FormWindowState.Normal)
            {
                formulario.MaximumSize = Size.Empty;
                formulario.StartPosition = FormStartPosition.Manual;
            }
            AplicarEscala(formulario, registro, objetivo);
            registro.Monitor = pantalla.DeviceName;
            Rectangle ancla = registro.CentrarEnPropietario && formulario.Owner != null &&
                !formulario.Owner.IsDisposed ? formulario.Owner.Bounds : area;
            float centroX = ancla.Left + ancla.Width / 2f + registro.DesplazamientoCentroDiseno.X * escalaUniforme;
            float centroY = ancla.Top + ancla.Height / 2f + registro.DesplazamientoCentroDiseno.Y * escalaUniforme;
            if (estadoAnterior == FormWindowState.Normal)
            {
                int izquierda = (int)Math.Round(centroX - formulario.Width / 2f);
                int arriba = (int)Math.Round(centroY - formulario.Height / 2f);
                if (formulario.IsHandleCreated)
                {
                    // Nunca activar ni subir el formulario al ajustar sus bounds.
                    SetWindowPos(formulario.Handle, IntPtr.Zero, izquierda, arriba,
                        formulario.Width, formulario.Height,
                        SwpNoZOrder | SwpNoActivate | SwpNoOwnerZOrder);
                }
                else
                {
                    formulario.Location = new Point(izquierda, arriba);
                }
                MantenerEnAreaVisible(formulario, area);
            }
            Trace.TraceInformation(String.Format(
                "Escalado de {0}: DPI {1}, lienzo {2}x{3}, factor uniforme {4:0.###}, cliente {5}x{6}.",
                formulario.Name, dpi, AnchoLienzoDiseno, AltoLienzoDiseno, escalaUniforme,
                formulario.ClientSize.Width, formulario.ClientSize.Height));
        }

        private static void AplicarEscala(Form formulario, Registro registro, Size clienteObjetivo)
        {
            if (formulario == null || formulario.IsDisposed || !registro.Capturado || registro.Ajustando) return;
            float escalaUniforme = Math.Max(0.01f, Math.Min(
                (float)clienteObjetivo.Width / Math.Max(1, registro.ClienteDiseno.Width),
                (float)clienteObjetivo.Height / Math.Max(1, registro.ClienteDiseno.Height)));
            float escalaX = escalaUniforme;
            float escalaY = escalaUniforme;
            if (Math.Abs(escalaX - registro.EscalaX) < 0.0001f &&
                Math.Abs(escalaY - registro.EscalaY) < 0.0001f &&
                formulario.ClientSize == clienteObjetivo &&
                registro.DpiActual == formulario.DeviceDpi) return;
            registro.Ajustando = true;
            bool suspendido = false;
            bool redibujadoSuspendido = false;
            var controlesRedibujados = new List<Control>();
            try
            {
                redibujadoSuspendido = true;
                ControlesPresentacion.SuspenderRedibujadoArbol(formulario, controlesRedibujados);
                suspendido = true;
                SuspenderArbol(formulario);
                ControlBase baseFormulario;
                if (registro.Controles.TryGetValue(formulario, out baseFormulario))
                {
                    formulario.MinimumSize = EscalarTamano(baseFormulario.MinimumSize,
                        escalaUniforme, escalaUniforme);
                    formulario.MaximumSize = EscalarTamano(baseFormulario.MaximumSize,
                        escalaUniforme, escalaUniforme);
                }
                if (formulario.WindowState != FormWindowState.Maximized)
                    formulario.ClientSize = clienteObjetivo;

                foreach (KeyValuePair<Control, ControlBase> par in registro.Controles)
                {
                    Control control = par.Key;
                    if (control.IsDisposed || control == formulario) continue;
                    ControlBase baseControl = par.Value;
                    Rectangle bounds = baseControl.Bounds;
                    if (!(control.Parent is TableLayoutPanel))
                        control.SetBounds((int)Math.Round(bounds.X * escalaX),
                            (int)Math.Round(bounds.Y * escalaY),
                            Math.Max(1, (int)Math.Round(bounds.Width * escalaX)),
                            Math.Max(1, (int)Math.Round(bounds.Height * escalaY)), BoundsSpecified.All);
                    control.Margin = EscalarPadding(baseControl.Margin, escalaX, escalaY);
                    control.Padding = EscalarPadding(baseControl.Padding, escalaX, escalaY);
                    control.MinimumSize = EscalarTamano(baseControl.MinimumSize, escalaX, escalaY);
                    control.MaximumSize = EscalarTamano(baseControl.MaximumSize, escalaX, escalaY);
                    AplicarPropiedadesVisuales(control, baseControl, escalaUniforme, registro);
                    AplicarFuente(control, baseControl, escalaUniforme, registro);
                }

                foreach (EstiloTabla estilo in registro.Tablas)
                {
                    if (estilo.Tabla.IsDisposed) continue;
                    for (int i = 0; i < Math.Min(estilo.AnchosColumnas.Length, estilo.Tabla.ColumnStyles.Count); i++)
                        if (estilo.Tabla.ColumnStyles[i].SizeType == SizeType.Absolute)
                            estilo.Tabla.ColumnStyles[i].Width = estilo.AnchosColumnas[i] * escalaX;
                    for (int i = 0; i < Math.Min(estilo.AltosFilas.Length, estilo.Tabla.RowStyles.Count); i++)
                        if (estilo.Tabla.RowStyles[i].SizeType == SizeType.Absolute)
                            estilo.Tabla.RowStyles[i].Height = estilo.AltosFilas[i] * escalaY;
                }
                registro.EscalaX = escalaX;
                registro.EscalaY = escalaY;
                registro.DpiAnterior = registro.DpiActual;
                registro.DpiActual = formulario.DeviceDpi;
            }
            finally
            {
                if (suspendido) ReanudarArbol(formulario, true);
                registro.Ajustando = false;
                if (redibujadoSuspendido && !formulario.IsDisposed)
                {
                    ControlesPresentacion.ReanudarRedibujadoArbol(controlesRedibujados);
                    // Update() solo vacía el área invalidada de esta ventana;
                    // RedrawWindow solicita el repintado inmediato del formulario
                    // y de todos sus HWND hijos después de restaurar WM_SETREDRAW.
                    if (formulario.IsHandleCreated)
                        RedrawWindow(formulario.Handle, IntPtr.Zero, IntPtr.Zero,
                            RedrawInvalidate | RedrawErase | RedrawAllChildren |
                            RedrawUpdateNow | RedrawFrame);
                }
            }
        }

        private static void AplicarFuente(Control control, ControlBase baseControl,
            float escala, Registro registro)
        {
            if (baseControl.Font == null || control.IsDisposed) return;
            Font anterior;
            if (registro.FuentesAplicadas.TryGetValue(control, out anterior))
                registro.FuentesAplicadas.Remove(control);
            // Se conserva la unidad original de la fuente y se aplica el mismo
            // factor uniforme usado para la geometría.
            Font fuente = new Font(baseControl.Font.FontFamily,
                Math.Max(0.1f, baseControl.Font.Size * escala), baseControl.Font.Style,
                baseControl.Font.Unit, baseControl.Font.GdiCharSet, baseControl.Font.GdiVerticalFont);
            control.Font = fuente;
            registro.FuentesAplicadas[control] = fuente;
            registro.FuentesCreadas.Add(fuente);
            if (anterior != null)
            {
                registro.FuentesCreadas.Remove(anterior);
                anterior.Dispose();
            }
        }

        private static readonly string[] PropiedadesVisualesEscalables =
            { "Radius", "BorderWidth", "SplitterWidth", "SplitterDistance", "Panel1MinSize", "Panel2MinSize", "RowHeadersWidth", "ColumnHeadersHeight" };

        private static void CapturarPropiedadesVisuales(Control control, ControlBase baseControl)
        {
            foreach (string nombre in PropiedadesVisualesEscalables)
            {
                try
                {
                    var propiedad = control.GetType().GetProperty(nombre);
                    if (propiedad == null || !propiedad.CanRead || !propiedad.CanWrite) continue;
                    Type tipo = Nullable.GetUnderlyingType(propiedad.PropertyType) ?? propiedad.PropertyType;
                    if (tipo != typeof(byte) && tipo != typeof(short) && tipo != typeof(int) &&
                        tipo != typeof(long) && tipo != typeof(float) && tipo != typeof(double) &&
                        tipo != typeof(decimal)) continue;
                    baseControl.PropiedadesVisuales[nombre] = Convert.ToSingle(propiedad.GetValue(control, null));
                }
                catch { }
            }
        }

        private static void AplicarPropiedadesVisuales(Control control, ControlBase baseControl,
            float escala, Registro registro)
        {
            LimpiarFuentesEspeciales(control, registro);
            if (control is ListBox && baseControl.AltoElementoLista.HasValue)
                ((ListBox)control).ItemHeight = Math.Max(1,
                    (int)Math.Round(baseControl.AltoElementoLista.Value * escala));
            if (control is TreeView && baseControl.AltoElementoArbol.HasValue)
                ((TreeView)control).ItemHeight = Math.Max(1,
                    (int)Math.Round(baseControl.AltoElementoArbol.Value * escala));
            ListView listaVista = control as ListView;
            if (listaVista != null)
                foreach (KeyValuePair<ColumnHeader, int> columna in baseControl.AnchosColumnasListView)
                    if (columna.Key.ListView == listaVista)
                        columna.Key.Width = Math.Max(1, (int)Math.Round(columna.Value * escala));
            IconosGoogle.AplicarEscala(control, escala);
            ToolStrip barraImagenes = control as ToolStrip;
            if (barraImagenes != null && baseControl.TamanoImagenesToolStrip.HasValue)
                barraImagenes.ImageScalingSize = EscalarTamano(baseControl.TamanoImagenesToolStrip.Value, escala, escala);
            TabControl pestañas = control as TabControl;
            if (pestañas != null && baseControl.TamanoElementoTab.HasValue)
                pestañas.ItemSize = EscalarTamano(baseControl.TamanoElementoTab.Value, escala, escala);
            foreach (KeyValuePair<ImageList, Size> imagenes in baseControl.TamanosImageList)
                if (imagenes.Key != null)
                    imagenes.Key.ImageSize = EscalarTamano(imagenes.Value, escala, escala);
            DataGridView rejilla = control as DataGridView;
            if (rejilla != null)
            {
                AplicarFuenteEstiloGrid(rejilla, rejilla.DefaultCellStyle, baseControl.FuenteGrid, escala, registro);
                AplicarFuenteEstiloGrid(rejilla, rejilla.ColumnHeadersDefaultCellStyle, baseControl.FuenteEncabezadoGrid, escala, registro);
                AplicarFuenteEstiloGrid(rejilla, rejilla.RowHeadersDefaultCellStyle, baseControl.FuenteEncabezadoFilaGrid, escala, registro);
                if (baseControl.RellenoGrid.HasValue)
                    rejilla.DefaultCellStyle.Padding = EscalarPadding(baseControl.RellenoGrid.Value, escala, escala);
                if (baseControl.RellenoEncabezadoGrid.HasValue)
                    rejilla.ColumnHeadersDefaultCellStyle.Padding = EscalarPadding(baseControl.RellenoEncabezadoGrid.Value, escala, escala);
                if (baseControl.RellenoEncabezadoFilaGrid.HasValue)
                    rejilla.RowHeadersDefaultCellStyle.Padding = EscalarPadding(baseControl.RellenoEncabezadoFilaGrid.Value, escala, escala);
                if (baseControl.AltoFilaGrid.HasValue)
                    rejilla.RowTemplate.Height = Math.Max(2,
                        (int)Math.Round(baseControl.AltoFilaGrid.Value * escala));
                foreach (KeyValuePair<DataGridViewColumn, int> columna in baseControl.AnchosColumnasGrid)
                {
                    if (columna.Key.DataGridView != rejilla ||
                        columna.Key.AutoSizeMode != DataGridViewAutoSizeColumnMode.NotSet &&
                        columna.Key.AutoSizeMode != DataGridViewAutoSizeColumnMode.None ||
                        rejilla.AutoSizeColumnsMode != DataGridViewAutoSizeColumnsMode.None) continue;
                    try { columna.Key.Width = Math.Max(2, (int)Math.Round(columna.Value * escala)); }
                    catch (InvalidOperationException) { }
                }
                foreach (KeyValuePair<DataGridViewRow, int> fila in baseControl.AltosFilasGrid)
                {
                    if (fila.Key.DataGridView != rejilla || fila.Key.IsNewRow ||
                        fila.Key.Resizable == DataGridViewTriState.False) continue;
                    try { fila.Key.Height = Math.Max(2, (int)Math.Round(fila.Value * escala)); }
                    catch (InvalidOperationException) { }
                }
            }
            foreach (KeyValuePair<ToolStripItem, Font> itemFuente in baseControl.FuentesItemsBarra)
                if (itemFuente.Key != null && itemFuente.Key.Owner != null && itemFuente.Value != null)
                    itemFuente.Key.Font = CrearFuenteAplicada(control, itemFuente.Value,
                        escala, registro);
            foreach (KeyValuePair<ToolStripItem, Padding> margenItem in baseControl.MargenesItemsBarra)
                if (margenItem.Key != null && margenItem.Key.Owner != null)
                    margenItem.Key.Margin = EscalarPadding(margenItem.Value, escala, escala);
            foreach (KeyValuePair<ToolStripItem, Padding> rellenoItem in baseControl.RellenosItemsBarra)
                if (rellenoItem.Key != null && rellenoItem.Key.Owner != null)
                    rellenoItem.Key.Padding = EscalarPadding(rellenoItem.Value, escala, escala);
            foreach (KeyValuePair<string, float> par in baseControl.PropiedadesVisuales)
            {
                try
                {
                    var propiedad = control.GetType().GetProperty(par.Key);
                    if (propiedad == null || !propiedad.CanWrite) continue;
                    Type tipo = Nullable.GetUnderlyingType(propiedad.PropertyType) ?? propiedad.PropertyType;
                    object valor = Convert.ChangeType(par.Value * escala, tipo,
                        System.Globalization.CultureInfo.InvariantCulture);
                    propiedad.SetValue(control, valor, null);
                }
                catch { }
            }
        }

        private static void AplicarFuenteEstiloGrid(Control control, DataGridViewCellStyle estilo,
            Font baseFuente, float escala, Registro registro)
        {
            if (estilo == null || baseFuente == null) return;
            estilo.Font = CrearFuenteAplicada(control, baseFuente, escala, registro);
        }

        private static Font CrearFuenteAplicada(Control control, Font baseFuente, float escala, Registro registro)
        {
            Font fuente = new Font(baseFuente.FontFamily,
                Math.Max(0.1f, baseFuente.Size * escala), baseFuente.Style,
                baseFuente.Unit, baseFuente.GdiCharSet, baseFuente.GdiVerticalFont);
            List<Font> aplicadas;
            if (!registro.FuentesEspecialesAplicadas.TryGetValue(control, out aplicadas))
                registro.FuentesEspecialesAplicadas[control] = aplicadas = new List<Font>();
            aplicadas.Add(fuente);
            registro.FuentesCreadas.Add(fuente);
            return fuente;
        }

        private static void LimpiarFuentesEspeciales(Control control, Registro registro)
        {
            List<Font> anteriores;
            if (!registro.FuentesEspecialesAplicadas.TryGetValue(control, out anteriores)) return;
            registro.FuentesEspecialesAplicadas.Remove(control);
            foreach (Font fuente in anteriores)
            {
                registro.FuentesCreadas.Remove(fuente);
                fuente.Dispose();
            }
        }

        private static Padding EscalarPadding(Padding valor, float escalaX, float escalaY)
        {
            return new Padding((int)Math.Round(valor.Left * escalaX), (int)Math.Round(valor.Top * escalaY),
                (int)Math.Round(valor.Right * escalaX), (int)Math.Round(valor.Bottom * escalaY));
        }

        private static Size EscalarTamano(Size valor, float escalaX, float escalaY)
        {
            if (valor == Size.Empty) return Size.Empty;
            return new Size(Math.Max(0, (int)Math.Round(valor.Width * escalaX)),
                Math.Max(0, (int)Math.Round(valor.Height * escalaY)));
        }

        private static void SuspenderArbol(Control control)
        {
            control.SuspendLayout();
            foreach (Control hijo in control.Controls)
                SuspenderArbol(hijo);
        }

        private static void ReanudarArbol(Control control, bool realizarLayout = false)
        {
            foreach (Control hijo in control.Controls)
                ReanudarArbol(hijo, realizarLayout);
            control.ResumeLayout(realizarLayout);
        }
        private static void MantenerEnAreaVisible(Form formulario, Rectangle area)
        {
            int x = Math.Max(area.Left, Math.Min(formulario.Left, area.Right - formulario.Width));
            int y = Math.Max(area.Top, Math.Min(formulario.Top, area.Bottom - formulario.Height));
            if (x == formulario.Left && y == formulario.Top) return;
            if (formulario.IsHandleCreated)
                SetWindowPos(formulario.Handle, IntPtr.Zero, x, y, formulario.Width,
                    formulario.Height, SwpNoZOrder | SwpNoActivate | SwpNoOwnerZOrder);
            else
                formulario.Location = new Point(x, y);
        }

        private static void RegistrarControlesDinamicos(Control contenedor, Registro registro)
        {
            if (!registro.ContenedoresRegistrados.Add(contenedor)) return;
            contenedor.ControlAdded += (s, e) =>
            {
                Form propietario = e.Control.FindForm();
                if (propietario != null) RegistrarElementoDinamico(propietario, e.Control);
            };

            foreach (Control hijo in contenedor.Controls)
                RegistrarControlesDinamicos(hijo, registro);
        }

        /// <summary>Captura y escala inmediatamente un control dinámico y sus hijos.</summary>
        internal static void RegistrarElementoDinamico(Form formulario, Control elemento)
        {
            Registro registro;
            if (formulario == null || elemento == null || formulario.IsDisposed ||
                !Formularios.TryGetValue(formulario, out registro) || !registro.Capturado ||
                registro.Controles.ContainsKey(elemento)) return;
            try
            {
                CapturarArbol(elemento, registro, 1f, 1f);
                DesactivarAutoescaladoWinForms(elemento);
                RegistrarControlesDinamicos(elemento, registro);
                // Los controles que nacen antes del primer escalado conservan
                // sus valores de Designer. AplicarEscala escalará todo el árbol.
                if (float.IsNaN(registro.EscalaX) || float.IsNaN(registro.EscalaY)) return;
                AplicarEscalaSubarbol(elemento, registro);
            }
            catch (Exception ex)
            {
                DesactivarRegistro(formulario, registro, ex, "registro de control dinámico");
            }
        }

        private static void AplicarEscalaSubarbol(Control raiz, Registro registro)
        {
            foreach (KeyValuePair<Control, ControlBase> par in registro.Controles)
            {
                Control control = par.Key;
                if (control.IsDisposed || (control != raiz && !raiz.Contains(control))) continue;
                ControlBase baseControl = par.Value;
                Rectangle bounds = baseControl.Bounds;
                if (!(control.Parent is TableLayoutPanel))
                    control.SetBounds((int)Math.Round(bounds.X * registro.EscalaX),
                        (int)Math.Round(bounds.Y * registro.EscalaY),
                        Math.Max(1, (int)Math.Round(bounds.Width * registro.EscalaX)),
                        Math.Max(1, (int)Math.Round(bounds.Height * registro.EscalaY)), BoundsSpecified.All);
                control.Margin = EscalarPadding(baseControl.Margin, registro.EscalaX, registro.EscalaY);
                control.Padding = EscalarPadding(baseControl.Padding, registro.EscalaX, registro.EscalaY);
                control.MinimumSize = EscalarTamano(baseControl.MinimumSize, registro.EscalaX, registro.EscalaY);
                control.MaximumSize = EscalarTamano(baseControl.MaximumSize, registro.EscalaX, registro.EscalaY);
                AplicarPropiedadesVisuales(control, baseControl, registro.EscalaX, registro);
                AplicarFuente(control, baseControl, registro.EscalaX, registro);
            }

            foreach (EstiloTabla estilo in registro.Tablas)
            {
                if (estilo.Tabla.IsDisposed || (estilo.Tabla != raiz && !raiz.Contains(estilo.Tabla))) continue;
                for (int i = 0; i < Math.Min(estilo.AnchosColumnas.Length, estilo.Tabla.ColumnStyles.Count); i++)
                    if (estilo.Tabla.ColumnStyles[i].SizeType == SizeType.Absolute)
                        estilo.Tabla.ColumnStyles[i].Width = estilo.AnchosColumnas[i] * registro.EscalaX;
                for (int i = 0; i < Math.Min(estilo.AltosFilas.Length, estilo.Tabla.RowStyles.Count); i++)
                    if (estilo.Tabla.RowStyles[i].SizeType == SizeType.Absolute)
                        estilo.Tabla.RowStyles[i].Height = estilo.AltosFilas[i] * registro.EscalaY;
            }
        }

    }
}
