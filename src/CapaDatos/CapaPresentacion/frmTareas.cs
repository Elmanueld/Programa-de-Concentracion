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

    public partial class frmTareas : FormularioSuave
    {
        private readonly int idUsuario;
        private readonly DateTime fechaSeleccionada;
        private readonly bool esPlantilla;
        private readonly E_Tarea tareaEditar;
        private readonly List<E_Aplicacion> appsSeleccionadas =
            new List<E_Aplicacion>();
        private readonly List<E_Web> websSeleccionadas =
            new List<E_Web>();
        private bool mostrarAplicaciones = true;

        public frmTareas() : this(0, DateTime.Today, false, null)
        {
        }

        public frmTareas(int idUsuario, DateTime fechaSeleccionada)
            : this(idUsuario, fechaSeleccionada, false, null)
        {
        }

        public frmTareas(int idUsuario, DateTime fechaSeleccionada, bool esPlantilla)
            : this(idUsuario, fechaSeleccionada, esPlantilla, null)
        {
        }

        public frmTareas(int idUsuario, E_Tarea tarea)
            : this(idUsuario,
                tarea == null ? DateTime.Today : tarea.FechaInicio.Date,
                false, tarea)
        {
            if (tarea == null) throw new ArgumentNullException("tarea");
        }

        private frmTareas(int idUsuario, DateTime fechaSeleccionada, bool esPlantilla, E_Tarea tareaEditar)
        {
            InitializeComponent();
            if (EnModoDiseno) return;
            AntdUI.Localization.Provider = new LocalizacionAntdEspanol();
            AntdUI.Localization.DefaultLanguage = "es-ES";
            AntdUI.Localization.SetLanguage("es-ES");
            IconosGoogle.Aplicar(pbSubTargetaTiempoDescanso, "timer");
            IconosGoogle.Aplicar(pbTargetaIntervalo, "alarm");
            IconosGoogle.Aplicar(pbTargetaDescanso, "coffee");
            IconosGoogle.Aplicar(pbTargetaAgregar, "dashboard_customize");
            IconosGoogle.Aplicar(pbTargetaTarea, "assignment_add");
            IconosGoogle.Aplicar(btnAgregarWeb, "add_link");
            IconosGoogle.Aplicar(btnAgregarApp, "apps");
            IconosGoogle.Aplicar(btnGuardar, "save");
            IconosGoogle.Aplicar(btnCerrar, "close");
            ControlesPresentacion.Optimizar(flLista);
            ArrastreFormulario.Habilitar(this, pnlTop);

            this.idUsuario = idUsuario;
            this.fechaSeleccionada = fechaSeleccionada.Date;
            this.esPlantilla = esPlantilla;
            this.tareaEditar = tareaEditar;
            if (esPlantilla)
            {
                lblTargetaTarea.Text = "Plantilla principal";
                lblPanelTop.Text = "Crea o reemplaza la plantilla";
                btnGuardar.Text = "Guardar plantilla";
            }

            // Los controles muestran solo la hora; la fecha se tomará
            // del día seleccionado en el calendario.
            ConfigurarSelectorHora(dtpHoraInicio, TimeSpan.FromHours(8));
            ConfigurarSelectorHora(dtpHoraFin, TimeSpan.FromHours(9));
            nudDescanso.SuffixText = String.Empty;
            nudDescanso.Text = "5 min";
            nudIntervalo.SuffixText = String.Empty;
            nudIntervalo.Text = "5 min";

            pnlSubTargetaAgregaApp.Cursor = Cursors.Hand;
            pnlSubTargetaAgregarWeb.Cursor = Cursors.Hand;
            lblSubTargetaAgregarApp.Cursor = Cursors.Hand;
            lblSubTargetaAgregarWeb.Cursor = Cursors.Hand;
            pnlSubTargetaAgregaApp.Click += (s, e) => CambiarCategoria(true);
            lblSubTargetaAgregarApp.Click += (s, e) => CambiarCategoria(true);
            pnlSubTargetaAgregarWeb.Click += (s, e) => CambiarCategoria(false);
            lblSubTargetaAgregarWeb.Click += (s, e) => CambiarCategoria(false);
            cbNoBloquearWebs.Click += (s, e) => CambiarCategoria(false);
            cbNoBloquearApps.Click += (s, e) => CambiarCategoria(true);

            btnSumarDescanso.Click += (s, e) => AjustarMinutos(nudDescanso, 1);
            btnRestarDescanso.Click += (s, e) => AjustarMinutos(nudDescanso, -1);
            btnSumarIntervalo.Click += (s, e) => AjustarMinutos(nudIntervalo, 5);
            btnRestarIntervalo.Click += (s, e) => AjustarMinutos(nudIntervalo, -5);
            btnCancelar.Click += btnCancelar_Click;
            CancelButton = btnCancelar;

            if (!esPlantilla && tareaEditar == null)
                AplicarPlantillaPredeterminada();

            CambiarCategoria(true);
            if (tareaEditar != null)
                CargarTareaParaEditar(tareaEditar);

        }

        private void CargarTareaParaEditar(E_Tarea tarea)
        {
            Text = "Editar tarea";
            lblTargetaTarea.Text = "Editar tarea";
            lblPanelTop.Text = "Modifica los datos de la tarea";
            btnGuardar.Text = "Guardar cambios";
            txtNombreTarea.Text = tarea.Nombre ?? String.Empty;
            dtpHoraInicio.Value = tarea.FechaInicio.TimeOfDay;
            dtpHoraFin.Value = tarea.FechaFin.TimeOfDay;
            cbNoBloquearApps.Checked = tarea.NoBloquearApps;
            cbNoBloquearWebs.Checked = tarea.NoBloquearWebs;

            E_Descanso descanso = new N_Descanso().BuscarDescanso(idUsuario, tarea.IdTarea);
            if (descanso != null)
            {
                nudDescanso.Text = (descanso.DuracionDescanso ?? 0) + " min";
                nudIntervalo.Text = (descanso.IntervaloDescanso ?? 0) + " min";
            }

            List<E_Aplicacion> catalogoApps = new N_Aplicacion().BuscarAplicacion(idUsuario, String.Empty);
            foreach (E_TareaAplicacion relacion in new N_TareaAplicacion().Buscar(idUsuario, tarea.IdTarea))
            {
                E_Aplicacion app = catalogoApps.FirstOrDefault(a => a.IdAplicacion == relacion.IdAplicacion);
                if (app != null) appsSeleccionadas.Add(app);
            }

            List<E_Web> catalogoWebs = new N_Web().BuscarWeb(idUsuario, String.Empty);
            foreach (E_TareaWeb relacion in new N_TareaWeb().Buscar(idUsuario, tarea.IdTarea))
            {
                E_Web web = catalogoWebs.FirstOrDefault(w => w.IdWeb == relacion.IdWeb);
                if (web != null) websSeleccionadas.Add(web);
            }
            ActualizarListaSeleccionados();
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

        private void AplicarPlantillaPredeterminada()
        {
            E_Tarea plantilla = new N_Tarea().ObtenerPlantilla(idUsuario);
            if (plantilla == null) return;

            txtNombreTarea.Text = plantilla.Nombre ?? String.Empty;
            dtpHoraInicio.Value = plantilla.FechaInicio.TimeOfDay;
            dtpHoraFin.Value = plantilla.FechaFin.TimeOfDay;
            cbNoBloquearApps.Checked = plantilla.NoBloquearApps;
            cbNoBloquearWebs.Checked = plantilla.NoBloquearWebs;

            E_Descanso descanso = new N_Descanso().BuscarDescanso(idUsuario, plantilla.IdTarea);
            if (descanso != null)
            {
                nudDescanso.Text = (descanso.DuracionDescanso ?? 0) + " min";
                nudIntervalo.Text = (descanso.IntervaloDescanso ?? 0) + " min";
            }

            List<E_TareaAplicacion> relacionesApps =
                new N_TareaAplicacion().Buscar(idUsuario, plantilla.IdTarea);
            if (relacionesApps.Count > 0)
            {
                List<E_Aplicacion> catalogoApps =
                    new N_Aplicacion().BuscarAplicacion(idUsuario, String.Empty);
                appsSeleccionadas.AddRange(relacionesApps
                    .Select(relacion => catalogoApps.FirstOrDefault(app => app.IdAplicacion == relacion.IdAplicacion))
                    .Where(app => app != null));
            }

            List<E_TareaWeb> relacionesWebs =
                new N_TareaWeb().Buscar(idUsuario, plantilla.IdTarea);
            if (relacionesWebs.Count > 0)
            {
                List<E_Web> catalogoWebs = new N_Web().BuscarWeb(idUsuario, String.Empty);
                websSeleccionadas.AddRange(relacionesWebs
                    .Select(relacion => catalogoWebs.FirstOrDefault(web => web.IdWeb == relacion.IdWeb))
                    .Where(web => web != null));
            }
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
                    IdTarea = tareaEditar == null ? 0 : tareaEditar.IdTarea,
                    IdUsuario = idUsuario,
                    Nombre = txtNombreTarea.Text.Trim(),
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    EsPlantilla = esPlantilla,
                    NoBloquearApps = cbNoBloquearApps.Checked,
                    NoBloquearWebs = cbNoBloquearWebs.Checked
                };

                var negocioTarea = new N_Tarea();
                int idTarea;
                if (tareaEditar == null)
                    idTarea = negocioTarea.InsertarTarea(tarea);
                else
                {
                    negocioTarea.ModificarTarea(tarea);
                    idTarea = tarea.IdTarea;
                }

                // Los dos valores deben estar definidos juntos o quedar ambos vacíos.
                int duracionMinutos = LeerMinutos(nudDescanso);
                int intervaloMinutos = LeerMinutos(nudIntervalo);
                int? duracionDescanso = duracionMinutos > 0 && intervaloMinutos > 0
                    ? (int?)duracionMinutos : null;
                int? intervaloDescanso = duracionMinutos > 0 && intervaloMinutos > 0
                    ? (int?)intervaloMinutos : null;

                var negocioDescanso = new N_Descanso();
                E_Descanso descansoActual = tareaEditar == null
                    ? null : negocioDescanso.BuscarDescanso(idUsuario, idTarea);
                var descansoNuevo = new E_Descanso
                {
                    IdDescanso = descansoActual == null ? 0 : descansoActual.IdDescanso,
                    IdTarea = idTarea,
                    DuracionDescanso = duracionDescanso,
                    IntervaloDescanso = intervaloDescanso
                };
                if (descansoActual == null)
                {
                    if (tareaEditar == null || duracionDescanso.HasValue)
                        negocioDescanso.InsertarDescanso(idUsuario, descansoNuevo);
                }
                else negocioDescanso.ModificarDescanso(idUsuario, descansoNuevo);

                var negocioAplicacion = new N_Aplicacion();
                var negocioTareaAplicacion = new N_TareaAplicacion();
                var negocioTareaWeb = new N_TareaWeb();
                if (tareaEditar != null)
                {
                    foreach (E_TareaAplicacion anterior in negocioTareaAplicacion.Buscar(idUsuario, idTarea))
                        negocioTareaAplicacion.Eliminar(idUsuario, idTarea, anterior.IdAplicacion);
                    foreach (E_TareaWeb anterior in negocioTareaWeb.Buscar(idUsuario, idTarea))
                        negocioTareaWeb.Eliminar(idUsuario, idTarea, anterior.IdWeb);
                }

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
            var controles = new List<Control>
            {
                CrearEtiquetaRedondeada(
                mostrarAplicaciones ? "Mostrando aplicaciones" : "Mostrando páginas web",
                PaletaAplicacion.AzulTarjetaOscura)
            };

            if (mostrarAplicaciones)
            {
                if (appsSeleccionadas.Count == 0)
                {
                    controles.Add(CrearListaVacia("Todavía no agregas aplicaciones."));
                }
                else foreach (E_Aplicacion app in appsSeleccionadas)
                {
                    E_Aplicacion elemento = app;
                    var tarjeta = new ucAplicacion { Margin = new Padding(6) };
                    tarjeta.Configurar(app, true);
                    tarjeta.QuitarSolicitado += (s, e) =>
                    {
                        appsSeleccionadas.Remove(elemento);
                        ActualizarListaSeleccionados();
                    };
                    controles.Add(tarjeta);
                }
            }
            else if (websSeleccionadas.Count == 0)
            {
                controles.Add(CrearListaVacia("Todavía no agregas páginas web."));
            }
            else foreach (E_Web web in websSeleccionadas)
            {
                E_Web elemento = web;
                var tarjeta = new ucWeb { Margin = new Padding(6) };
                tarjeta.Configurar(web, true);
                tarjeta.QuitarSolicitado += (s, e) =>
                {
                    websSeleccionadas.Remove(elemento);
                    ActualizarListaSeleccionados();
                };
                controles.Add(tarjeta);
            }
            ControlesPresentacion.Reemplazar(flLista, controles.ToArray());
        }

        private void CambiarCategoria(bool mostrarApps)
        {
            mostrarAplicaciones = mostrarApps;
            AplicarTemaCategoria();
            ActualizarListaSeleccionados();
        }

        internal void AplicarTemaCategoria()
        {
            Color normal = PaletaAplicacion.AzulPrimario;
            Color seleccionado = PaletaAplicacion.AzulHover;
            Color fondoApps = mostrarAplicaciones ? seleccionado : normal;
            Color fondoWebs = mostrarAplicaciones ? normal : seleccionado;
            pnlSubTargetaAgregaApp.Back = fondoApps;
            pnlSubTargetaAgregarWeb.Back = fondoWebs;
            AplicarColorBotonCategoria(btnAgregarApp, fondoApps);
            AplicarColorBotonCategoria(btnAgregarWeb, fondoWebs);
        }

        private static void AplicarColorBotonCategoria(AntdUI.Button boton, Color fondo)
        {
            boton.BackColor = fondo;
            boton.BackHover = PaletaAplicacion.AzulHover;
            boton.BackActive = PaletaAplicacion.AzulPrimario;
            boton.DefaultBack = fondo;
            boton.ForeColor = PaletaAplicacion.ColorTextoContraste(fondo);
            IconosGoogle.AplicarPaleta(boton);
        }

        private Control CrearListaVacia(string mensaje)
        {
            var etiqueta = CrearEtiquetaRedondeada(mensaje, PaletaAplicacion.AzulTarjetaOscura);
            etiqueta.Height = 60;
            return etiqueta;
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
                ForeColor = PaletaAplicacion.ColorTextoContraste(color),
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

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void PnlLayoutLista_Click(object sender, EventArgs e)
        {

        }
    }
}
