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
            var tabs = new TabControl { Dock = DockStyle.Fill };
            var acceso = new TabPage("Iniciar sesión");
            var registro = new TabPage("Registro");
            var login = PanelFormulario(acceso);
            Campo(login, "Correo electrónico", correo);
            Campo(login, "Contraseña", clave);
            var entrar = new Button { Text = "Iniciar sesión", AutoSize = true };
            entrar.Click += (s, e) => Entrar();
            login.Controls.Add(entrar);
            login.Controls.Add(new Label { Text = "Las cuentas nuevas requieren verificación por el administrador.", Width = 380, Height = 60 });
            var alta = PanelFormulario(registro);
            var nombre = new TextBox { Width = 330, MaxLength = 50 };
            var email = new TextBox { Width = 330, MaxLength = 254 };
            var password = new TextBox { Width = 330, MaxLength = 128, UseSystemPasswordChar = true };
            var confirmar = new TextBox { Width = 330, MaxLength = 128, UseSystemPasswordChar = true };
            Campo(alta, "Nombre", nombre);
            Campo(alta, "Correo electrónico", email);
            Campo(alta, "Contraseña (8–128 caracteres)", password);
            Campo(alta, "Confirmar contraseña", confirmar);
            var registrar = new Button { Text = "Registrar usuario", AutoSize = true };
            registrar.Click += (s, e) => {
                if (!OperacionUI.Ejecutar(this, () => {
                    if (password.Text != confirmar.Text) throw new ArgumentException("Las contraseñas no coinciden.");
                    usuarios.RegistrarUsuario(nombre.Text, email.Text, password.Text);
                })) return;
                correo.Text = email.Text.Trim();
                password.Clear(); confirmar.Clear(); clave.Clear();
                tabs.SelectedTab = acceso;
                MessageBox.Show(this, "Registro guardado. Solicite al administrador verificar el correo antes de iniciar sesión.", "Registro");
            };
            alta.Controls.Add(registrar);
            tabs.TabPages.Add(acceso); tabs.TabPages.Add(registro);
            Controls.Add(tabs);
        }

        private void Entrar()
        {
            CapaEntidades.E_Usuario usuario = null;
            if (!OperacionUI.Ejecutar(this, () => usuario = usuarios.IniciarSesion(correo.Text, clave.Text))) return;
            clave.Clear();
            Hide();
            try { using (var principal = new PrincipalForm(usuario)) principal.ShowDialog(); }
            finally { usuario = null; clave.Clear(); Show(); correo.Focus(); }
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
