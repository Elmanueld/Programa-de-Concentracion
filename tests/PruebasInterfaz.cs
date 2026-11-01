using System;
using System.Windows.Forms;
using CapaPresentacion;

internal static class PruebasInterfaz
{
    public static void Ejecutar(Action<bool, string> comprobar)
    {
        Application.EnableVisualStyles();
        using (var ventana = new Form1())
        {
            ventana.Show();
            Application.DoEvents();
            var pestanas = (TabControl)ventana.Controls.Find("pestanas", true)[0];
            comprobar(pestanas.TabPages.Count == 3, "Acceso, registro y verificación disponibles sin conexión SQL");
            comprobar(((TextBox)ventana.Controls.Find("clave", true)[0]).UseSystemPasswordChar &&
                ((TextBox)ventana.Controls.Find("contrasenaRegistro", true)[0]).UseSystemPasswordChar &&
                ((TextBox)ventana.Controls.Find("confirmar", true)[0]).UseSystemPasswordChar,
                "Las contraseñas están ocultas");
            var correo = (TextBox)ventana.Controls.Find("correo", true)[0];
            correo.Text = "pruebas@example.com";
            ((Button)ventana.Controls.Find("btnVerificarCorreo", true)[0]).PerformClick();
            comprobar(pestanas.SelectedTab.Name == "tabVerificacion" &&
                ventana.Controls.Find("correoVerificacion", true)[0].Text == correo.Text,
                "El botón del login abre verificación y conserva el correo");
            foreach (TabPage pagina in pestanas.TabPages)
            {
                pestanas.SelectedTab = pagina;
                ventana.Refresh();
                pagina.Refresh();
                Application.DoEvents();
                bool dentro = true;
                foreach (Control control in pagina.Controls)
                    dentro = dentro && control.Left >= 0 && control.Top >= 0 &&
                        control.Right <= pagina.ClientSize.Width && control.Bottom <= pagina.ClientSize.Height;
                comprobar(dentro, "Controles dentro de los límites: " + pagina.Text);
            }
            ventana.Close();
        }
    }
}
