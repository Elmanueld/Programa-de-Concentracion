using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaNegocio;

namespace CapaPresentacion
{
    public sealed class frmRecuperarContrasena : Form
    {
        private readonly N_RecuperacionContrasena recuperacion = new N_RecuperacionContrasena();
        private readonly TextBox correo = new TextBox();
        private readonly TextBox codigo = new TextBox();
        private readonly TextBox nuevaContrasena = new TextBox();
        private readonly TextBox confirmarContrasena = new TextBox();
        private readonly Label estado = new Label();
        private readonly Button enviarCodigo = new Button();
        private readonly Button restablecer = new Button();

        public frmRecuperarContrasena(string correoInicial)
        {
            Text = "Recuperar contraseña";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(480, 500);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);

            var titulo = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Location = new Point(28, 20),
                Text = "Restablecer contraseña"
            };
            Controls.Add(titulo);

            AgregarEtiqueta("Correo electrónico", 66);
            ConfigurarEntrada(correo, 88, 254);
            correo.Text = (correoInicial ?? String.Empty).Trim();

            enviarCodigo.Text = "Enviar código por correo";
            enviarCodigo.Location = new Point(28, 126);
            enviarCodigo.Size = new Size(424, 36);
            enviarCodigo.Click += EnviarCodigo_Click;
            Controls.Add(enviarCodigo);

            AgregarEtiqueta("Código de recuperación", 174);
            ConfigurarEntrada(codigo, 196, 6);

            AgregarEtiqueta("Nueva contraseña", 236);
            ConfigurarEntrada(nuevaContrasena, 258, 128);
            nuevaContrasena.UseSystemPasswordChar = true;

            AgregarEtiqueta("Confirma la nueva contraseña", 298);
            ConfigurarEntrada(confirmarContrasena, 320, 128);
            confirmarContrasena.UseSystemPasswordChar = true;

            estado.AutoSize = false;
            estado.Location = new Point(28, 360);
            estado.Size = new Size(424, 46);
            estado.ForeColor = Color.FromArgb(90, 100, 120);
            estado.Text = "Si la cuenta está registrada y verificada, recibirás un código.";
            Controls.Add(estado);

            restablecer.Text = "Restablecer contraseña";
            restablecer.Location = new Point(28, 418);
            restablecer.Size = new Size(270, 38);
            restablecer.Click += Restablecer_Click;
            Controls.Add(restablecer);

            var cancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(308, 418),
                Size = new Size(144, 38),
                DialogResult = DialogResult.Cancel
            };
            Controls.Add(cancelar);

            AcceptButton = restablecer;
            CancelButton = cancelar;
            AjustePantalla.Preparar(this);
            FormClosed += (sender, args) =>
            {
                nuevaContrasena.Clear();
                confirmarContrasena.Clear();
                codigo.Clear();
            };
        }

        private void AgregarEtiqueta(string texto, int y)
        {
            Controls.Add(new Label
            {
                AutoSize = true,
                Location = new Point(28, y),
                Text = texto
            });
        }

        private void ConfigurarEntrada(TextBox entrada, int y, int maximo)
        {
            entrada.Location = new Point(28, y);
            entrada.Size = new Size(424, 25);
            entrada.MaxLength = maximo;
            entrada.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            Controls.Add(entrada);
        }

        private async void EnviarCodigo_Click(object sender, EventArgs e)
        {
            await OperacionUI.EjecutarEventoAsync(this, async () =>
            {
                string destino = correo.Text.Trim();
                if (!recuperacion.EstaConfigurado())
                {
                    estado.Text = "El restablecimiento de contraseña no está disponible ahora. Inténtalo más tarde.";
                    return;
                }

                Exception errorSolicitud = null;
                bool completado = await OperacionUI.EjecutarAsync(this,
                    () => Task.Run(() => errorSolicitud = recuperacion.SolicitarCodigo(destino)));
                if (!completado) return;
                if (errorSolicitud != null)
                {
                    estado.Text = OperacionUI.DescribirError(errorSolicitud);
                    OperacionUI.ReportarError(this, errorSolicitud, "No se pudo enviar el código");
                    return;
                }
                estado.Text = "Si la cuenta está registrada y verificada, recibirás un código. Revisa también el correo no deseado.";
            }, "No se pudo enviar el código");
        }

        private async void Restablecer_Click(object sender, EventArgs e)
        {
            await OperacionUI.EjecutarEventoAsync(this, async () =>
            {
                if (!String.Equals(nuevaContrasena.Text, confirmarContrasena.Text, StringComparison.Ordinal))
                {
                    MessageBox.Show(this, "Las contraseñas no coinciden.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    confirmarContrasena.Focus();
                    confirmarContrasena.SelectAll();
                    return;
                }

                string destino = correo.Text.Trim();
                string codigoIngresado = codigo.Text;
                string contrasenaIngresada = nuevaContrasena.Text;
                Exception errorRestablecimiento = null;
                bool completado = await OperacionUI.EjecutarAsync(this, () => Task.Run(() =>
                    errorRestablecimiento = recuperacion.RestablecerContrasena(destino, codigoIngresado, contrasenaIngresada)));

                if (!completado) return;
                if (errorRestablecimiento != null)
                {
                    estado.Text = OperacionUI.DescribirError(errorRestablecimiento);
                    OperacionUI.ReportarError(this, errorRestablecimiento, "No se pudo restablecer la contraseña");
                    return;
                }
                nuevaContrasena.Clear();
                confirmarContrasena.Clear();
                codigo.Clear();
                MessageBox.Show(this, "La contraseña se restableció. Ya puedes iniciar sesión.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }, "No se pudo restablecer la contraseña");
        }
    }
}
