using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaNegocio;
using CapaEntidades;

namespace CapaPresentacion
{
    public partial class Form1 : Form
    {
        private readonly N_Usuario usuarios = new N_Usuario();
        private readonly N_VerificacionCorreo verificacion = new N_VerificacionCorreo();

        public Form1()
        {
            InitializeComponent();
        }

        private void btnEntrar_Click(object sender, EventArgs e)
        {
            E_Usuario usuario = null;
            if (!OperacionUI.Ejecutar(this, () => usuario = usuarios.IniciarSesion(correo.Text, clave.Text)))
                return;
            clave.Clear();
            Hide();
            try
            {
                using (var principal = new PrincipalForm(usuario))
                    principal.ShowDialog();
            }
            finally
            {
                usuario = null;
                clave.Clear();
                Show();
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
            pestanas.SelectedTab = tabVerificacion;
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
            pestanas.SelectedTab = tabAcceso;
            MessageBox.Show(this, "Correo verificado. Ya puede iniciar sesión.", "Verificación");
            clave.Focus();
        }

        private void btnVerificarCorreo_Click(object sender, EventArgs e)
        {
            correoVerificacion.Text = correo.Text.Trim();
            pestanas.SelectedTab = tabVerificacion;
            correoVerificacion.Focus();
        }

        private void pestanas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pestanas.SelectedTab == tabAcceso) AcceptButton = btnEntrar;
            else if (pestanas.SelectedTab == tabRegistro) AcceptButton = btnRegistrar;
            else AcceptButton = btnConfirmarCodigo;
        }
    }
}
