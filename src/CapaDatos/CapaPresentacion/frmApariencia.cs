using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CapaEntidades;

namespace CapaPresentacion
{
    public partial class frmApariencia : FormularioSuave
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point point);

        private enum RolColor
        {
            FondoVentana, Cabecera, TextoCabecera, BotonCerrar, BotonBorrar, BotonAgregarTarea,
            TextoTarjetaEjemplo, TextoDescripcionTarjeta, IconosTarjeta, IconosPrincipal,
            Primario, FondoSubTarjeta, TarjetaOscura, SuperficieClara, PanelDia, Exito, BotonSumar, BotonRestar,
            DiaNormal, DiaSeleccionado, DiaActual, DiaFuera, SuperficieTarea,
            FondoTextoGrande, FondoTextoPequeno,
            TextoOscuro, TextoClaro
        }

        private readonly int idUsuario;
        private readonly PreferenciasApariencia preferenciasOriginales;
        private PreferenciasApariencia preferencias;
        private bool cierreAutorizado;
        private static bool selectorColorAbierto;
        private static Form selectorColorPopup;
        private static System.Windows.Forms.Timer verificadorSelectorColor;
        private static readonly Color[] PresetsSelectorColor =
        {
            Color.Black, Color.White, Color.Gray, Color.Silver,
            Color.Red, Color.OrangeRed, Color.Orange, Color.Gold,
            Color.Yellow, Color.Green, Color.LimeGreen, Color.Teal,
            Color.Cyan, Color.DeepSkyBlue, Color.Blue, Color.MediumBlue,
            Color.Purple, Color.Magenta, Color.HotPink, Color.Brown,
            Color.FromArgb(71, 104, 180), Color.FromArgb(75, 119, 209),
            Color.FromArgb(89, 133, 225), Color.FromArgb(37, 63, 118)
        };

        public frmApariencia() : this(0) { }

        public frmApariencia(E_Usuario usuario) : this(usuario == null ? 0 : usuario.IdUsuario) { }

        private frmApariencia(int idUsuario)
        {
            InitializeComponent();
            if (EnModoDiseno) return;
            // Guarda los colores del diseñador antes de pintar la vista previa:
            // así "Restablecer" puede devolver exactamente esos valores.
            TemaAplicacion.Preparar(this);
            IconosGoogle.Aplicar(lblIconoTop, "settings");
            IconosGoogle.Aplicar(pbTargetaEjemplo, "font_download");
            IconosGoogle.Aplicar(pbSubTargetaEjemplo, "font_download");
            IconosGoogle.Aplicar(btnCerrar, "close");
            IconosGoogle.Aplicar(btnGuardar, "save");
            ArrastreFormulario.Habilitar(this, pnlTop);
            ConfigurarListasTransparentes();
            this.idUsuario = idUsuario;
            preferencias = CargarApariencia(idUsuario);
            preferenciasOriginales = preferencias.Copiar();

            ConfigurarEditorFuente();
            ConfigurarCuentagotas();

            btnCambiarFuente.Click += (s, e) => AlternarEditorFuente();
            lblFuenteActual.Click += (s, e) => ElegirFuente();
            lblTamanoFuente.Click += (s, e) => ElegirTamanoFuente();
            lblFondoOscuro.Click += (s, e) => ElegirColorTexto(true);
            lblFondoClaro.Click += (s, e) => ElegirColorTexto(false);
            btnGuardar.Click += BtnGuardar_Click;
            btnRestablecerApariencia.Click += BtnRestablecer_Click;
            btnCancelar.Click += BtnCancelar_Click;
            FormClosing += FrmApariencia_FormClosing;
            AplicarVistaPrevia();
        }

        private static PreferenciasApariencia CargarApariencia(int idUsuario)
        {
            try { return AparienciaUsuarioApp.Cargar(idUsuario); }
            catch { return new PreferenciasApariencia(); }
        }





        private void ConfigurarEditorFuente()
        {
            // Se conserva la visibilidad establecida por el diseñador.
        }

        private void AlternarEditorFuente()
        {
            // El cambio visual del editor queda desactivado temporalmente.
        }

        private void ActualizarValoresFuente()
        {
            // Se conservan los textos configurados por el diseñador.
        }

        private void ConfigurarCuentagotas()
        {
            RegistrarCursorYSeleccionColor(this);
            pnlEditor.Cursor = Cursors.Default;
            EstablecerCursorEditor(pnlEditor);
        }

        private void RegistrarCursorYSeleccionColor(Control control)
        {
            if (control == pnlEditor) return;
            control.Cursor = Cursors.Cross;
            control.Click += ControlVistaPrevia_Click;
            foreach (Control hijo in control.Controls)
                RegistrarCursorYSeleccionColor(hijo);
        }

        private static void EstablecerCursorEditor(Control control)
        {
            control.Cursor = Cursors.Default;
            foreach (Control hijo in control.Controls) EstablecerCursorEditor(hijo);
        }

        private void ControlVistaPrevia_Click(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control == null) return;
            Point puntoPantalla;
            if (!GetCursorPos(out puntoPantalla))
                puntoPantalla = control.PointToScreen(new Point(control.Width / 2, control.Height / 2));
            Point puntoLocal = control.PointToClient(puntoPantalla);
            RolColor? rol = ObtenerRol(control);

            // En los botones con icono, pulsar el glifo cambia su color; pulsar
            // el margen cambia el fondo del botón.
            if (EsZonaIconoDeAccion(control, puntoLocal)) rol = RolColor.IconosPrincipal;
            if (control == nudNotiPrevia) return;
            if (!rol.HasValue) return;
            MostrarSelectorColor(control, puntoLocal, ObtenerColor(rol.Value),
                color => CambiarColor(rol.Value, color));
        }

        private bool EsZonaIconoDeAccion(Control control, Point puntoLocal)
        {
            if (!(control is AntdUI.Button)) return false;
            var boton = (AntdUI.Button)control;
            if (boton.Icon == null) return false;
            int lado = Math.Min(control.Width, control.Height) * 3 / 5;
            var zona = new Rectangle((control.Width - lado) / 2, (control.Height - lado) / 2, lado, lado);
            return zona.Contains(puntoLocal);
        }

        private RolColor? ObtenerRol(Control control)
        {
            if (control == null) return null;
            if (control == this) return RolColor.FondoVentana;
            if (control == nudNotiPrevia) return null;
            if (control.Name == "lblIconoTop" || control.Name == "pbIcono" ||
                control.Name == "pbIcono2" || control is PictureBox ||
                control is AntdUI.Image3D)
                return RolColor.IconosTarjeta;

            // El texto es solo un punto cómodo para elegir su fondo más cercano.
            // No existe un rol de color editable para las etiquetas.
            if (control is Label || control is AntdUI.Label)
            {
                Control padreTexto = control.Parent;
                while (padreTexto != null && padreTexto != this)
                {
                    RolColor? fondoTexto = ObtenerRolFondo(padreTexto);
                    if (fondoTexto.HasValue) return fondoTexto;
                    padreTexto = padreTexto.Parent;
                }
                return null;
            }

            RolColor? rolNombrado = ObtenerRolPorNombre(control);
            if (rolNombrado.HasValue) return rolNombrado;

            RolColor? rolFondo = ObtenerRolFondo(control);
            if (rolFondo.HasValue) return rolFondo;
            Control contenedor = control.Parent;
            while (contenedor != null && contenedor != this)
            {
                rolFondo = ObtenerRolFondo(contenedor);
                if (rolFondo.HasValue) return rolFondo;
                contenedor = contenedor.Parent;
            }
            return null;
        }

        private RolColor? ObtenerRolFondo(Control control)
        {
            return ObtenerRolPorNombre(control);
        }

        private static RolColor? ObtenerRolPorNombre(Control control)
        {
            if (control == null) return null;
            string nombre = control.Name ?? String.Empty;
            if (nombre == "pnlTop") return RolColor.Cabecera;
            if (nombre == "btnCerrar") return RolColor.BotonCerrar;
            if (nombre == "btnCancelar") return RolColor.Primario;
            if (nombre == "btnBorrar" || nombre == "btnQuitar") return RolColor.BotonBorrar;
            if (nombre == "btnSumar" || nombre.StartsWith("btnSumar", StringComparison.Ordinal))
                return RolColor.BotonSumar;
            if (nombre == "btnRestar" || nombre.StartsWith("btnRestar", StringComparison.Ordinal))
                return RolColor.BotonRestar;
            if (nombre == "lblIconoTop" || nombre == "pbIcono" || nombre == "pbIcono2" ||
                control is PictureBox || control is AntdUI.Image3D)
                return RolColor.IconosTarjeta;
            if (nombre == "pnlTareaDiaActual" || nombre == "pnlTareaDiaMes" ||
                nombre == "pnlTareaDiaSeleccionado" || nombre == "pnlTareaFueraDelMes")
                return RolColor.PanelDia;
            if (nombre == "pnlTarjeta" || nombre == "pnlTargeta") return RolColor.TarjetaOscura;
            if (nombre.StartsWith("pnlSubTargeta", StringComparison.OrdinalIgnoreCase) ||
                nombre.StartsWith("pnlSubTarjeta", StringComparison.OrdinalIgnoreCase))
                return RolColor.FondoSubTarjeta;
            if (nombre == "pnlDiaMes") return RolColor.DiaNormal;
            if (nombre == "pnlDiaActual") return RolColor.DiaActual;
            if (nombre == "pnlDiaSeleccionado") return RolColor.DiaSeleccionado;
            if (nombre == "pnlDiaFueraMes") return RolColor.DiaFuera;
            if (nombre == "btnAgregarTarea") return RolColor.BotonAgregarTarea;
            if (nombre == "pnlTargetaTarea") return RolColor.SuperficieTarea;
            if (nombre == "pnlLayoutLista") return RolColor.SuperficieClara;
            if (nombre == "pnlTextoGrande") return RolColor.FondoTextoGrande;
            if (nombre == "pnlTextoPequeño" ||
                nombre.StartsWith("pnlPequeno", StringComparison.OrdinalIgnoreCase))
                return RolColor.FondoTextoPequeno;
            if (nombre == "btnEjemplo") return RolColor.Primario;
            return null;
        }

        private Color ObtenerColor(RolColor rol)
        {
            switch (rol)
            {
                case RolColor.FondoVentana: return Color.FromArgb(preferencias.FondoVentana);
                case RolColor.Cabecera: return Color.FromArgb(preferencias.Cabecera);
                case RolColor.TextoCabecera: return Color.FromArgb(preferencias.TextoCabecera);
                case RolColor.BotonCerrar: return Color.FromArgb(preferencias.BotonCerrar);
                case RolColor.BotonBorrar: return Color.FromArgb(preferencias.BotonBorrar);
                case RolColor.BotonAgregarTarea: return Color.FromArgb(preferencias.BotonAgregarTarea);
                case RolColor.TextoTarjetaEjemplo: return Color.FromArgb(preferencias.TextoTarjetaEjemplo);
                case RolColor.TextoDescripcionTarjeta: return Color.FromArgb(preferencias.TextoDescripcionTarjeta);
                case RolColor.IconosTarjeta: return Color.FromArgb(preferencias.IconosTarjeta);
                case RolColor.IconosPrincipal: return Color.FromArgb(preferencias.IconosPrincipal);
                case RolColor.Primario: return Color.FromArgb(preferencias.Primario);
                case RolColor.FondoSubTarjeta: return Color.FromArgb(preferencias.FondoSubTarjeta);
                case RolColor.TarjetaOscura: return Color.FromArgb(preferencias.TarjetaOscura);
                case RolColor.SuperficieClara: return Color.FromArgb(preferencias.SuperficieClara);
                case RolColor.SuperficieTarea: return Color.FromArgb(preferencias.SuperficieTarea);
                case RolColor.FondoTextoGrande: return Color.FromArgb(preferencias.FondoTextoGrande);
                case RolColor.FondoTextoPequeno: return Color.FromArgb(preferencias.FondoTextoPequeno);
                case RolColor.PanelDia: return Color.FromArgb(preferencias.PanelDia);
                case RolColor.Exito: return Color.FromArgb(preferencias.Exito);
                case RolColor.BotonSumar: return Color.FromArgb(preferencias.BotonSumar);
                case RolColor.BotonRestar: return Color.FromArgb(preferencias.BotonRestar);
                case RolColor.DiaNormal: return Color.FromArgb(preferencias.DiaNormal);
                case RolColor.DiaSeleccionado: return Color.FromArgb(preferencias.DiaSeleccionado);
                case RolColor.DiaActual: return Color.FromArgb(preferencias.DiaActual);
                case RolColor.DiaFuera: return Color.FromArgb(preferencias.DiaFueraDeMes);
                case RolColor.TextoOscuro: return preferencias.ColorTextoOscuro;
                default: return preferencias.ColorTextoClaro;
            }
        }

        private void CambiarColor(RolColor rol, Color color)
        {
            switch (rol)
            {
                case RolColor.FondoVentana: preferencias.FondoVentana = color.ToArgb(); break;
                case RolColor.Cabecera: preferencias.Cabecera = color.ToArgb(); break;
                case RolColor.TextoCabecera: preferencias.TextoCabecera = color.ToArgb(); break;
                case RolColor.BotonCerrar: preferencias.BotonCerrar = color.ToArgb(); break;
                case RolColor.BotonBorrar: preferencias.BotonBorrar = color.ToArgb(); break;
                case RolColor.BotonAgregarTarea: preferencias.BotonAgregarTarea = color.ToArgb(); break;
                case RolColor.TextoTarjetaEjemplo: preferencias.TextoTarjetaEjemplo = color.ToArgb(); break;
                case RolColor.TextoDescripcionTarjeta: preferencias.TextoDescripcionTarjeta = color.ToArgb(); break;
                case RolColor.IconosTarjeta: preferencias.IconosTarjeta = color.ToArgb(); break;
                case RolColor.IconosPrincipal: preferencias.IconosPrincipal = color.ToArgb(); break;
                case RolColor.Primario: preferencias.Primario = color.ToArgb(); break;
                case RolColor.FondoSubTarjeta: preferencias.FondoSubTarjeta = color.ToArgb(); break;
                case RolColor.TarjetaOscura: preferencias.TarjetaOscura = color.ToArgb(); break;
                case RolColor.SuperficieClara: preferencias.SuperficieClara = color.ToArgb(); break;
                case RolColor.SuperficieTarea: preferencias.SuperficieTarea = color.ToArgb(); break;
                case RolColor.FondoTextoGrande: preferencias.FondoTextoGrande = color.ToArgb(); break;
                case RolColor.FondoTextoPequeno: preferencias.FondoTextoPequeno = color.ToArgb(); break;
                case RolColor.PanelDia: preferencias.PanelDia = color.ToArgb(); break;
                case RolColor.Exito: preferencias.Exito = color.ToArgb(); break;
                case RolColor.BotonSumar: preferencias.BotonSumar = color.ToArgb(); break;
                case RolColor.BotonRestar: preferencias.BotonRestar = color.ToArgb(); break;
                case RolColor.DiaNormal: preferencias.DiaNormal = color.ToArgb(); break;
                case RolColor.DiaSeleccionado: preferencias.DiaSeleccionado = color.ToArgb(); break;
                case RolColor.DiaActual: preferencias.DiaActual = color.ToArgb(); break;
                case RolColor.DiaFuera: preferencias.DiaFueraDeMes = color.ToArgb(); break;
                case RolColor.TextoOscuro: preferencias.ColorTextoFondoOscuro = color.ToArgb(); break;
                case RolColor.TextoClaro: preferencias.ColorTextoFondoClaro = color.ToArgb(); break;
            }
            PaletaAplicacion.Aplicar(preferencias);
            AplicarVistaPrevia();
        }

        private void AplicarVistaPrevia()
        {
            PaletaAplicacion.Aplicar(preferencias);
            // Vuelve a los colores del diseñador antes de repintar la vista
            // previa; de otro modo algunos controles conservarían la plantilla
            // anterior después de restablecer.
            TemaAplicacion.Restablecer(this);
            if (!PaletaAplicacion.TieneColoresPersonalizados)
            {
                IconosGoogle.AplicarPaleta(this);
                ActualizarListasTransparentes();
                return;
            }
            pnlTop.BackColor = Color.FromArgb(preferencias.Cabecera);
            BackColor = Color.FromArgb(preferencias.FondoVentana);
            pnlTarjeta.Back = Color.FromArgb(preferencias.TarjetaOscura);
            pnlSubTarjetaEjemplo.Back = Color.FromArgb(preferencias.FondoSubTarjeta);
            pnlTareaDiaActual.Back = Color.FromArgb(preferencias.PanelDia);
            pnlTareaDiaMes.Back = Color.FromArgb(preferencias.PanelDia);
            pnlTareaDiaSeleccionado.Back = Color.FromArgb(preferencias.PanelDia);
            pnlTareaFueraDelMes.Back = Color.FromArgb(preferencias.PanelDia);
            pnlDiaActual.Back = Color.FromArgb(preferencias.DiaActual);
            pnlDiaMes.Back = Color.FromArgb(preferencias.DiaNormal);
            pnlDiaSeleccionado.Back = Color.FromArgb(preferencias.DiaSeleccionado);
            pnlDiaFueraMes.Back = Color.FromArgb(preferencias.DiaFueraDeMes);
            pnlTargetaTarea.Back = Color.FromArgb(preferencias.SuperficieTarea);
            pnlLayoutLista.Back = Color.FromArgb(preferencias.SuperficieClara);
            pnlTextoGrande.Back = Color.FromArgb(preferencias.FondoTextoGrande);
            pnlTextoPequeño.Back = Color.FromArgb(preferencias.FondoTextoPequeno);
            Color fondoMiniTitulo = Color.FromArgb(preferencias.FondoTextoPequeno);
            pnlPequenoFuenteActual.Back = fondoMiniTitulo;
            pnlPequenoTamanoFuente.Back = fondoMiniTitulo;
            pnlPequenoFondoOscuro.Back = fondoMiniTitulo;
            pnlPequenoFondoClaro.Back = fondoMiniTitulo;
            AplicarBotonPrevia(btnSumar, Color.FromArgb(preferencias.BotonSumar));
            AplicarBotonPrevia(btnRestar, Color.FromArgb(preferencias.BotonRestar));
            AplicarBotonPrevia(btnCerrar, Color.FromArgb(preferencias.BotonCerrar));
            AplicarBotonPrevia(btnAgregarTarea, Color.FromArgb(preferencias.BotonAgregarTarea));
            AplicarBotonPrevia(btnEjemplo, Color.FromArgb(preferencias.Primario));
            ActualizarListasTransparentes();
            AplicarContrasteEtiquetas(this);
            IconosGoogle.AplicarPaleta(this);
            ActualizarValoresFuente();
            AplicarPaletaEnVentanasPropietarias();
        }

        private void ConfigurarListasTransparentes()
        {
            foreach (ListBox lista in new[] { listTareaDiaActual, listTareaDiaSeleccionado, listTareaDiaFueraMes, listTareaDiaMes })
            {
                lista.DrawMode = DrawMode.OwnerDrawFixed;
                lista.SelectionMode = SelectionMode.None;
                lista.BorderStyle = BorderStyle.None;
                lista.DrawItem += ListaTransparente_DrawItem;
            }
        }

        private void ActualizarListasTransparentes()
        {
            foreach (ListBox lista in new[] { listTareaDiaActual, listTareaDiaSeleccionado, listTareaDiaFueraMes, listTareaDiaMes })
            {
                Color fondo = ObtenerFondoVisual(lista.Parent);
                lista.BackColor = fondo;
                lista.ForeColor = PaletaAplicacion.ColorTextoContraste(fondo);
                lista.Invalidate();
            }
        }

        private static Color ObtenerFondoVisual(Control control)
        {
            return PaletaAplicacion.ObtenerFondoVisible(control);
        }

        private void ListaTransparente_DrawItem(object sender, DrawItemEventArgs e)
        {
            var lista = sender as ListBox;
            if (lista == null || e.Index < 0 || e.Index >= lista.Items.Count) return;
            Color fondo = ObtenerFondoVisual(lista.Parent);
            using (var pincel = new SolidBrush(fondo)) e.Graphics.FillRectangle(pincel, e.Bounds);
            TextRenderer.DrawText(e.Graphics, lista.GetItemText(lista.Items[e.Index]), lista.Font,
                Rectangle.Inflate(e.Bounds, -2, 0), PaletaAplicacion.ColorTextoContraste(fondo),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private static void AplicarBotonPrevia(AntdUI.Button boton, Color fondo)
        {
            boton.BackColor = fondo;
            boton.BackHover = PaletaAplicacion.AzulHover;
            boton.BackActive = PaletaAplicacion.AzulPrimario;
            if (boton.DefaultBack.HasValue && boton.DefaultBack.Value.A > 0)
                boton.DefaultBack = fondo;
            boton.ForeColor = PaletaAplicacion.ColorTextoContraste(fondo);
        }

        private static void AplicarContrasteEtiquetas(Control raiz)
        {
            foreach (Control control in raiz.Controls)
            {
                if (control is Label || control is AntdUI.Label)
                    control.ForeColor = PaletaAplicacion.ColorTextoContraste(control);
                if (control is AntdUI.Button || control is ListBox)
                    control.ForeColor = PaletaAplicacion.ColorTextoContraste(control);
                AplicarContrasteEtiquetas(control);
            }
        }

        private void AplicarPaletaEnVentanasPropietarias()
        {
            Form propietario = Owner;
            while (propietario != null && !propietario.IsDisposed)
            {
                TemaAplicacion.Aplicar(propietario, TemaAplicacion.ModoOscuroActual);
                propietario = propietario.Owner;
            }
        }

        private void MostrarSelectorColor(Control ancla, Point puntoClicLocal,
            Color inicial, Action<Color> aplicar)
        {
            if (selectorColorAbierto && (selectorColorPopup == null ||
                selectorColorPopup.IsDisposed || !selectorColorPopup.Visible))
                LiberarSelectorColor(selectorColorPopup);
            if (selectorColorAbierto) return;
            selectorColorAbierto = true;
            try
            {
                var configuracion = new AntdUI.ColorPicker.Config(
                    new AntdUI.Target(ancla), color =>
                {
                    if (color != Color.Empty) aplicar(color);
                })
                {
                    Value = inicial,
                    Mode = AntdUI.TColorMode.Hex,
                    DisabledAlpha = true,
                    ShowClose = true,
                    ShowReset = true,
                    Presets = PresetsSelectorColor,
                    Radius = 8,
                    Font = Font,
                    Placement = AntdUI.TAlignFrom.TL,
                    Offset = new Rectangle(puntoClicLocal.X, puntoClicLocal.Y, 0, 0)
                };
                selectorColorPopup = AntdUI.ColorPicker.open(configuracion);
                if (selectorColorPopup == null)
                {
                    LiberarSelectorColor(null);
                    return;
                }
                Form popup = selectorColorPopup;
                popup.FormClosed += (s, e) => LiberarSelectorColor(popup);
                popup.Disposed += (s, e) => LiberarSelectorColor(popup);
                popup.VisibleChanged += (s, e) =>
                {
                    if (!popup.Visible) LiberarSelectorColor(popup);
                };
                if (verificadorSelectorColor == null)
                {
                    verificadorSelectorColor = new System.Windows.Forms.Timer { Interval = 200 };
                    verificadorSelectorColor.Tick += (s, e) =>
                    {
                        Form activo = selectorColorPopup;
                        if (activo == null || activo.IsDisposed || !activo.Visible)
                            LiberarSelectorColor(activo);
                    };
                }
                verificadorSelectorColor.Start();
            }
            catch
            {
                LiberarSelectorColor(selectorColorPopup);
                throw;
            }
        }

        private static void LiberarSelectorColor(Form popup)
        {
            if (popup != null && selectorColorPopup != null &&
                !ReferenceEquals(popup, selectorColorPopup)) return;
            selectorColorPopup = null;
            selectorColorAbierto = false;
            if (verificadorSelectorColor != null) verificadorSelectorColor.Stop();
        }

        private void ElegirFuente()
        {
            using (var selector = new Form())
            using (var fuentes = new ComboBox())
            using (var aceptar = new Button())
            using (var cancelar = new Button())
            using (var etiqueta = new Label())
            {
                selector.Text = "Elegir fuente";
                selector.FormBorderStyle = FormBorderStyle.FixedDialog;
                selector.StartPosition = FormStartPosition.CenterParent;
                selector.MinimizeBox = false;
                selector.MaximizeBox = false;
                selector.ShowInTaskbar = false;
                selector.ClientSize = new Size(360, 112);
                etiqueta.Text = "Fuente actual";
                etiqueta.AutoSize = true;
                etiqueta.Location = new Point(14, 14);
                fuentes.DropDownStyle = ComboBoxStyle.DropDownList;
                fuentes.Location = new Point(14, 38);
                fuentes.Width = 330;
                foreach (string nombre in FontFamily.Families.Select(f => f.Name).Distinct().OrderBy(n => n))
                    fuentes.Items.Add(nombre);
                if (!fuentes.Items.Contains(preferencias.FuenteActual)) fuentes.Items.Add(preferencias.FuenteActual);
                fuentes.SelectedItem = preferencias.FuenteActual;
                if (fuentes.SelectedIndex < 0 && fuentes.Items.Count > 0) fuentes.SelectedIndex = 0;
                aceptar.Text = "Aceptar";
                aceptar.DialogResult = DialogResult.OK;
                aceptar.Location = new Point(170, 74);
                cancelar.Text = "Cancelar";
                cancelar.DialogResult = DialogResult.Cancel;
                cancelar.Location = new Point(254, 74);
                selector.Controls.AddRange(new Control[] { etiqueta, fuentes, aceptar, cancelar });
                selector.AcceptButton = aceptar;
                selector.CancelButton = cancelar;
                AjustePantalla.Preparar(selector);
                if (selector.ShowDialog(this) != DialogResult.OK || fuentes.SelectedItem == null) return;
                preferencias.FuenteActual = fuentes.SelectedItem.ToString();
                AplicarVistaPrevia();
            }
        }

        private void ElegirTamanoFuente()
        {
            using (var selector = new Form())
            {
                selector.Text = "Tamaño de fuente";
                selector.FormBorderStyle = FormBorderStyle.FixedDialog;
                selector.StartPosition = FormStartPosition.CenterParent;
                selector.MinimizeBox = false;
                selector.MaximizeBox = false;
                selector.ShowInTaskbar = false;
                selector.ClientSize = new Size(210, 176);
                selector.Controls.Add(new Label
                {
                    Text = "Selecciona el tamaño",
                    AutoSize = true,
                    Location = new Point(14, 12)
                });

                string[] tamanos = { "Pequeño", "Normal", "Grande" };
                for (int i = 0; i < tamanos.Length; i++)
                {
                    string tamano = tamanos[i];
                    var opcion = new RadioButton
                    {
                        Text = tamano,
                        AutoSize = true,
                        Location = new Point(18, 40 + (i * 29)),
                        Checked = tamano == preferencias.TamanoFuente
                    };
                    selector.Controls.Add(opcion);
                }

                var aceptar = new Button { Text = "Aceptar", DialogResult = DialogResult.OK, Location = new Point(30, 135) };
                var cancelar = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Location = new Point(115, 135) };
                selector.Controls.Add(aceptar);
                selector.Controls.Add(cancelar);
                selector.AcceptButton = aceptar;
                selector.CancelButton = cancelar;
                AjustePantalla.Preparar(selector);
                if (selector.ShowDialog(this) != DialogResult.OK) return;
                RadioButton seleccion = selector.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked);
                if (seleccion == null || seleccion.Text == preferencias.TamanoFuente) return;
                preferencias.TamanoFuente = seleccion.Text;
                AplicarVistaPrevia();
            }
        }

        private void ElegirColorTexto(bool sobreFondoOscuro)
        {
            Color inicial = sobreFondoOscuro
                ? preferencias.ColorTextoOscuro
                : preferencias.ColorTextoClaro;
            Control ancla = sobreFondoOscuro ? (Control)lblFondoOscuro : lblFondoClaro;
            var punto = new Point(ancla.Width / 2, ancla.Height / 2);
            MostrarSelectorColor(ancla, punto, inicial, color => CambiarColor(
                sobreFondoOscuro ? RolColor.TextoOscuro : RolColor.TextoClaro, color));
        }







        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (idUsuario > 0) AparienciaUsuarioApp.Guardar(idUsuario, preferencias);
                PaletaAplicacion.Aplicar(preferencias);
                cierreAutorizado = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo guardar la apariencia: " + ex.Message,
                    "Apariencia", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            bool cambiosPendientes = HayCambiosPendientes();
            if (cambiosPendientes && MessageBox.Show(this,
                "¿Seguro que quieres cancelar? Se perderán los cambios de apariencia que no hayas guardado.",
                "Cancelar apariencia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
            if (cambiosPendientes) RevertirCambios();
            cierreAutorizado = true;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FrmApariencia_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cierreAutorizado || !HayCambiosPendientes()) return;
            if (MessageBox.Show(this,
                "¿Seguro que quieres cerrar? Se perderán los cambios de apariencia que no hayas guardado.",
                "Cancelar apariencia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                RevertirCambios();
                cierreAutorizado = true;
                DialogResult = DialogResult.Cancel;
            }
            else e.Cancel = true;
        }

        private bool HayCambiosPendientes()
        {
            return !AparienciasIguales(preferencias, preferenciasOriginales);
        }

        private static bool AparienciasIguales(PreferenciasApariencia a, PreferenciasApariencia b)
        {
            if (a == null || b == null) return a == b;
            return a.FuenteActual == b.FuenteActual && a.TamanoFuente == b.TamanoFuente &&
                a.ColorTextoFondoOscuro == b.ColorTextoFondoOscuro &&
                a.ColorTextoFondoClaro == b.ColorTextoFondoClaro &&
                a.TextoTarjetaEjemplo == b.TextoTarjetaEjemplo &&
                a.TextoDescripcionTarjeta == b.TextoDescripcionTarjeta &&
                a.FondoTextoGrande == b.FondoTextoGrande &&
                a.FondoTextoPequeno == b.FondoTextoPequeno &&
                a.IconosTarjeta == b.IconosTarjeta && a.IconosPrincipal == b.IconosPrincipal &&
                a.FondoVentana == b.FondoVentana && a.FondoOscuro == b.FondoOscuro &&
                a.SuperficieOscura == b.SuperficieOscura && a.Cabecera == b.Cabecera &&
                a.TextoCabecera == b.TextoCabecera && a.Primario == b.Primario &&
                a.PrimarioHover == b.PrimarioHover && a.TarjetaOscura == b.TarjetaOscura &&
                a.FondoSubTarjeta == b.FondoSubTarjeta &&
                a.SuperficieClara == b.SuperficieClara && a.SuperficieTarea == b.SuperficieTarea &&
                a.BotonSumar == b.BotonSumar && a.BotonRestar == b.BotonRestar &&
                a.Exito == b.Exito && a.BotonCerrar == b.BotonCerrar &&
                a.BotonBorrar == b.BotonBorrar && a.BotonAgregarTarea == b.BotonAgregarTarea &&
                a.PanelDia == b.PanelDia && a.DiaNormal == b.DiaNormal &&
                a.DiaSeleccionado == b.DiaSeleccionado && a.DiaActual == b.DiaActual &&
                a.DiaFueraDeMes == b.DiaFueraDeMes && a.BordeSuave == b.BordeSuave;
        }

        private void BtnRestablecer_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "¿Restablecer la apariencia a los colores y fuente originales?",
                "Restablecer apariencia", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            preferencias = PreferenciasApariencia.PorDefecto;
            PaletaAplicacion.Aplicar(preferencias);
            AplicarVistaPrevia();
        }

        private void RevertirCambios()
        {
            preferencias = preferenciasOriginales.Copiar();
            PaletaAplicacion.Aplicar(preferencias);
            AplicarVistaPrevia();
        }
    }
}
