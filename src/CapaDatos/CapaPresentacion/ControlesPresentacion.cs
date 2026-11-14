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
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!EnModoDiseno && !esquinasNativas) AplicarEsquinasRedondeadas();
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

            RectangleF limites = new RectangleF(
                AnchoBorde,
                AnchoBorde,
                ClientSize.Width - AnchoBorde * 2f,
                ClientSize.Height - AnchoBorde * 2f);
            float radio = RadioEsquinaEscalado();
            using (GraphicsPath trazado = CrearTrazadoRedondeado(
                limites, Math.Max(1f, radio - AnchoBorde)))
            using (Pen borde = new Pen(Color.FromArgb(96, 96, 96), AnchoBorde))
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
            return 8f * Math.Max(1, DeviceDpi) / 96f;
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
        public BufferedFlowLayoutPanel()
        {
            DoubleBuffered = true;
        }

    }

    /// <summary>Presenta solo una página de las tarjetas de un FlowLayoutPanel.</summary>
    internal sealed class PaginadorFlowLayout
    {
        private const int AltoBarra = 56;
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
                Height = AltoBarra,
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
                Font = new System.Drawing.Font("Segoe UI Symbol", 18F),
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
            int ancho = Math.Max(0, contenedor.ClientSize.Width - margenOriginal.Horizontal);
            int alto = Math.Max(0, contenedor.ClientSize.Height - margenOriginal.Vertical);
            int espacioBarra = barra.Visible ? AltoBarra : 0;
            lista.SetBounds(margenOriginal.Left, margenOriginal.Top, ancho,
                Math.Max(0, alto - espacioBarra));
            barra.SetBounds(margenOriginal.Left,
                margenOriginal.Top + Math.Max(0, alto - AltoBarra), ancho, AltoBarra);
            anterior.SetBounds(4, 7, 48, 42);
            int anchoBarra = barra.ClientSize.Width;
            if (encabezado != null && !encabezado.IsDisposed)
            {
                int anchoEncabezado = Math.Min(220, Math.Max(0, anchoBarra - 200));
                encabezado.SetBounds(4, 15, anchoEncabezado, 26);
                anterior.SetBounds(Math.Max(anchoEncabezado + 8, anchoBarra - 106), 7, 48, 42);
                siguiente.SetBounds(Math.Max(anterior.Right + 4, anchoBarra - 54), 7, 48, 42);
                pagina.SetBounds(anterior.Left - 68, 0, 64, AltoBarra);
            }
            else
            {
                siguiente.SetBounds(Math.Max(4, anchoBarra - 54), 7, 48, 42);
                pagina.SetBounds(52, 0, Math.Max(0, anchoBarra - 104), AltoBarra);
            }
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
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float lado = Math.Max(18, Math.Min(28, Height - 12));
            var circulo = new RectangleF(8, (Height - lado) / 2f, lado, lado);
            Color tinta = PaletaAplicacion.ColorTextoContraste(this);
            using (var lapizFondo = new Pen(Color.FromArgb(55, tinta), 3f))
                g.DrawEllipse(lapizFondo, circulo);
            using (var lapiz = new Pen(PaletaAplicacion.AzulPrimario, 3f))
            {
                lapiz.StartCap = LineCap.Round;
                lapiz.EndCap = LineCap.Round;
                g.DrawArc(lapiz, circulo, angulo, 105f);
            }

            var areaTexto = new Rectangle((int)(circulo.Right + 12), 0,
                Math.Max(0, Width - (int)circulo.Right - 16), Height);
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
            if (lista != null && lista.AutoScroll && !FormularioSuave.EnModoDiseno)
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

    /// <summary>Reduce formularios grandes para que quepan en el área visible de cualquier pantalla.</summary>
    internal static class AjustePantalla
    {
        private sealed class Registro
        {
            public bool Preparado;
        }

        private static readonly ConditionalWeakTable<Form, Registro> Formularios =
            new ConditionalWeakTable<Form, Registro>();

        public static void Preparar(Form formulario)
        {
            if (formulario == null || formulario.IsDisposed) return;
            ControlesPresentacion.Optimizar(formulario);
            Registro registro = Formularios.GetValue(formulario, _ => new Registro());
            if (registro.Preparado) return;

            registro.Preparado = true;
            formulario.Load += (s, e) => AjustarAlAreaVisible(formulario);
        }

        private static void AjustarAlAreaVisible(Form formulario)
        {
            if (formulario == null || formulario.IsDisposed) return;

            Screen pantalla = formulario.Owner != null && !formulario.Owner.IsDisposed
                ? Screen.FromControl(formulario.Owner)
                : Screen.FromControl(formulario);
            Rectangle area = pantalla.WorkingArea;
            Size clienteOriginal = formulario.ClientSize;
            Size ventanaOriginal = formulario.Size;
            int marcoAncho = Math.Max(0, ventanaOriginal.Width - clienteOriginal.Width);
            int marcoAlto = Math.Max(0, ventanaOriginal.Height - clienteOriginal.Height);
            int clienteMaxAncho = Math.Max(1, area.Width - marcoAncho);
            int clienteMaxAlto = Math.Max(1, area.Height - marcoAlto);

            float escala = Math.Min(1f, Math.Min(
                (float)clienteMaxAncho / Math.Max(1, clienteOriginal.Width),
                (float)clienteMaxAlto / Math.Max(1, clienteOriginal.Height)));

            formulario.SuspendLayout();
            try
            {
                formulario.WindowState = FormWindowState.Normal;
                formulario.MaximumSize = Size.Empty;
                if (escala < 0.999f)
                {
                    Size minimoOriginal = formulario.MinimumSize;
                    formulario.MinimumSize = Size.Empty;
                    formulario.Scale(new SizeF(escala, escala));
                    formulario.ClientSize = new Size(
                        Math.Max(1, (int)Math.Round(clienteOriginal.Width * escala)),
                        Math.Max(1, (int)Math.Round(clienteOriginal.Height * escala)));
                    formulario.MinimumSize = new Size(
                        Math.Max(0, (int)Math.Round(minimoOriginal.Width * escala)),
                        Math.Max(0, (int)Math.Round(minimoOriginal.Height * escala)));
                    RegistrarControlesDinamicos(formulario, escala);
                }
                else if (formulario.MinimumSize.Width > area.Width ||
                    formulario.MinimumSize.Height > area.Height)
                {
                    formulario.MinimumSize = Size.Empty;
                }

                formulario.MaximumSize = area.Size;
                formulario.StartPosition = FormStartPosition.Manual;
                CentrarEnAreaVisible(formulario, area);
            }
            finally { formulario.ResumeLayout(true); }
        }

        private static void CentrarEnAreaVisible(Form formulario, Rectangle area)
        {
            Rectangle referencia = area;
            Form propietario = formulario.Owner;
            if (propietario != null && !propietario.IsDisposed && propietario.Visible)
                referencia = Rectangle.Intersect(area, propietario.Bounds);
            if (referencia.Width <= 0 || referencia.Height <= 0) referencia = area;

            int x = referencia.Left + (referencia.Width - formulario.Width) / 2;
            int y = referencia.Top + (referencia.Height - formulario.Height) / 2;
            x = Math.Max(area.Left, Math.Min(x, area.Right - formulario.Width));
            y = Math.Max(area.Top, Math.Min(y, area.Bottom - formulario.Height));
            formulario.Location = new Point(x, y);
        }

        private static void RegistrarControlesDinamicos(Control contenedor, float escala)
        {
            contenedor.ControlAdded += (s, e) =>
            {
                Control agregado = e.Control;
                if (agregado is UserControl || agregado is IndicadorCarga)
                    agregado.Scale(new SizeF(escala, escala));
                RegistrarControlesDinamicos(agregado, escala);
            };

            foreach (Control hijo in contenedor.Controls)
                RegistrarControlesDinamicos(hijo, escala);
        }
    }
}
