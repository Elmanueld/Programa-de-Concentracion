using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{
    public partial class Form1 : Form
    {
        private readonly CapaNegocio.N_Usuario usuarios = new CapaNegocio.N_Usuario();
        private readonly TextBox correo = new TextBox { Width = 330, MaxLength = 254 };
        private readonly TextBox clave = new TextBox { Width = 330, MaxLength = 128, UseSystemPasswordChar = true };

        public Form1()
        {
            InitializeComponent();
            Text = "Programa de Concentración — Acceso";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(510, 520);
            Size = new Size(550, 580);
            Font = new Font("Segoe UI", 10);
            var pestanas = new TabControl { Dock = DockStyle.Fill };
            var acceso = new TabPage("Iniciar sesión");
            var registro = new TabPage("Registro");
            var panelAcceso = PanelFormulario(acceso);
            Campo(panelAcceso, "Correo electrónico", correo);
            Campo(panelAcceso, "Contraseña", clave);
            var entrar = new Button { Text = "Iniciar sesión", AutoSize = true };
            entrar.Click += (s, e) => Entrar();
            panelAcceso.Controls.Add(entrar);
            panelAcceso.Controls.Add(new Label { Text = "Las cuentas nuevas requieren verificación por el administrador.", Width = 380, Height = 60 });
            var alta = PanelFormulario(registro);
            var nombre = new TextBox { Width = 330, MaxLength = 50 };
            var correoRegistro = new TextBox { Width = 330, MaxLength = 254 };
            var contrasenaRegistro = new TextBox { Width = 330, MaxLength = 128, UseSystemPasswordChar = true };
            var confirmar = new TextBox { Width = 330, MaxLength = 128, UseSystemPasswordChar = true };
            Campo(alta, "Nombre", nombre);
            Campo(alta, "Correo electrónico", correoRegistro);
            Campo(alta, "Contraseña (8–128 caracteres)", contrasenaRegistro);
            Campo(alta, "Confirmar contraseña", confirmar);
            var registrar = new Button { Text = "Registrar usuario", AutoSize = true };
            registrar.Click += (s, e) =>
            {
                if (!OperacionUI.Ejecutar(this, () =>
                {
                    if (contrasenaRegistro.Text != confirmar.Text)
                        throw new ArgumentException("Las contraseñas no coinciden.");
                    usuarios.RegistrarUsuario(nombre.Text, correoRegistro.Text, contrasenaRegistro.Text);
                }))
                    return;
                correo.Text = correoRegistro.Text.Trim();
                contrasenaRegistro.Clear();
                confirmar.Clear();
                clave.Clear();
                pestanas.SelectedTab = acceso;
                MessageBox.Show(this, "Registro guardado. Solicite al administrador verificar el correo antes de iniciar sesión.", "Registro");
            };
            alta.Controls.Add(registrar);
            pestanas.TabPages.Add(acceso);
            pestanas.TabPages.Add(registro);
            Controls.Add(pestanas);
        }

        private void Entrar()
        {
            CapaEntidades.E_Usuario usuario = null;
            if (!OperacionUI.Ejecutar(this, () => usuario = usuarios.IniciarSesion(correo.Text, clave.Text))) return;
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

        private static FlowLayoutPanel PanelFormulario(Control padre)
        {
            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            padre.Controls.Add(panel);
            return panel;
        }

        private static void Campo(Control padre, string titulo, Control entrada)
        {
            padre.Controls.Add(new Label { Text = titulo, AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
            padre.Controls.Add(entrada);
        }
    }
}
