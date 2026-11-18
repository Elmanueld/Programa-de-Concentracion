using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaNegocio;
using CapaEntidades;

namespace CapaPresentacion
{
    public partial class frmLogin : FormularioSuave
    {
        private readonly N_Usuario usuarios = new N_Usuario();
        private readonly N_VerificacionCorreo verificacion = new N_VerificacionCorreo();
        private System.Windows.Forms.Timer timerRegistroPendiente;
        private int reintentoRegistroEnCurso;
        private bool avisoRegistroPendienteMostrado;
        private E_Usuario sesionAutomatica;

        public frmLogin() : this(null)
        {
        }

        public frmLogin(E_Usuario sesionAutomatica)
        {
            InitializeComponent();
            if (EnModoDiseno) return;
            var enlaceRecuperacion = new LinkLabel
            {
                AutoSize = false,
                Font = new Font("Inter", 9F),
                LinkColor = Color.FromArgb(71, 104, 180),
                Location = new Point(34, 180),
                Name = "enlaceRecuperacion",
                Size = new Size(460, 22),
                Text = "¿Olvidaste tu contraseña?",
                TextAlign = ContentAlignment.MiddleRight
            };
            enlaceRecuperacion.LinkClicked += EnlaceRecuperacion_LinkClicked;
            pnlPaginaAcceso.Controls.Add(enlaceRecuperacion);
            enlaceRecuperacion.BringToFront();
            timerRegistroPendiente = new System.Windows.Forms.Timer { Interval = 60000 };
            timerRegistroPendiente.Tick += TimerRegistroPendiente_Tick;
            FormClosed += (s, e) => timerRegistroPendiente.Dispose();
            this.sesionAutomatica = sesionAutomatica;
            if (sesionAutomatica != null)
            {
                // La sesión automática abre PrincipalForm inmediatamente; el
                // formulario de acceso solo se muestra si luego se cierra sesión.
                Opacity = 0;
                ShowInTaskbar = false;
            }
            PaletaAplicacion.Aplicar(PreferenciasApariencia.PorDefecto);
            TemaAplicacion.Preparar(this);
            TemaAplicacion.Aplicar(this, false);
            Shown += FrmLogin_Shown;
            ActualizarVista(tabAcceso, false);
            if (N_Usuario.ModoDemostracion)
            {
                enlaceRecuperacion.Visible = false;
                pestanas.TabPages.Remove(tabVerificacion);
                lblRegistro.Text = "Modo demostración: cree la cuenta y entre con su contraseña. No se enviará correo.";
                lblAcceso.Text = "Modo demostración: acceso con contraseña, sin verificación de correo.";
                ActualizarVista(tabAcceso, false);
            }
        }

        internal void MostrarAvisoInicial(string mensaje)
        {
            lblAcceso.Text = mensaje;
        }

        internal void PrepararParaMostrar()
        {
            Opacity = 1;
            ShowInTaskbar = true;
        }

        private void FrmLogin_Shown(object sender, EventArgs e)
        {
            if (sesionAutomatica == null) return;
            E_Usuario usuario = sesionAutomatica;
            sesionAutomatica = null;
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed) return;
                try
                {
                    AparienciaUsuarioApp.Aplicar(usuario.IdUsuario);
                    using (var principal = new PrincipalForm(usuario))
                        NavegacionFormularios.MostrarModal(this, principal);
                }
                finally { usuario = null; }
            }));
        }

        private void btnEntrar_Click(object sender, EventArgs e)
        {
            E_Usuario usuario = null;
            if (N_Usuario.ConexionRemotaConfigurada)
            {
                if (!OperacionUI.Ejecutar(this, () => usuario = usuarios.IniciarSesion(correo.Text, clave.Text)))
                    return;
            }
            else
            {
                if (!OperacionUI.Ejecutar(this, () => usuario = usuarios.IniciarSesionOffline(correo.Text, clave.Text)))
                    return;
                if (usuario == null)
                {
                    MessageBox.Show(this,
                        "No se pudo conectar con tu cuenta en línea y este equipo no tiene información guardada para entrar sin conexión. Comprueba Internet e inténtalo de nuevo.",
                        "No se pudo iniciar sesión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            if (usuarios.AdvertenciaCacheLocal != null)
                OperacionUI.ReportarError(this, usuarios.AdvertenciaCacheLocal, "Inicio de sesión completado con aviso",
                    "La cuenta se abrió correctamente, pero no se actualizó toda la información guardada en este equipo. Inicia sesión con Internet para volver a prepararla.", true);
            AparienciaUsuarioApp.Aplicar(usuario.IdUsuario);
            clave.Clear();
            try
            {
                using (var principal = new PrincipalForm(usuario))
                    NavegacionFormularios.MostrarModal(this, principal);
            }
            finally
            {
                usuario = null;
                clave.Clear();
                correo.Focus();
            }
        }

        private void EnlaceRecuperacion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (var formulario = new frmRecuperarContrasena(correo.Text))
                formulario.ShowDialog(this);
        }

        private async void btnRegistrar_Click(object sender, EventArgs e)
        {
            if (!OperacionUI.Ejecutar(this, () =>
            {
                if (contrasenaRegistro.Text != confirmar.Text)
                    throw new ArgumentException("Las contraseñas no coinciden.");
                usuarios.RegistrarUsuario(nombre.Text, correoRegistro.Text, contrasenaRegistro.Text);
            }))
                return;
            correo.Text = correoRegistro.Text.Trim();
            correoVerificacion.Text = correo.Text;
            contrasenaRegistro.Clear();
            confirmar.Clear();
            clave.Clear();
            codigoVerificacion.Clear();
            if (N_Usuario.ModoDemostracion)
            {
                ActualizarVista(tabAcceso, true);
                lblAcceso.Text = "Cuenta creada. Ya puede iniciar sesión con su contraseña (modo demostración).";
                clave.Focus();
                return;
            }
            ActualizarVista(tabVerificacion, true);
            estadoVerificacion.Text = "Cuenta creada. Enviando el código al correo indicado...";
            await SolicitarCodigo();
        }

        private async void btnEnviarCodigo_Click(object sender, EventArgs e)
        {
            await SolicitarCodigo();
        }

        private async Task SolicitarCodigo()
        {
            if (!verificacion.EstaConfigurado())
            {
                estadoVerificacion.Text = "Tu cuenta quedó guardada, pero todavía no se pudo enviar el correo de verificación. Inténtalo más tarde.";
                return;
            }
            string destino = correoVerificacion.Text;
            Exception errorSolicitud = null;
            bool enviado = await OperacionUI.EjecutarAsync(this,
                () => Task.Run(() => errorSolicitud = verificacion.SolicitarCodigo(destino)));
            if (!enviado) return;
            if (errorSolicitud != null)
            {
                bool reintentable = errorSolicitud is RegistroRemotoPendienteException;
                if (reintentable) timerRegistroPendiente.Start();
                else timerRegistroPendiente.Stop();
                estadoVerificacion.Text = reintentable
                    ? "Tu cuenta quedó guardada en este equipo. Se intentará completar el registro cuando vuelva la conexión."
                    : "No se pudo completar el envío. Revisa el mensaje para corregirlo o inténtalo de nuevo.";
                if (!reintentable || !avisoRegistroPendienteMostrado)
                {
                    OperacionUI.ReportarError(this, errorSolicitud, "No se pudo enviar el código",
                        reintentable
                            ? "La cuenta quedó guardada en este equipo. Se intentará completar el registro cuando vuelva la conexión."
                            : null,
                        reintentable);
                    avisoRegistroPendienteMostrado = reintentable;
                }
                return;
            }
            timerRegistroPendiente.Stop();
            avisoRegistroPendienteMostrado = false;
            estadoVerificacion.Text = "Si la cuenta existe y está pendiente, recibirá un código. Revise también el correo no deseado.";
        }

        private async void TimerRegistroPendiente_Tick(object sender, EventArgs e)
        {
            if (Interlocked.Exchange(ref reintentoRegistroEnCurso, 1) != 0) return;
            try
            {
                string destino = (correoVerificacion.Text ?? "").Trim();
                if (!usuarios.HayRegistroPendiente(destino))
                {
                    timerRegistroPendiente.Stop();
                    return;
                }

                Exception errorSolicitud = await Task.Run(() => verificacion.SolicitarCodigo(destino));
                if (errorSolicitud != null)
                {
                    if (errorSolicitud is RegistroRemotoPendienteException)
                    {
                        estadoVerificacion.Text = "Tu cuenta quedó guardada en este equipo. Se intentará completar el registro cuando vuelva la conexión.";
                        if (!avisoRegistroPendienteMostrado)
                        {
                            OperacionUI.ReportarError(this, errorSolicitud, "Registro pendiente",
                                "La cuenta quedó guardada en este equipo y se enviará cuando vuelva la conexión.", true);
                            avisoRegistroPendienteMostrado = true;
                        }
                        return;
                    }
                    timerRegistroPendiente.Stop();
                    OperacionUI.ReportarError(this, errorSolicitud, "No se pudo completar el registro");
                    estadoVerificacion.Text = "Tu cuenta quedó guardada en este equipo. Pulsa Enviar código para volver a intentarlo.";
                    return;
                }
                timerRegistroPendiente.Stop();
                avisoRegistroPendienteMostrado = false;
                estadoVerificacion.Text = "La solicitud se procesó. Revise el correo y la carpeta de correo no deseado.";
            }
            catch (Exception ex)
            {
                timerRegistroPendiente.Stop();
                if (!avisoRegistroPendienteMostrado)
                {
                    OperacionUI.ReportarError(this, ex, "No se pudo completar el registro");
                    avisoRegistroPendienteMostrado = true;
                }
                estadoVerificacion.Text = "Tu cuenta quedó guardada en este equipo. Pulsa Enviar código para volver a intentarlo.";
            }
            finally { Interlocked.Exchange(ref reintentoRegistroEnCurso, 0); }
        }

        private async void btnConfirmarCodigo_Click(object sender, EventArgs e)
        {
            string emailVerificacion = correoVerificacion.Text;
            string codigo = codigoVerificacion.Text;
            Exception errorConfirmacion = null;
            if (!await OperacionUI.EjecutarAsync(this,
                () => Task.Run(() => errorConfirmacion = verificacion.ConfirmarCodigo(emailVerificacion, codigo))))
                return;
            if (errorConfirmacion != null)
            {
                estadoVerificacion.Text = OperacionUI.DescribirError(errorConfirmacion);
                OperacionUI.ReportarError(this, errorConfirmacion, "No se pudo verificar el correo");
                return;
            }
            correo.Text = emailVerificacion.Trim();
            codigoVerificacion.Clear();
            clave.Clear();
            estadoVerificacion.Text = "Correo verificado.";
            ActualizarVista(tabAcceso, true);
            MessageBox.Show(this, "Correo verificado. Ya puede iniciar sesión.", "Verificación");
            clave.Focus();
        }

        private void btnNavAcceso_Click(object sender, EventArgs e)
        {
            ActualizarVista(tabAcceso, true);
        }

        private void btnNavRegistro_Click(object sender, EventArgs e)
        {
            ActualizarVista(tabRegistro, true);
        }

        private void pestanas_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarVista(pestanas.SelectedTab, false);
        }

        private void ActualizarVista(TabPage pagina, bool actualizarEstadoInterno)
        {
            if (pagina == null) pagina = tabAcceso;
            if (actualizarEstadoInterno && pestanas.SelectedTab != pagina && pestanas.TabPages.Contains(pagina))
                pestanas.SelectedTab = pagina;

            pnlPaginaAcceso.Visible = pagina == tabAcceso;
            pnlPaginaRegistro.Visible = pagina == tabRegistro;
            pnlPaginaVerificacion.Visible = pagina == tabVerificacion;
            ActualizarBotonesNavegacion(pagina);

            if (pagina == tabAcceso)
            {
                AcceptButton = btnEntrar;
                lblBienvenida.Text = "Bienvenido";
                lblDescripcion.Text = "Inicia sesión para continuar con tus tareas.";
            }
            else if (pagina == tabRegistro)
            {
                AcceptButton = btnRegistrar;
                lblBienvenida.Text = "Crea tu cuenta";
                lblDescripcion.Text = "Unos datos y estarás listo para organizarte.";
            }
            else
            {
                AcceptButton = btnConfirmarCodigo;
                lblBienvenida.Text = "Confirma tu correo";
                lblDescripcion.Text = "Escribe el código de verificación que recibiste por correo.";
            }
        }

        private void ActualizarBotonesNavegacion(TabPage pagina)
        {
            bool accesoActivo = pagina == tabAcceso;
            AplicarEstadoBotonNavegacion(btnNavAcceso, accesoActivo);
            AplicarEstadoBotonNavegacion(btnNavRegistro, !accesoActivo);
        }

        private static void AplicarEstadoBotonNavegacion(AntdUI.Button boton, bool activo)
        {
            if (boton == null) return;
            Color fondo = activo ? PaletaAplicacion.AzulPrimario : Color.FromArgb(242, 245, 251);
            Color texto = activo ? Color.White : Color.FromArgb(40, 52, 75);
            boton.BackColor = fondo;
            boton.DefaultBack = fondo;
            boton.ForeColor = texto;
            boton.ForeHover = texto;
            boton.ForeActive = texto;
            boton.BackHover = activo ? PaletaAplicacion.AzulHover : Color.FromArgb(232, 238, 249);
            boton.BackActive = activo ? PaletaAplicacion.AzulPrimario : Color.FromArgb(220, 230, 246);
            boton.DefaultBorderColor = activo ? PaletaAplicacion.AzulPrimario : Color.FromArgb(242, 245, 251);
        }

        private void BtnTopCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BtnTopMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
