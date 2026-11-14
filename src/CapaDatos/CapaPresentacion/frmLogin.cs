using System;
using System.Drawing;
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
        private E_Usuario sesionAutomatica;

        public frmLogin() : this(null)
        {
        }

        public frmLogin(E_Usuario sesionAutomatica)
        {
            InitializeComponent();
            if (EnModoDiseno) return;
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
                pestanas.TabPages.Remove(tabVerificacion);
                lblRegistro.Text = "Modo demostración: cree la cuenta y entre con su contraseña. No se enviará correo.";
                lblAcceso.Text = "Modo demostración: acceso con contraseña, sin verificación de correo.";
                ActualizarVista(tabAcceso, false);
            }
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
            if (!OperacionUI.Ejecutar(this, () => usuario = usuarios.IniciarSesion(correo.Text, clave.Text)))
                return;
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
                estadoVerificacion.Text = "Verificación pendiente. El envío de correo aún no está configurado. Si acaba de registrarse, su cuenta ya fue guardada.";
                return;
            }
            string destino = correoVerificacion.Text;
            bool enviado = await OperacionUI.EjecutarAsync(this,
                () => Task.Run(() => verificacion.SolicitarCodigo(destino)));
            estadoVerificacion.Text = enviado
                ? "Si la cuenta existe y está pendiente, recibirá un código. Revise también el correo no deseado."
                : "La cuenta sigue pendiente. Revise el mensaje e intente solicitar otro código cuando corresponda.";
        }

        private void btnConfirmarCodigo_Click(object sender, EventArgs e)
        {
            if (!OperacionUI.Ejecutar(this,
                () => verificacion.ConfirmarCodigo(correoVerificacion.Text, codigoVerificacion.Text)))
                return;
            correo.Text = correoVerificacion.Text.Trim();
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
