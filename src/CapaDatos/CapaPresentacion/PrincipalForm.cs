using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaEntidades;
using CapaNegocio;

namespace CapaPresentacion
{
    public sealed partial class PrincipalForm : FormularioSuave
    {
        private readonly E_Usuario usuario;
        private PreferenciasApariencia aparienciaUsuario;
        private Image fotoPerfilActual;
        private frmAviso avisoBloqueo;
        private frmAviso avisoTareaProxima;
        private frmDescanso formularioDescanso;
        private readonly NotifyIcon iconoNotificacion;
        private readonly ContextMenuStrip menuBandeja;
        private bool permitirCierreCompleto;

        private readonly N_Tarea tareas;
        private readonly N_BloqueoApp bloqueador;
        private readonly N_BloqueoWeb bloqueadorWeb;
        private readonly DataGridView listado = new DataGridView {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
            AllowUserToDeleteRows = false, AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        private DateTime fechaActual = RelojNegocio.Hoy;
        private List<ucDia> diasCalendario = new List<ucDia>();
        private List<E_Tarea> tareasCache;
        private readonly TextBox filtro = new TextBox { Width = 180, MaxLength = 70 };
        private ucDia diaSeleccionado = null;
        private readonly System.Windows.Forms.Timer timerSincronizacion;
        private readonly System.Windows.Forms.Timer timerAvisosTarea;
        private readonly HashSet<string> avisosTareaEmitidos = new HashSet<string>(StringComparer.Ordinal);
        private int sincronizacionEnCurso;
        private bool errorSincronizacionNotificado;
        private bool errorCopiaLocalNotificado;
        private bool errorPreferenciasNotificado;
        private bool errorPreferenciaGuardadaNotificado;
        private bool errorEstadoTareaNotificado;
        private readonly N_SincronizacionOffline sincronizacion;

        public PrincipalForm() : this(null)
        {
        }

        public PrincipalForm(E_Usuario usuario)
        {
            InitializeComponent();
            if (EnModoDiseno) return;
            if (usuario == null) throw new ArgumentNullException("usuario");
            AjustePantalla.Preparar(this);
            Icon iconoPrograma = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
            Icon = iconoPrograma;
            menuBandeja = new ContextMenuStrip();
            menuBandeja.Items.Add("Abrir Programa de Concentración", null, (s, e) => SolicitarMostrarVentanaPrincipal());
            menuBandeja.Items.Add(new ToolStripSeparator());
            menuBandeja.Items.Add("Salir", null, (s, e) => SalirDesdeBandeja());
            iconoNotificacion = new NotifyIcon
            {
                Icon = iconoPrograma,
                Visible = true,
                Text = "Programa de Concentración",
                ContextMenuStrip = menuBandeja
            };
            iconoNotificacion.DoubleClick += (s, e) => SolicitarMostrarVentanaPrincipal();
            tareas = new N_Tarea();
            bloqueador = N_BloqueoApp.Instancia;
            bloqueadorWeb = N_BloqueoWeb.Instancia;
            sincronizacion = new N_SincronizacionOffline();
            timerSincronizacion = new System.Windows.Forms.Timer { Interval = 20000 };
            timerAvisosTarea = new System.Windows.Forms.Timer { Interval = 15000 };
            IconosGoogle.Aplicar(btnSiguiente, "chevron_right");
            IconosGoogle.Aplicar(btnAnterior, "chevron_left");
            IconosGoogle.Aplicar(btnTopMinimizar, "minimize");
            IconosGoogle.Aplicar(btnTopConfiguracion, "settings");
            IconosGoogle.Aplicar(btnTopCerrarSesion, "logout");
            IconosGoogle.Aplicar(pbFotoDePerfil, "account_circle");
            IconosGoogle.Aplicar(btnDetener, "stop_circle");
            ControlesPresentacion.Optimizar(tlpCalendario);
            // Esta lista usa desplazamiento real; la paginación sigue activa
            // en las listas de selección de aplicaciones y webs.
            flTareas.AutoScroll = true;
            flTareas.PaginacionHabilitada = false;
            flTareas.FlowDirection = FlowDirection.TopDown;
            flTareas.WrapContents = false;
            flTareas.Layout += FlTareas_Layout;
            ControlesPresentacion.Optimizar(flTareas);
            ArrastreFormulario.Habilitar(this, pnlTop);

            FormClosing += PrincipalForm_FormClosing;
            FormClosed += (s, e) => Program.QuitarPrincipal(this);
            Program.RegistrarPrincipal(this);

            this.usuario = usuario;
            timerSincronizacion.Tick += TimerSincronizacion_Tick;
            timerSincronizacion.Start();
            aparienciaUsuario = AparienciaUsuarioApp.Cargar(usuario.IdUsuario);
            PaletaAplicacion.Aplicar(aparienciaUsuario);
            PreferenciasUsuario preferenciasIniciales = ObtenerPreferencias();
            iconoNotificacion.Visible = true;
            if (String.Equals(preferenciasIniciales.PantallaInicio, "Ultima", StringComparison.OrdinalIgnoreCase) &&
                preferenciasIniciales.UltimaFechaSeleccionada != DateTime.MinValue)
                fechaActual = preferenciasIniciales.UltimaFechaSeleccionada;
            timerAvisosTarea.Tick += TimerAvisosTarea_Tick;
            timerAvisosTarea.Start();
            TemaAplicacion.EstablecerModoOscuro(preferenciasIniciales.ModoOscuro);
            bloqueador.AplicacionBloqueada +=
                Bloqueador_AplicacionBloqueada;
            bloqueador.ErrorSupervisor += Bloqueador_ErrorSupervisor;
            N_SincronizacionOffline.ErrorAlActualizarCopiaLocal += Sincronizacion_ErrorAlActualizarCopiaLocal;
            bloqueador.DescansoIniciado +=
                Bloqueador_DescansoIniciado;
            bloqueador.DescansoTerminado += Bloqueador_DescansoTerminado;
            bloqueadorWeb.ErrorAplicandoBloqueo += BloqueadorWeb_ErrorAplicandoBloqueo;
            bloqueador.Iniciar(usuario.IdUsuario);
            bloqueadorWeb.Iniciar(usuario.IdUsuario);

            btnTopConfiguracion.Click += BtnConfiguracion_Click;
            btnDetener.Click += BtnDetener_Click;
            Shown += PrincipalForm_Shown;

            CargarCalendario();
            DateTime diaInicio = String.Equals(preferenciasIniciales.PantallaInicio, "Ultima", StringComparison.OrdinalIgnoreCase)
                && preferenciasIniciales.UltimaFechaSeleccionada != DateTime.MinValue
                ? preferenciasIniciales.UltimaFechaSeleccionada.Date : RelojNegocio.Hoy;
            ucDia diaInicioControl = diasCalendario.FirstOrDefault(d => d.Fecha.Date == diaInicio);
            if (diaInicioControl != null) SeleccionarDia(diaInicioControl);
            TemaAplicacion.Aplicar(this, preferenciasIniciales.ModoOscuro);
            ActualizarPerfil(preferenciasIniciales);
            ProcesarEstadosDeTarea();

        }

        private void BtnConfiguracion_Click(object sender, EventArgs e)
        {
            using (var ajustes = new frmAjustes(usuario))
            {
                DialogResult resultado = NavegacionFormularios.MostrarModal(this, ajustes);
                if (resultado == DialogResult.Abort)
                {
                    permitirCierreCompleto = true;
                    Close();
                    return;
                }
                ActualizarPerfil(ObtenerPreferencias());
                iconoNotificacion.Visible = true;
                if (resultado == DialogResult.OK)
                {
                    AparienciaUsuarioApp.Aplicar(usuario.IdUsuario);
                    aparienciaUsuario = AparienciaUsuarioApp.Cargar(usuario.IdUsuario);
                    PreferenciasUsuario preferencias = ObtenerPreferencias();
                    TemaAplicacion.Aplicar(this, preferencias.ModoOscuro);
                    ActualizarColoresDias();
                    bloqueador.ActualizarProgramacion();
                    bloqueadorWeb.ActualizarProgramacion();
                }
            }
        }

        private void ActualizarPerfil(PreferenciasUsuario preferencias)
        {
            lblBienvenida.Text = "Bienvenido, " + (usuario.Nombre ?? String.Empty).Normalize(System.Text.NormalizationForm.FormC);
            ActualizarFotoPerfil(preferencias == null ? null : preferencias.RutaFoto);
        }

        private void ActualizarFotoPerfil(string ruta)
        {
            Image nuevaFoto = null;
            if (!String.IsNullOrWhiteSpace(ruta) && File.Exists(ruta))
            {
                try
                {
                    using (Image original = Image.FromFile(ruta))
                        nuevaFoto = new Bitmap(original);
                }
                catch
                {
                    nuevaFoto = null;
                }
            }

            Image anterior = fotoPerfilActual;
            if (nuevaFoto == null)
            {
                fotoPerfilActual = null;
                IconosGoogle.RestaurarImagenPredeterminada(pbFotoDePerfil);
            }
            else
            {
                fotoPerfilActual = nuevaFoto;
                IconosGoogle.EstablecerImagenPersonalizada(pbFotoDePerfil, nuevaFoto);
            }

            if (anterior != null && !ReferenceEquals(anterior, fotoPerfilActual))
                anterior.Dispose();
        }

        private void ActualizarColoresDias()
        {
            foreach (ucDia dia in diasCalendario)
                dia.ActualizarApariencia(aparienciaUsuario);
        }

        private void PrincipalForm_Shown(object sender, EventArgs e)
        {
            ActualizadorGithub.ComprobarYOfrecer(this);
            PreferenciasUsuario preferencias = ObtenerPreferencias();
            if (Program.InicioDesdeWindows && preferencias.IniciarConWindows && preferencias.IniciarMinimizado)
                MinimizarEnBandeja();

            if (String.Equals((preferencias.PantallaInicio ?? String.Empty).Trim(),
                "Tarea", StringComparison.OrdinalIgnoreCase))
            {
                BeginInvoke(new Action(() =>
                {
                    using (var formulario = new frmTareas(usuario.IdUsuario, RelojNegocio.Hoy))
                        NavegacionFormularios.MostrarModal(this, formulario);
                }));
            }
        }

        private void MostrarVentanaPrincipal()
        {
            if (IsDisposed) return;
            ShowInTaskbar = true;
            if (!Visible) Show();
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            BringToFront();
            Activate();
        }

        private void SolicitarMostrarVentanaPrincipal()
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (!IsDisposed) MostrarVentanaPrincipal();
                }));
            }
            catch (InvalidOperationException ex)
            {
                ErroresAplicacion.Registrar(ex, "No se pudo restaurar la ventana desde la bandeja");
            }
        }

        private void MinimizarEnBandeja()
        {
            iconoNotificacion.Visible = true;
            ShowInTaskbar = false;
            WindowState = FormWindowState.Normal;
            Hide();
        }

        internal void RestaurarDesdeInstanciaSecundaria()
        {
            SolicitarMostrarVentanaPrincipal();
        }

        private void SalirDesdeBandeja()
        {
            Program.SalidaSolicitada = true;
            permitirCierreCompleto = true;
            Close();
        }

        private void PrincipalForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !permitirCierreCompleto &&
                !Program.CierreControladoPorErrorCritico && DialogResult != DialogResult.Retry)
            {
                e.Cancel = true;
                MinimizarEnBandeja();
                return;
            }

            bloqueadorWeb.ErrorAplicandoBloqueo -= BloqueadorWeb_ErrorAplicandoBloqueo;
            bloqueador.ErrorSupervisor -= Bloqueador_ErrorSupervisor;
            bloqueador.DescansoIniciado -= Bloqueador_DescansoIniciado;
            bloqueador.DescansoTerminado -= Bloqueador_DescansoTerminado;
            N_SincronizacionOffline.ErrorAlActualizarCopiaLocal -= Sincronizacion_ErrorAlActualizarCopiaLocal;
            bloqueadorWeb.Detener();
            bloqueador.Detener();
            timerSincronizacion.Stop();
            timerSincronizacion.Dispose();
            timerAvisosTarea.Stop();
            timerAvisosTarea.Dispose();
            if (!Program.CierreControladoPorErrorCritico)
            {
                PreferenciasUsuario prefsAlCerrar = ObtenerPreferencias();
                if (String.Equals(prefsAlCerrar.PantallaInicio, "Ultima", StringComparison.OrdinalIgnoreCase) && diaSeleccionado != null)
                {
                    prefsAlCerrar.UltimaFechaSeleccionada = diaSeleccionado.Fecha.Date;
                    try { PreferenciasUsuarioApp.Guardar(usuario.IdUsuario, prefsAlCerrar); }
                    catch (Exception ex)
                    {
                        OperacionUI.ReportarError(null, ex, "No se pudo guardar la última pantalla",
                            "La aplicación se cerrará; la próxima vez se abrirá la pantalla guardada anteriormente.", true);
                    }
                }
            }
            iconoNotificacion.Visible = false;
            iconoNotificacion.Dispose();
            menuBandeja.Dispose();
            if (fotoPerfilActual != null)
            {
                pbFotoDePerfil.Image = null;
                fotoPerfilActual.Dispose();
                fotoPerfilActual = null;
            }
        }

        private void TimerAvisosTarea_Tick(object sender, EventArgs e)
        {
            ProcesarEstadosDeTarea();
            ActualizarEstadoBotonDetener();
            PreferenciasUsuario preferencias = ObtenerPreferencias();
            int minutos = preferencias.MinutosAnticipacion;
            if (minutos <= 0 || preferencias.NoMostrarAnuncios || preferencias.NoMostrarAvisos ||
                (!preferencias.MostrarComoVentana && !preferencias.MostrarComoNotificacion) || tareasCache == null)
                return;

            DateTime ahora = RelojNegocio.Ahora;
            foreach (E_Tarea tarea in tareasCache.Where(t => t != null && t.Estado == 'P' && !t.EsPlantilla))
            {
                DateTime inicio = tarea.FechaInicio;
                DateTime avisoDesde = inicio.AddMinutes(-minutos);
                if (ahora < avisoDesde || ahora >= inicio) continue;
                string clave = tarea.IdTarea + "|" + inicio.Ticks;
                if (!avisosTareaEmitidos.Add(clave)) continue;

                if (preferencias.MostrarComoNotificacion)
                {
                    iconoNotificacion.Visible = true;
                    iconoNotificacion.ShowBalloonTip(6000, "Tarea próxima",
                        "\"" + tarea.Nombre + "\" empieza a las " + inicio.ToString("HH:mm") + ".", ToolTipIcon.Info);
                }
                if (!preferencias.MostrarComoVentana || VentanaEnBandeja) continue;
                if (avisoTareaProxima == null || avisoTareaProxima.IsDisposed)
                {
                    avisoTareaProxima = new frmAviso();
                    AjustePantalla.Preparar(avisoTareaProxima);
                    avisoTareaProxima.FormClosed += (s, args) => avisoTareaProxima = null;
                }
                TemaAplicacion.Aplicar(avisoTareaProxima, TemaAplicacion.ModoOscuroActual);
                avisoTareaProxima.ConfigurarRecordatorioTarea(tarea.Nombre, inicio, minutos);
                avisoTareaProxima.TopMost = true;
                if (!avisoTareaProxima.Visible) avisoTareaProxima.Show();
            }
        }

        private bool VentanaEnBandeja => !Visible || !ShowInTaskbar;

        private void MostrarNotificacionTareaIniciada(E_Tarea tarea)
        {
            if (tarea == null) return;
            PreferenciasUsuario preferencias = ObtenerPreferencias();
            if (preferencias.NoMostrarAnuncios || preferencias.NoMostrarAvisos ||
                !preferencias.MostrarComoNotificacion || iconoNotificacion == null)
                return;

            try
            {
                iconoNotificacion.Visible = true;
                iconoNotificacion.ShowBalloonTip(6000, "Tarea iniciada",
                    "Comenzó " + tarea.Nombre + ".", ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                ErroresAplicacion.Registrar(ex, "No se pudo mostrar la notificación de inicio de tarea");
            }
        }
        private void ProcesarEstadosDeTarea()
        {
            if (tareasCache == null) return;
            DateTime ahora = RelojNegocio.Ahora;
            bool huboCambios = false;
            bool errorEnCiclo = false;
            foreach (E_Tarea tarea in tareasCache.ToArray())
            {
                if (tarea == null || tarea.EsPlantilla || tarea.Estado == 'C' || tarea.Estado == 'X')
                    continue;
                try
                {
                    if (tarea.Estado == 'P' && ahora >= tarea.FechaInicio)
                    {
                        tareas.CambiarEstadoTarea(usuario.IdUsuario, tarea.IdTarea, 'A');
                        tarea.Estado = 'A';
                        huboCambios = true;
                        MostrarNotificacionTareaIniciada(tarea);
                    }
                    if (tarea.Estado == 'A' && ahora >= tarea.FechaFin)
                    {
                        tareas.CambiarEstadoTarea(usuario.IdUsuario, tarea.IdTarea, 'C');
                        tarea.Estado = 'C';
                        huboCambios = true;
                    }
                }
                catch (Exception ex)
                {
                    char estadoEsperado = ahora >= tarea.FechaFin ? 'C' : 'A';
                    if (ReconciliarEstadoTarea(tarea, estadoEsperado))
                    {
                        // The request may have succeeded while its response was
                        // lost, or this cached task may have been removed remotely.
                        if (estadoEsperado == 'A') MostrarNotificacionTareaIniciada(tarea);
                        huboCambios = true;
                        continue;
                    }

                    errorEnCiclo = true;
                    if (!errorEstadoTareaNotificado)
                    {
                        OperacionUI.ReportarError(this, ex, "No se pudo actualizar el estado de una tarea",
                            "La tarea permanece en su estado guardado. El supervisor volverá a intentarlo.", true);
                        errorEstadoTareaNotificado = true;
                    }
                }
            }

            if (!huboCambios)
            {
                if (!errorEnCiclo) errorEstadoTareaNotificado = false;
                return;
            }
            DateTime fecha = diaSeleccionado == null ? RelojNegocio.Hoy : diaSeleccionado.Fecha;
            try
            {
                tareasCache = tareas.BuscarTarea(usuario.IdUsuario, String.Empty) ?? new List<E_Tarea>();
                ActualizarCalendarioYDia(fecha);
                bloqueador.ActualizarProgramacion();
                bloqueadorWeb.ActualizarProgramacion();
            }
            catch (Exception ex)
            {
                errorEnCiclo = true;
                if (!errorEstadoTareaNotificado)
                {
                    OperacionUI.ReportarError(this, ex, "No se pudo actualizar la vista de tareas",
                        "Los cambios ya guardados se conservan. La pantalla se actualizará en el siguiente ciclo.", true);
                    errorEstadoTareaNotificado = true;
                }
            }
            if (!errorEnCiclo) errorEstadoTareaNotificado = false;
        }

        private bool ReconciliarEstadoTarea(E_Tarea tarea, char estadoEsperado)
        {
            if (tarea == null) return false;
            try
            {
                List<E_Tarea> actualizadas = tareas.BuscarTarea(usuario.IdUsuario, String.Empty)
                    ?? new List<E_Tarea>();
                E_Tarea actual = actualizadas.FirstOrDefault(t => t.IdTarea == tarea.IdTarea);
                if (actual == null || actual.Estado == estadoEsperado)
                {
                    tareasCache = actualizadas;
                    if (actual != null) tarea.Estado = actual.Estado;
                    return true;
                }
            }
            catch (Exception errorLectura)
            {
                ErroresAplicacion.Registrar(errorLectura,
                    "Reconciliar estado de tarea después de un error de actualización");
            }

            return false;
        }

        private void CargarCalendario()
        {
            ControlesPresentacion.IniciarActualizacionVisual(this);
            SuspendLayout();
            try
            {
            string mesAnio = fechaActual.ToString(
                "MMMM yyyy",
                new System.Globalization.CultureInfo("es-ES")
            );

            lblMesAnio.Text = char.ToUpper(mesAnio[0]) + mesAnio.Substring(1);

            DateTime primerDia = new DateTime(
                fechaActual.Year,
                fechaActual.Month,
                1
            );
            if (tareasCache == null)
                tareasCache = tareas.BuscarTarea(usuario.IdUsuario, "") ?? new List<E_Tarea>();


            // Ajustar para que la semana comience en lunes
            int desplazamiento = ((int)primerDia.DayOfWeek + 6) % 7;

            // Fecha del primer espacio del calendario
            DateTime fechaInicio = primerDia.AddDays(-desplazamiento);

            var tareasPorFecha = new Dictionary<DateTime, List<E_Tarea>>();
            for (int i = 0; i < 42; i++)
                tareasPorFecha[fechaInicio.AddDays(i).Date] = new List<E_Tarea>();

            DateTime fechaFinal = fechaInicio.AddDays(41).Date;
            foreach (E_Tarea tarea in tareasCache)
            {
                DateTime desde = tarea.FechaInicio.Date < fechaInicio.Date
                    ? fechaInicio.Date : tarea.FechaInicio.Date;
                DateTime hasta = tarea.FechaFin.Date > fechaFinal
                    ? fechaFinal : tarea.FechaFin.Date;
                for (DateTime fecha = desde; fecha <= hasta; fecha = fecha.AddDays(1))
                {
                    List<E_Tarea> delDia;
                    if (tareasPorFecha.TryGetValue(fecha, out delDia)) delDia.Add(tarea);
                }
            }

            tlpCalendario.SuspendLayout();
            try
            {
                tlpCalendario.Controls.Clear();
                while (diasCalendario.Count < 42)
                {
                    var nuevoDia = new ucDia { Dock = DockStyle.Fill };
                    nuevoDia.SeleccionSolicitada += (s, e) => SeleccionarDia((ucDia)s);
                    diasCalendario.Add(nuevoDia);
                }

                for (int i = 0; i < 42; i++)
                {
                    DateTime fecha = fechaInicio.AddDays(i);

                    ucDia dia = diasCalendario[i];
                    dia.ConfigurarDia(fecha, fecha.Month == fechaActual.Month, aparienciaUsuario);
                    dia.CargarTareas(tareasPorFecha[fecha.Date]
                        .OrderBy(t => t.FechaInicio.TimeOfDay)
                        .ThenBy(t => t.Nombre));

                    tlpCalendario.Controls.Add(dia, i % 7, i / 7);
                }
            }
            finally { tlpCalendario.ResumeLayout(true); }

            foreach (ucDia dia in diasCalendario) dia.ActualizarTema();
            }
            finally
            {
                ResumeLayout(false);
                PerformLayout();
                ControlesPresentacion.FinalizarActualizacionVisual(this);
            }
        }

        private void Control_EliminarSolicitado(object sender, EventArgs e)
        {
            var control = sender as ucTareas;

            if (control == null || control.Tarea == null)
                return;

            DialogResult confirmacion = MessageBox.Show(
                this,
                "¿Seguro que quieres borrar la tarea \"" +
                    control.Tarea.Nombre + "\"?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            DateTime fecha = diaSeleccionado != null
                ? diaSeleccionado.Fecha
                : control.Tarea.FechaInicio.Date;
            if (confirmacion != DialogResult.Yes)
                return;

            try
            {
                tareas.EliminarTarea(
                    usuario.IdUsuario,
                    control.Tarea.IdTarea);

                bloqueador.ActualizarProgramacion();
                bloqueadorWeb.ActualizarProgramacion();
                ActualizarCalendarioYDia(fecha);
            }
            catch (Exception ex)
            {
                OperacionUI.ReportarError(this, ex, "No se pudo borrar la tarea");
            }
        }

        private void ActualizarCalendarioYDia(DateTime fecha)
        {
            tareasCache = tareas.BuscarTarea(usuario.IdUsuario, "") ?? new List<E_Tarea>();
            diaSeleccionado = null;
            CargarCalendario();

            ucDia diaActualizado = diasCalendario
                .FirstOrDefault(dia => dia.Fecha.Date == fecha.Date);

            if (diaActualizado != null)
                SeleccionarDia(diaActualizado);
        }

        private async void TimerSincronizacion_Tick(object sender, EventArgs e)
        {
            if (Interlocked.Exchange(ref sincronizacionEnCurso, 1) != 0) return;
            try
            {
                if (!sincronizacion.EstaEnModoOffline &&
                    !sincronizacion.HayCambiosPendientes(usuario.IdUsuario) &&
                    !sincronizacion.HayCopiaNuevaNecesaria)
                    return;

                bool sincronizada = await Task.Run(() => sincronizacion.Sincronizar(usuario.IdUsuario));
                if (IsDisposed || Disposing) return;
                // La falta temporal de red es una condición normal: los cambios
                // permanecen en la copia local y el siguiente ciclo reintentará.
                if (!sincronizada) return;
                errorSincronizacionNotificado = false;
                errorCopiaLocalNotificado = false;
                DateTime fecha = diaSeleccionado == null ? RelojNegocio.Hoy : diaSeleccionado.Fecha;
                tareasCache = null;
                ActualizarCalendarioYDia(fecha);
                bloqueador.ActualizarProgramacion();
                bloqueadorWeb.ActualizarProgramacion();
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing && !errorSincronizacionNotificado)
                {
                    OperacionUI.ReportarError(this, ex, "No se pudieron enviar los cambios",
                        "Los cambios pendientes se conservan en este equipo y se enviarán automáticamente cuando vuelva la conexión.",
                        true);
                    errorSincronizacionNotificado = true;
                }
            }
            finally { Interlocked.Exchange(ref sincronizacionEnCurso, 0); }
        }

        private void CargarTareasDelDia(DateTime fecha)
        {
            if (tareasCache == null)
                tareasCache = tareas.BuscarTarea(usuario.IdUsuario, "") ?? new List<E_Tarea>();
            List<E_Tarea> tareasDelDia = tareasCache
                .Where(t => t.FechaInicio.Date <= fecha.Date &&
                            t.FechaFin.Date >= fecha.Date)
                .OrderBy(t => t.FechaInicio)
                .ToList();

            var controles = new List<Control>();
            foreach (E_Tarea tarea in tareasDelDia)
            {
                var control = new ucTareas();
                control.CargarTarea(tarea);
                control.EliminarSolicitado += Control_EliminarSolicitado;
                control.EditarSolicitado += Control_EditarSolicitado;
                controles.Add(control);
            }
            ControlesPresentacion.Reemplazar(flTareas, controles.ToArray());
            foreach (ucTareas tarjeta in flTareas.Controls.OfType<ucTareas>())
                tarjeta.ActualizarApariencia();
            ActualizarEstadoBotonDetener();
        }

        private bool centrandoTarjetas;

        private void FlTareas_Layout(object sender, LayoutEventArgs e)
        {
            if (centrandoTarjetas || flTareas == null || flTareas.IsDisposed) return;
            centrandoTarjetas = true;
            try
            {
                int anchoDisponible = flTareas.ClientSize.Width;
                foreach (Control tarjeta in flTareas.Controls)
                {
                    if (tarjeta.IsDisposed) continue;
                    int espacioLateral = Math.Max(0, (anchoDisponible - tarjeta.Width) / 2);
                    Padding margen = tarjeta.Margin;
                    Padding centrado = new Padding(espacioLateral, margen.Top,
                        espacioLateral, margen.Bottom);
                    if (margen != centrado) tarjeta.Margin = centrado;
                }
            }
            finally { centrandoTarjetas = false; }
        }

        private E_Tarea ObtenerTareaActual()
        {
            if (tareasCache == null) return null;
            DateTime ahora = RelojNegocio.Ahora;
            return tareasCache
                .Where(t => t != null && !t.EsPlantilla && (t.Estado == 'P' || t.Estado == 'A') &&
                    ahora >= t.FechaInicio && ahora < t.FechaFin)
                .OrderByDescending(t => t.FechaInicio)
                .FirstOrDefault();
        }

        private void ActualizarEstadoBotonDetener()
        {
            if (btnDetener != null && !btnDetener.IsDisposed)
                btnDetener.Enabled = ObtenerTareaActual() != null;
        }

        private void BtnDetener_Click(object sender, EventArgs e)
        {
            E_Tarea tarea = ObtenerTareaActual();
            if (tarea == null)
            {
                MessageBox.Show(this, "No hay una tarea en curso para detener.", "Detener tarea",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ActualizarEstadoBotonDetener();
                return;
            }

            DialogResult confirmar = MessageBox.Show(this,
                "¿Seguro que quieres detener \"" + tarea.Nombre + "\"?\r\n\r\n" +
                "Se quitarán sus bloqueos y avisos. Esta tarea quedará marcada como no completada y no podrás volver a activarla.",
                "Detener tarea", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmar != DialogResult.Yes) return;

            try
            {
                tareas.CambiarEstadoTarea(usuario.IdUsuario, tarea.IdTarea, 'X');
                if (avisoBloqueo != null && !avisoBloqueo.IsDisposed) avisoBloqueo.Close();
                if (formularioDescanso != null && !formularioDescanso.IsDisposed) formularioDescanso.Close();
                bloqueador.ActualizarProgramacion();
                bloqueadorWeb.ActualizarProgramacion();
                ActualizarCalendarioYDia(diaSeleccionado == null ? RelojNegocio.Hoy : diaSeleccionado.Fecha);
            }
            catch (Exception ex)
            {
                OperacionUI.ReportarError(this, ex, "No se pudo detener la tarea");
            }
        }

        private void Control_EditarSolicitado(object sender, EventArgs e)
        {
            ucTareas control = sender as ucTareas;
            if (control == null || control.Tarea == null || control.Tarea.Estado == 'X' || control.Tarea.Estado == 'A')
                return;
            DateTime fecha = diaSeleccionado == null ? control.Tarea.FechaInicio.Date : diaSeleccionado.Fecha;
            try
            {
                using (var formulario = new frmTareas(usuario.IdUsuario, control.Tarea))
                {
                    if (NavegacionFormularios.MostrarModal(this, formulario) != DialogResult.OK) return;
                }
                bloqueador.ActualizarProgramacion();
                bloqueadorWeb.ActualizarProgramacion();
                ActualizarCalendarioYDia(fecha);
            }
            catch (Exception ex)
            {
                OperacionUI.ReportarError(this, ex, "No se pudo editar la tarea");
            }
        }

        private void SeleccionarDia(ucDia dia)
        {
            if (diaSeleccionado != null)
                diaSeleccionado.Seleccionar(false);

            diaSeleccionado = dia;
            diaSeleccionado.Seleccionar(true);

            PreferenciasUsuario preferencias = ObtenerPreferencias();
            if (String.Equals(preferencias.PantallaInicio, "Ultima", StringComparison.OrdinalIgnoreCase))
            {
                preferencias.UltimaFechaSeleccionada = dia.Fecha.Date;
                try
                {
                    PreferenciasUsuarioApp.Guardar(usuario.IdUsuario, preferencias);
                    errorPreferenciaGuardadaNotificado = false;
                }
                catch (Exception ex)
                {
                    if (!errorPreferenciaGuardadaNotificado)
                    {
                        OperacionUI.ReportarError(this, ex, "No se pudo guardar la fecha de inicio",
                            "La selección actual del calendario se mantiene, pero no se recordará al volver a abrir la aplicación.", true);
                        errorPreferenciaGuardadaNotificado = true;
                    }
                }
            }

            CargarTareasDelDia(dia.Fecha);
        }
        private void MesAnterior()
        {
            fechaActual = fechaActual.AddMonths(-1);
            diaSeleccionado = null;
            CargarCalendario();
        }

        private void MesSiguiente()
        {
            fechaActual = fechaActual.AddMonths(1);
            diaSeleccionado = null;
            CargarCalendario();
        }

        private void BtnSiguiente_Click(object sender, EventArgs e)
        {
            MesSiguiente();
        }

        private void BtnAnterior_Click(object sender, EventArgs e)
        {
            MesAnterior();
        }

        private void btnAgregarTarea_Click(object sender, EventArgs e)
        {
            if (diaSeleccionado == null)
            {
                MessageBox.Show("Selecciona primero un día del calendario.");
                return;
            }

            using (var formulario = new frmTareas(usuario.IdUsuario, diaSeleccionado.Fecha))
            {
                DateTime fecha = diaSeleccionado.Fecha;

                if (NavegacionFormularios.MostrarModal(this, formulario) == DialogResult.OK)
                {
                    bloqueador.ActualizarProgramacion();
                    bloqueadorWeb.ActualizarProgramacion();
                    ActualizarCalendarioYDia(fecha);
                }
            }
        }

        private void Bloqueador_AplicacionBloqueada(
    object sender,
    AplicacionBloqueadaEventArgs e)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action(
                        () => MostrarAvisoBloqueo(e)));
                }
                catch (InvalidOperationException)
                {
                    // El formulario pudo cerrarse antes de procesar el aviso.
                }

                return;
            }

            MostrarAvisoBloqueo(e);
        }

        private void MostrarAvisoBloqueo(
            AplicacionBloqueadaEventArgs e)
        {
            if (IsDisposed)
                return;

            PreferenciasUsuario preferencias = ObtenerPreferencias();
            if (preferencias.NoMostrarAnuncios || preferencias.NoMostrarAvisos) return;
            if (preferencias.MostrarComoNotificacion)
            {
                iconoNotificacion.Visible = true;
                iconoNotificacion.ShowBalloonTip(5000, "Página o aplicación bloqueada",
                    "" + e.NombreAplicacion + " está bloqueada durante " + e.NombreTarea + ".", ToolTipIcon.Info);
            }
            if (!preferencias.MostrarComoVentana || VentanaEnBandeja) return;

            if (avisoBloqueo == null || avisoBloqueo.IsDisposed)
            {
                avisoBloqueo = new frmAviso();
                AjustePantalla.Preparar(avisoBloqueo);

                avisoBloqueo.FormClosed += (s, args) =>
                {
                    avisoBloqueo = null;
                };
            }

            TemaAplicacion.Aplicar(avisoBloqueo, TemaAplicacion.ModoOscuroActual);

            avisoBloqueo.ConfigurarContexto(
                e.NombreAplicacion,
                e.NombreTarea);

            avisoBloqueo.TopMost = true;

            if (!avisoBloqueo.Visible)
                avisoBloqueo.Show();
        }

        private void BtnCerrarSesion_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "¿Quieres cerrar sesión y volver al inicio?",
                "Cerrar sesión", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try { new N_Usuario().CerrarSesion(usuario.IdUsuario); }
            catch (Exception ex)
            {
                OperacionUI.ReportarError(this, ex, "No se pudo actualizar el inicio automático");
            }
            DialogResult = DialogResult.Retry;
            permitirCierreCompleto = true;
            Close();
        }

        private void BloqueadorWeb_ErrorAplicandoBloqueo(Exception error, string contexto)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (!IsDisposed)
                        OperacionUI.ReportarError(this, error, "No se pudo aplicar el bloqueo web",
                            contexto + " Revisa el filtro web y vuelve a intentarlo.", true);
                }));
            }
            catch (InvalidOperationException ex)
            {
                ErroresAplicacion.Registrar(ex, "Notificar error del filtro web tras cerrar la ventana");
            }
        }

        private void Bloqueador_ErrorSupervisor(Exception error)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (!IsDisposed)
                        OperacionUI.ReportarError(this, error, "No se pudo actualizar el bloqueo de aplicaciones",
                            "Se conserva la última programación válida y el supervisor volverá a intentarlo.", true);
                }));
            }
            catch (InvalidOperationException ex)
            {
                ErroresAplicacion.Registrar(ex, "Notificar error del supervisor tras cerrar la ventana");
            }
        }

        private void Sincronizacion_ErrorAlActualizarCopiaLocal(Exception error)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (IsDisposed || errorCopiaLocalNotificado) return;
                    OperacionUI.ReportarError(this, error, "La copia local no se actualizó",
                        "El cambio sí se guardó en tu cuenta, pero la información de este equipo quedó desactualizada. Se actualizará de nuevo; no repitas el cambio.", true);
                    errorCopiaLocalNotificado = true;
                }));
            }
            catch (InvalidOperationException ex)
            {
                ErroresAplicacion.Registrar(ex, "Notificar error al actualizar copia local tras cerrar la ventana");
            }
        }

        private void Bloqueador_DescansoIniciado(
    object sender,
    DescansoIniciadoEventArgs e)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action(
                        () => MostrarAvisoDescansoSeguro(e)));
                }
                catch (InvalidOperationException)
                {
                    // El formulario pudo cerrarse antes de procesar el aviso.
                }

                return;
            }

            MostrarAvisoDescansoSeguro(e);
        }

        private void Bloqueador_DescansoTerminado(
            object sender,
            DescansoIniciadoEventArgs e)
        {
            if (IsDisposed || !IsHandleCreated) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(() => MostrarAvisoDescansoTerminadoSeguro(e))); }
                catch (InvalidOperationException) { }
                return;
            }
            MostrarAvisoDescansoTerminadoSeguro(e);
        }

        private void MostrarAvisoDescansoTerminadoSeguro(DescansoIniciadoEventArgs e)
        {
            try
            {
                PreferenciasUsuario preferencias = ObtenerPreferencias();
                if (preferencias.NoMostrarAnuncios || preferencias.NoMostrarDescansos ||
                    !preferencias.MostrarComoNotificacion || iconoNotificacion == null)
                    return;

                iconoNotificacion.Visible = true;
                iconoNotificacion.ShowBalloonTip(6000, "Descanso terminado",
                    "Terminó el descanso de " + e.NombreTarea + ". Puedes continuar con tu tarea.",
                    ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                ErroresAplicacion.Registrar(ex, "No se pudo mostrar el aviso de fin de descanso");
            }
        }

        private void MostrarAvisoDescansoSeguro(DescansoIniciadoEventArgs e)
        {
            try { MostrarAvisoDescanso(e); }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing)
                    OperacionUI.ReportarError(this, ex, "No se pudo mostrar el aviso de descanso");
                else
                    ErroresAplicacion.Registrar(ex, "Mostrar descanso después de cerrar la pantalla");
            }
        }

        private void MostrarAvisoDescanso(
            DescansoIniciadoEventArgs e)
        {
            if (IsDisposed)
                return;

            PreferenciasUsuario preferencias = ObtenerPreferencias();
            if (preferencias.NoMostrarAnuncios || preferencias.NoMostrarDescansos) return;
            if (preferencias.MostrarComoNotificacion)
            {
                iconoNotificacion.Visible = true;
                iconoNotificacion.ShowBalloonTip(5000, "Hora de descansar",
                    "Comenzó el descanso de " + e.NombreTarea + ".", ToolTipIcon.Info);
            }
            if (!preferencias.MostrarComoVentana || VentanaEnBandeja) return;

            if (avisoBloqueo != null && !avisoBloqueo.IsDisposed)
                avisoBloqueo.Close();

            if (formularioDescanso == null ||
                formularioDescanso.IsDisposed)
            {
                formularioDescanso = new frmDescanso();
                AjustePantalla.Preparar(formularioDescanso);

                formularioDescanso.FormClosed += (s, args) =>
                {
                    formularioDescanso = null;
                };
            }

            TemaAplicacion.Aplicar(formularioDescanso, TemaAplicacion.ModoOscuroActual);

            formularioDescanso.IniciarCuentaRegresiva(
                e.NombreTarea,
                e.FinDescanso,
                e.DescansosRestantes);

            formularioDescanso.TopMost = true;

            if (!formularioDescanso.Visible)
                formularioDescanso.Show();
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            MinimizarEnBandeja();
        }

        private PreferenciasUsuario ObtenerPreferencias()
        {
            try
            {
                PreferenciasUsuario resultado = PreferenciasUsuarioApp.Cargar(usuario.IdUsuario);
                errorPreferenciasNotificado = false;
                return resultado;
            }
            catch (Exception ex)
            {
                if (!errorPreferenciasNotificado)
                {
                    OperacionUI.ReportarError(this, ex, "No se pudieron cargar las preferencias",
                        "Se usarán temporalmente las opciones predeterminadas y se conservarán los datos almacenados.", true);
                    errorPreferenciasNotificado = true;
                }
                return new PreferenciasUsuario();
            }
        }

        private void Label8_Click(object sender, EventArgs e)
        {

        }

        private void BtnTopCerrar_Click(object sender, EventArgs e)
        {
            // The window close button hides the UI but keeps supervisors and
            // tray actions alive, even when there are no active tasks.
            MinimizarEnBandeja();
        }
    }
}
