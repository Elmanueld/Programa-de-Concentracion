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

        private readonly N_Tarea tareas;
        private readonly N_BloqueoApp bloqueador;
        private readonly N_BloqueoWeb bloqueadorWeb;
        private readonly DataGridView listado = new DataGridView {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
            AllowUserToDeleteRows = false, AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        private DateTime fechaActual = DateTime.Today;
        private List<ucDia> diasCalendario = new List<ucDia>();
        private List<E_Tarea> tareasCache;
        private readonly TextBox filtro = new TextBox { Width = 180, MaxLength = 70 };
        private ucDia diaSeleccionado = null;
        private readonly System.Windows.Forms.Timer timerSincronizacion;
        private readonly System.Windows.Forms.Timer timerAvisosTarea;
        private readonly HashSet<string> avisosTareaEmitidos = new HashSet<string>(StringComparer.Ordinal);
        private int sincronizacionEnCurso;
        private readonly N_SincronizacionOffline sincronizacion;

        public PrincipalForm() : this(null)
        {
        }

        public PrincipalForm(E_Usuario usuario)
        {
            InitializeComponent();
            if (EnModoDiseno) return;
            if (usuario == null) throw new ArgumentNullException("usuario");
            iconoNotificacion = new NotifyIcon
            {
                Icon = SystemIcons.Information,
                Visible = false,
                Text = "Programa de Concentración"
            };
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
            ControlesPresentacion.Optimizar(flTareas);
            ArrastreFormulario.Habilitar(this, pnlTop);

            FormClosing += (s, e) =>
            {
                bloqueadorWeb.ErrorAplicandoBloqueo -= BloqueadorWeb_ErrorAplicandoBloqueo;
                bloqueadorWeb.Detener();
                bloqueador.Detener();
                timerSincronizacion.Stop();
                timerSincronizacion.Dispose();
                timerAvisosTarea.Stop();
                timerAvisosTarea.Dispose();
                PreferenciasUsuario prefsAlCerrar = ObtenerPreferencias();
                if (String.Equals(prefsAlCerrar.PantallaInicio, "Ultima", StringComparison.OrdinalIgnoreCase) && diaSeleccionado != null)
                {
                    prefsAlCerrar.UltimaFechaSeleccionada = diaSeleccionado.Fecha.Date;
                    try { PreferenciasUsuarioApp.Guardar(usuario.IdUsuario, prefsAlCerrar); } catch { }
                }
                iconoNotificacion.Visible = false;
                iconoNotificacion.Dispose();
                if (fotoPerfilActual != null)
                {
                    pbFotoDePerfil.Image = null;
                    fotoPerfilActual.Dispose();
                    fotoPerfilActual = null;
                }
            };

            this.usuario = usuario;
            timerSincronizacion.Tick += TimerSincronizacion_Tick;
            timerSincronizacion.Start();
            aparienciaUsuario = AparienciaUsuarioApp.Cargar(usuario.IdUsuario);
            PaletaAplicacion.Aplicar(aparienciaUsuario);
            PreferenciasUsuario preferenciasIniciales = ObtenerPreferencias();
            if (String.Equals(preferenciasIniciales.PantallaInicio, "Ultima", StringComparison.OrdinalIgnoreCase) &&
                preferenciasIniciales.UltimaFechaSeleccionada != DateTime.MinValue)
                fechaActual = preferenciasIniciales.UltimaFechaSeleccionada;
            timerAvisosTarea.Tick += TimerAvisosTarea_Tick;
            timerAvisosTarea.Start();
            TemaAplicacion.EstablecerModoOscuro(preferenciasIniciales.ModoOscuro);
            bloqueador.AplicacionBloqueada +=
                Bloqueador_AplicacionBloqueada;
            bloqueador.DescansoIniciado +=
                Bloqueador_DescansoIniciado;
            bloqueadorWeb.ErrorAplicandoBloqueo += BloqueadorWeb_ErrorAplicandoBloqueo;
            bloqueador.Iniciar(usuario.IdUsuario);
            bloqueadorWeb.Iniciar(usuario.IdUsuario);

            btnTopConfiguracion.Click += BtnConfiguracion_Click;
            btnDetener.Click += BtnDetener_Click;
            Shown += PrincipalForm_Shown;

            CargarCalendario();
            DateTime diaInicio = String.Equals(preferenciasIniciales.PantallaInicio, "Ultima", StringComparison.OrdinalIgnoreCase)
                && preferenciasIniciales.UltimaFechaSeleccionada != DateTime.MinValue
                ? preferenciasIniciales.UltimaFechaSeleccionada.Date : DateTime.Today;
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
                    Close();
                    return;
                }
                ActualizarPerfil(ObtenerPreferencias());
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
            lblBienvenida.Text = "Bienvenido, " + (usuario.Nombre ?? String.Empty);
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
            PreferenciasUsuario preferencias = ObtenerPreferencias();
            if (String.Equals((preferencias.PantallaInicio ?? String.Empty).Trim(),
                "Tarea", StringComparison.OrdinalIgnoreCase))
            {
                BeginInvoke(new Action(() =>
                {
                    using (var formulario = new frmTareas(usuario.IdUsuario, DateTime.Today))
                        NavegacionFormularios.MostrarModal(this, formulario);
                }));
            }
            else if (Program.InicioDesdeWindows && preferencias.IniciarConWindows && preferencias.IniciarMinimizado)
            {
                WindowState = FormWindowState.Minimized;
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

            DateTime ahora = DateTime.Now;
            foreach (E_Tarea tarea in tareasCache.Where(t => t != null && t.Estado == 'P' && !t.EsPlantilla))
            {
                DateTime inicio = tarea.FechaInicio;
                DateTime avisoDesde = inicio.AddMinutes(-minutos);
                if (ahora < avisoDesde || ahora >= inicio) continue;
                string clave = tarea.IdTarea + "|" + inicio.Ticks;
                if (!avisosTareaEmitidos.Add(clave)) continue;

                if (preferencias.MostrarComoNotificacion)
                    iconoNotificacion.ShowBalloonTip(6000, "Tarea próxima",
                        "\"" + tarea.Nombre + "\" empieza a las " + inicio.ToString("HH:mm") + ".", ToolTipIcon.Info);
                if (!preferencias.MostrarComoVentana) continue;
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

        private void ProcesarEstadosDeTarea()
        {
            if (tareasCache == null) return;
            DateTime ahora = DateTime.Now;
            bool huboCambios = false;
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
                    }
                    if (tarea.Estado == 'A' && ahora >= tarea.FechaFin)
                    {
                        tareas.CambiarEstadoTarea(usuario.IdUsuario, tarea.IdTarea, 'C');
                        tarea.Estado = 'C';
                        huboCambios = true;
                    }
                }
                catch
                {
                    // El ciclo siguiente reintenta si la base está temporalmente inaccesible.
                }
            }

            if (!huboCambios) return;
            DateTime fecha = diaSeleccionado == null ? DateTime.Today : diaSeleccionado.Fecha;
            try
            {
                tareasCache = tareas.BuscarTarea(usuario.IdUsuario, String.Empty) ?? new List<E_Tarea>();
                ActualizarCalendarioYDia(fecha);
                bloqueador.ActualizarProgramacion();
                bloqueadorWeb.ActualizarProgramacion();
            }
            catch
            {
                // La actualización visual se reintentará en el siguiente ciclo.
            }
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
                MessageBox.Show(
                    this,
                    ex.Message,
                    "No se pudo borrar la tarea",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
                if (!sincronizada || IsDisposed || Disposing) return;
                DateTime fecha = diaSeleccionado == null ? DateTime.Today : diaSeleccionado.Fecha;
                tareasCache = null;
                ActualizarCalendarioYDia(fecha);
                bloqueador.ActualizarProgramacion();
                bloqueadorWeb.ActualizarProgramacion();
            }
            catch
            {
                // Fallos transitorios se guardan en SyncOutbox y se reintentan
                // en el siguiente ciclo, sin bloquear la pantalla de tareas.
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

        private E_Tarea ObtenerTareaActual()
        {
            if (tareasCache == null) return null;
            DateTime ahora = DateTime.Now;
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
                ActualizarCalendarioYDia(diaSeleccionado == null ? DateTime.Today : diaSeleccionado.Fecha);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "No se pudo detener la tarea",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show(this, ex.Message, "No se pudo editar la tarea",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                try { PreferenciasUsuarioApp.Guardar(usuario.IdUsuario, preferencias); }
                catch { /* La selección del calendario sigue funcionando aunque falle el guardado local. */ }
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
                iconoNotificacion.ShowBalloonTip(5000, "Página o aplicación bloqueada",
                    "" + e.NombreAplicacion + " está bloqueada durante " + e.NombreTarea + ".", ToolTipIcon.Info);
            if (!preferencias.MostrarComoVentana) return;

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
                MessageBox.Show(this, "No se pudo actualizar el inicio automático: " + ex.Message,
                    "Cerrar sesión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            DialogResult = DialogResult.Retry;
            Close();
        }

        private void BloqueadorWeb_ErrorAplicandoBloqueo(string mensaje)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (!IsDisposed)
                        MessageBox.Show(this, mensaje, "No se pudo aplicar el bloqueo web", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
            catch (InvalidOperationException) { }
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
                        () => MostrarAvisoDescanso(e)));
                }
                catch (InvalidOperationException)
                {
                    // El formulario pudo cerrarse antes de procesar el aviso.
                }

                return;
            }

            MostrarAvisoDescanso(e);
        }

        private void MostrarAvisoDescanso(
            DescansoIniciadoEventArgs e)
        {
            if (IsDisposed)
                return;

            PreferenciasUsuario preferencias = ObtenerPreferencias();
            if (preferencias.NoMostrarAnuncios || preferencias.NoMostrarDescansos) return;
            if (preferencias.MostrarComoNotificacion)
                iconoNotificacion.ShowBalloonTip(5000, "Hora de descansar",
                    "Comenzó el descanso de " + e.NombreTarea + ".", ToolTipIcon.Info);
            if (!preferencias.MostrarComoVentana) return;

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
            this.WindowState = FormWindowState.Minimized;
        }

        private PreferenciasUsuario ObtenerPreferencias()
        {
            try { return PreferenciasUsuarioApp.Cargar(usuario.IdUsuario); }
            catch { return new PreferenciasUsuario(); }
        }

        private void Label8_Click(object sender, EventArgs e)
        {

        }

        private void BtnTopCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
