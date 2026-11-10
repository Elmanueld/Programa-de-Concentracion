using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaEntidades;
using CapaNegocio;

namespace CapaPresentacion
{

    public partial class frmTareas : Form
    {
        private readonly int idUsuario;
        private readonly DateTime fechaSeleccionada;
        private readonly List<E_Aplicacion> appsSeleccionadas =
            new List<E_Aplicacion>();
        private readonly List<E_Web> websSeleccionadas =
            new List<E_Web>();
        private bool mostrarAplicaciones = true;

        public frmTareas(int idUsuario, DateTime fechaSeleccionada)
        {
            AntdUI.Localization.Provider = new LocalizacionAntdEspanol();
            AntdUI.Localization.DefaultLanguage = "es-ES";
            AntdUI.Localization.SetLanguage("es-ES");
            InitializeComponent();
            Tipografia.Aplicar(this);
            ArrastreFormulario.Habilitar(this, pnlTop);

            this.idUsuario = idUsuario;
            this.fechaSeleccionada = fechaSeleccionada.Date;

            // Los controles muestran solo la hora; la fecha se tomará
            // del día seleccionado en el calendario.
            ConfigurarSelectorHora(dtpHoraInicio, TimeSpan.FromHours(8));
            ConfigurarSelectorHora(dtpHoraFin, TimeSpan.FromHours(9));
            nudDescanso.SuffixText = String.Empty;
            nudDescanso.Text = "5 min";
            nudIntervalo.SuffixText = String.Empty;
            nudIntervalo.Text = "5 min";
            flLista.FlowDirection = FlowDirection.LeftToRight;
            flLista.WrapContents = true;

            pnlApps.Cursor = Cursors.Hand;
            pnlWebs.Cursor = Cursors.Hand;
            label3.Cursor = Cursors.Hand;
            label4.Cursor = Cursors.Hand;
            pnlApps.Click += (s, e) => CambiarCategoria(true);
            label3.Click += (s, e) => CambiarCategoria(true);
            pnlWebs.Click += (s, e) => CambiarCategoria(false);
            label4.Click += (s, e) => CambiarCategoria(false);
            cbNoBloquearWebs.Click += (s, e) => CambiarCategoria(false);
            cbNoBloquearApps.Click += (s, e) => CambiarCategoria(true);

            btnSumarDescanso.Click += (s, e) => AjustarMinutos(nudDescanso, 1);
            btnRestarDescanso.Click += (s, e) => AjustarMinutos(nudDescanso, -1);
            btnSumarIntervalo.Click += (s, e) => AjustarMinutos(nudIntervalo, 5);
            btnRestarIntervalo.Click += (s, e) => AjustarMinutos(nudIntervalo, -5);

            CambiarCategoria(true);

        }

        private static void ConfigurarSelectorHora(
            AntdUI.TimePicker selector,
            TimeSpan horaInicial)
        {
            selector.Format = "hh:mm tt";
            selector.DropDownArrow = true;
            selector.ShowButtonNow = false;
            selector.EnabledValueTextChange = false;
            selector.ValueTimeHorizontal = true;
            selector.Value = horaInicial;
        }

        private sealed class LocalizacionAntdEspanol : AntdUI.ILocalization
        {
            public string GetLocalizedString(string key)
            {
                switch (key)
                {
                    case "Now": return "Ahora";
                    case "OK": return "Aceptar";
                    default: return key;
                }
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!dtpHoraInicio.Value.HasValue || !dtpHoraFin.Value.HasValue)
                    throw new ArgumentException("Selecciona la hora de inicio y la hora de fin.");

                DateTime fechaInicio = fechaSeleccionada.Add(dtpHoraInicio.Value.Value);
                DateTime fechaFin = fechaSeleccionada.Add(dtpHoraFin.Value.Value);

                var tarea = new E_Tarea
                {
                    IdUsuario = idUsuario,
                    Nombre = txtNombreTarea.Text.Trim(),
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    EsPlantilla = false,
                    NoBloquearApps = cbNoBloquearApps.Checked,
                    NoBloquearWebs = cbNoBloquearWebs.Checked
                };

                // Guarda la tarea y obtiene su nuevo IdTarea.
                int idTarea = new N_Tarea().InsertarTarea(tarea);

                // Los dos valores deben estar definidos juntos o quedar ambos vacíos.
                int duracionMinutos = LeerMinutos(nudDescanso);
                int intervaloMinutos = LeerMinutos(nudIntervalo);
                int? duracionDescanso = duracionMinutos > 0 && intervaloMinutos > 0
                    ? (int?)duracionMinutos : null;
                int? intervaloDescanso = duracionMinutos > 0 && intervaloMinutos > 0
                    ? (int?)intervaloMinutos : null;

                new N_Descanso().InsertarDescanso(idUsuario, new E_Descanso
                {
                    IdTarea = idTarea,
                    DuracionDescanso = duracionDescanso,
                    IntervaloDescanso = intervaloDescanso
                });

                var negocioAplicacion = new N_Aplicacion();
                var negocioTareaAplicacion = new N_TareaAplicacion();

                List<E_Aplicacion> aplicacionesRegistradas =
                    negocioAplicacion.BuscarAplicacion(idUsuario, "");

                foreach (E_Aplicacion app in appsSeleccionadas)
                {
                    E_Aplicacion registrada = aplicacionesRegistradas
                        .FirstOrDefault(actual =>
                            String.Equals(
                                actual.Identificador,
                                app.Identificador,
                                StringComparison.OrdinalIgnoreCase));

                    if (registrada == null)
                    {
                        app.IdAplicacion =
                            negocioAplicacion.InsertarAplicacion(idUsuario, app);

                        aplicacionesRegistradas.Add(app);
                        registrada = app;
                    }
                    else
                    {
                        app.IdAplicacion = registrada.IdAplicacion;
                    }

                    negocioTareaAplicacion.Insertar(
                        idUsuario,
                        idTarea,
                        app.IdAplicacion);
                }

                var negocioWeb = new N_Web();
                var negocioTareaWeb = new N_TareaWeb();

                foreach (E_Web web in websSeleccionadas)
                {
                    E_Web registrada = negocioWeb.BuscarWeb(idUsuario, "")
                        .FirstOrDefault(actual =>
                            String.Equals(
                                actual.Link.TrimEnd('/'),
                                web.Link.TrimEnd('/'),
                                StringComparison.OrdinalIgnoreCase));

                    if (registrada == null)
                    {
                        web.IdWeb = negocioWeb.InsertarWeb(idUsuario, web);
                        registrada = web;
                    }

                    if (registrada == null || registrada.IdWeb <= 0)
                    {
                        throw new InvalidOperationException(
                            "No se pudo obtener el registro de la web: " + web.Nombre);
                    }

                    negocioTareaWeb.Insertar(idUsuario, idTarea, registrada.IdWeb);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al guardar la tarea",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void btnAgregarApp_Click(object sender, EventArgs e)
        {
            CambiarCategoria(true);
            using (var selector = new frmSeleccionarApp(idUsuario))
            {
                if (NavegacionFormularios.MostrarModal(this, selector) != DialogResult.OK)
                    return;

                foreach (E_Aplicacion app in selector.AplicacionesSeleccionadas)
                {
                    bool yaAgregada = appsSeleccionadas.Any(actual =>
                        string.Equals(
                            actual.Identificador,
                            app.Identificador,
                            StringComparison.OrdinalIgnoreCase));

                    if (!yaAgregada)
                        appsSeleccionadas.Add(app);
                }
                ActualizarListaSeleccionados();
            }
        }

        private void btnAgregarWeb_Click(object sender, EventArgs e)
        {
            CambiarCategoria(false);
            using (var selector = new frmSeleccionarWeb(idUsuario))
            {
                if (NavegacionFormularios.MostrarModal(this, selector) != DialogResult.OK)
                    return;

                foreach (E_Web web in selector.WebsSeleccionadas)
                {
                    bool yaAgregada = websSeleccionadas.Any(actual =>
                        String.Equals(
                            actual.Link.TrimEnd('/'),
                            web.Link.TrimEnd('/'),
                            StringComparison.OrdinalIgnoreCase));

                    if (!yaAgregada)
                        websSeleccionadas.Add(web);
                }

                ActualizarListaSeleccionados();
            }
        }

        private void ActualizarListaSeleccionados()
        {
            foreach (Control control in flLista.Controls.Cast<Control>().ToList())
            {
                flLista.Controls.Remove(control);
                control.Dispose();
            }

            flLista.Controls.Add(CrearEtiquetaRedondeada(
                mostrarAplicaciones ? "Mostrando aplicaciones" : "Mostrando páginas web",
                Color.FromArgb(37, 63, 118)));

            if (mostrarAplicaciones)
            {
                if (appsSeleccionadas.Count == 0)
                {
                    MostrarListaVacia("Todavía no agregas aplicaciones.");
                    return;
                }

                foreach (E_Aplicacion app in appsSeleccionadas)
                {
                    E_Aplicacion elemento = app;
                    var tarjeta = new ucAplicacion { Margin = new Padding(6) };
                    tarjeta.Configurar(app, true);
                    tarjeta.QuitarSolicitado += (s, e) =>
                    {
                        appsSeleccionadas.Remove(elemento);
                        ActualizarListaSeleccionados();
                    };
                    flLista.Controls.Add(tarjeta);
                }
                return;
            }

            if (websSeleccionadas.Count == 0)
            {
                MostrarListaVacia("Todavía no agregas páginas web.");
                return;
            }

            foreach (E_Web web in websSeleccionadas)
            {
                E_Web elemento = web;
                var tarjeta = new ucWeb { Margin = new Padding(6) };
                tarjeta.Configurar(web, true);
                tarjeta.QuitarSolicitado += (s, e) =>
                {
                    websSeleccionadas.Remove(elemento);
                    ActualizarListaSeleccionados();
                };
                flLista.Controls.Add(tarjeta);
            }
        }

        private void CambiarCategoria(bool mostrarApps)
        {
            mostrarAplicaciones = mostrarApps;
            pnlApps.Back = mostrarApps
                ? Color.FromArgb(89, 133, 225)
                : Color.FromArgb(75, 119, 209);
            pnlWebs.Back = mostrarApps
                ? Color.FromArgb(75, 119, 209)
                : Color.FromArgb(89, 133, 225);
            pnlApps.BorderColor = mostrarApps ? Color.White : Color.FromArgb(10, 15, 28);
            pnlWebs.BorderColor = mostrarApps ? Color.FromArgb(10, 15, 28) : Color.White;
            ActualizarListaSeleccionados();
        }

        private void MostrarListaVacia(string mensaje)
        {
            var etiqueta = CrearEtiquetaRedondeada(mensaje, Color.FromArgb(37, 63, 118));
            etiqueta.Height = 60;
            flLista.Controls.Add(etiqueta);
        }

        private EtiquetaRedondeada CrearEtiquetaRedondeada(string texto, Color color)
        {
            return new EtiquetaRedondeada
            {
                AutoSize = false,
                Width = Math.Max(100, flLista.ClientSize.Width - 24),
                Height = 38,
                Text = texto,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = color,
                Margin = new Padding(6),
                Padding = new Padding(8)
            };
        }

        private static int LeerMinutos(AntdUI.Input entrada)
        {
            int minutos;
            if (!Int32.TryParse(entrada.Text.Trim().ToLowerInvariant().Replace("min", "").Trim(), out minutos) || minutos < 0 || minutos > 1440)
                throw new ArgumentException("Los minutos de descanso deben ser un número entre 0 y 1440.");
            return minutos;
        }

        private static void AjustarMinutos(AntdUI.Input entrada, int cambio)
        {
            int minutos;
            if (!Int32.TryParse(entrada.Text.Trim().ToLowerInvariant().Replace("min", "").Trim(), out minutos)) minutos = 0;
            minutos = Math.Max(0, Math.Min(1440, minutos + cambio));
            entrada.Text = minutos + " min";
        }

        private sealed class EtiquetaRedondeada : Label
        {
            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                if (Width <= 0 || Height <= 0) return;

                int diametro = Math.Min(24, Math.Min(Width, Height));
                using (var trazado = new GraphicsPath())
                {
                    trazado.AddArc(0, 0, diametro, diametro, 180, 90);
                    trazado.AddArc(Width - diametro, 0, diametro, diametro, 270, 90);
                    trazado.AddArc(Width - diametro, Height - diametro, diametro, diametro, 0, 90);
                    trazado.AddArc(0, Height - diametro, diametro, diametro, 90, 90);
                    trazado.CloseFigure();

                    Region regionAnterior = Region;
                    Region = new Region(trazado);
                    if (regionAnterior != null) regionAnterior.Dispose();
                }
            }
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

