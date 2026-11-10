using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using CapaEntidades;
using CapaNegocio;

namespace CapaPresentacion
{
    public sealed partial class PrincipalForm : Form
    {
        private readonly E_Usuario usuario;
        private frmAviso avisoBloqueo;
        private frmDescanso formularioDescanso;

        private readonly N_Tarea tareas = new N_Tarea();
        private readonly N_BloqueoApp bloqueador =
            N_BloqueoApp.Instancia;
        private readonly N_BloqueoWeb bloqueadorWeb =
            N_BloqueoWeb.Instancia;
        private readonly DataGridView listado = new DataGridView {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
            AllowUserToDeleteRows = false, AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        private DateTime fechaActual = DateTime.Today;
        private List<ucDia> diasCalendario = new List<ucDia>();
        private readonly TextBox filtro = new TextBox { Width = 180, MaxLength = 70 };
        private ucDia diaSeleccionado = null;

        public PrincipalForm(E_Usuario usuario)
        {
            InitializeComponent();
            Tipografia.Aplicar(this);
            ArrastreFormulario.Habilitar(this, pnlTop);

            FormClosing += (s, e) =>
            {
                bloqueadorWeb.ErrorAplicandoBloqueo -= BloqueadorWeb_ErrorAplicandoBloqueo;
                bloqueadorWeb.Detener();
            };

            this.usuario = usuario;
            bloqueador.AplicacionBloqueada +=
                Bloqueador_AplicacionBloqueada;
            bloqueador.DescansoIniciado +=
                Bloqueador_DescansoIniciado;
            bloqueadorWeb.ErrorAplicandoBloqueo += BloqueadorWeb_ErrorAplicandoBloqueo;
            bloqueador.Iniciar(usuario.IdUsuario);
            bloqueadorWeb.Iniciar(usuario.IdUsuario);

            btnCerrarSesion.Click += BtnCerrarSesion_Click;
            
            CargarCalendario();

        }

        private void CargarCalendario()
        {
            string mesAnio = fechaActual.ToString(
                "MMMM yyyy",
                new System.Globalization.CultureInfo("es-ES")
            );

            lblMesAnio.Text = char.ToUpper(mesAnio[0]) + mesAnio.Substring(1);

            tlpCalendario.Controls.Clear();
            diasCalendario.Clear();

            DateTime primerDia = new DateTime(
                fechaActual.Year,
                fechaActual.Month,
                1
            );
            List<E_Tarea> todasLasTareas =
    tareas.BuscarTarea(usuario.IdUsuario, "");


            // Ajustar para que la semana comience en lunes
            int desplazamiento = ((int)primerDia.DayOfWeek + 6) % 7;

            // Fecha del primer espacio del calendario
            DateTime fechaInicio = primerDia.AddDays(-desplazamiento);

            for (int i = 0; i < 42; i++)
            {
                DateTime fecha = fechaInicio.AddDays(i);

                ucDia dia = new ucDia();

                dia.Dock = DockStyle.Fill;
                dia.ConfigurarDia(
                    fecha,
                    fecha.Month == fechaActual.Month
                );
                var tareasDelDia = todasLasTareas
                        .Where(t => t.FechaInicio.Date <= fecha.Date &&
                                    t.FechaFin.Date >= fecha.Date)
                        .OrderBy(t => t.FechaInicio.TimeOfDay)
                        .ThenBy(t => t.Nombre);

                dia.CargarTareas(tareasDelDia);
                dia.SeleccionSolicitada += (s, e) => SeleccionarDia(dia);

                diasCalendario.Add(dia);

                int fila = i / 7;
                int columna = i % 7;

                tlpCalendario.Controls.Add(dia, columna, fila);
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
            // CargarCalendario crea controles ucDia nuevos.
            diaSeleccionado = null;
            CargarCalendario();

            ucDia diaActualizado = diasCalendario
                .FirstOrDefault(dia => dia.Fecha.Date == fecha.Date);

            if (diaActualizado != null)
                SeleccionarDia(diaActualizado);
        }

        private void CargarTareasDelDia(DateTime fecha)
        {
            flTareas.Controls.Clear();

            List<E_Tarea> resultado = tareas.BuscarTarea(usuario.IdUsuario, "");
            List<E_Tarea> tareasDelDia = resultado
                .Where(t => t.FechaInicio.Date <= fecha.Date &&
                            t.FechaFin.Date >= fecha.Date)
                .OrderBy(t => t.FechaInicio)
                .ToList();

            foreach (E_Tarea tarea in tareasDelDia)
            {
                var control = new ucTareas();
                control.CargarTarea(tarea);
                control.EliminarSolicitado += Control_EliminarSolicitado;
                flTareas.Controls.Add(control);
            }
        }

        private void SeleccionarDia(ucDia dia)
        {
            if (diaSeleccionado != null)
                diaSeleccionado.Seleccionar(false);

            diaSeleccionado = dia;
            diaSeleccionado.Seleccionar(true);

            CargarTareasDelDia(dia.Fecha);
        }
        private void MesAnterior()
        {
            fechaActual = fechaActual.AddMonths(-1);
            CargarCalendario();
        }

        private void MesSiguiente()
        {
            fechaActual = fechaActual.AddMonths(1);
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

            if (avisoBloqueo == null || avisoBloqueo.IsDisposed)
            {
                avisoBloqueo = new frmAviso();

                avisoBloqueo.FormClosed += (s, args) =>
                {
                    avisoBloqueo = null;
                };
            }

            avisoBloqueo.ConfigurarContexto(
                e.NombreAplicacion,
                e.NombreTarea);

            avisoBloqueo.TopMost = true;

            if (!avisoBloqueo.Visible)
                avisoBloqueo.Show();
        }

        private void BtnCerrarSesion_Click(object sender, EventArgs e)
        {
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

            if (avisoBloqueo != null && !avisoBloqueo.IsDisposed)
                avisoBloqueo.Close();

            if (formularioDescanso == null ||
                formularioDescanso.IsDisposed)
            {
                formularioDescanso = new frmDescanso();

                formularioDescanso.FormClosed += (s, args) =>
                {
                    formularioDescanso = null;
                };
            }

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
    }
}
