using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CapaPresentacion;

internal static class PruebasInterfaz
{
    public static void Ejecutar(Action<bool, string> comprobar)
    {
        Application.EnableVisualStyles();
        string modoAnterior = Environment.GetEnvironmentVariable("PC_MODO_DEMOSTRACION");
        Environment.SetEnvironmentVariable("PC_MODO_DEMOSTRACION", "false");
        try
        {
            using (var ventana = new frmLogin())
            {
                ventana.Show();
                Application.DoEvents();

                var pestanas = Obtener<TabControl>(ventana, "pestanas");
                comprobar(pestanas.TabPages.Count == 3,
                    "El login expone acceso, registro y verificación");

                comprobar(PropiedadBooleana(ObtenerControl(ventana, "clave"), "UseSystemPasswordChar") &&
                    PropiedadBooleana(ObtenerControl(ventana, "contrasenaRegistro"), "UseSystemPasswordChar") &&
                    PropiedadBooleana(ObtenerControl(ventana, "confirmar"), "UseSystemPasswordChar"),
                    "Los campos de contraseña están ocultos");

                Invocar(ventana, "btnNavRegistro_Click");
                comprobar(ObtenerControl(ventana, "pnlPaginaRegistro").Visible,
                    "El selector abre la página de crear cuenta");
                Invocar(ventana, "btnNavAcceso_Click");
                comprobar(ObtenerControl(ventana, "pnlPaginaAcceso").Visible,
                    "El selector vuelve a la página de acceso");

                Control correo = ObtenerControl(ventana, "correo");
                correo.Text = "pruebas@example.com";
                ObtenerControl(ventana, "correoVerificacion").Text = correo.Text;
                Invocar(ventana, "ActualizarVista", pestanas.TabPages[2], false);
                comprobar(ObtenerControl(ventana, "pnlPaginaVerificacion").Visible &&
                    ObtenerControl(ventana, "correoVerificacion").Text == correo.Text,
                    "La página de código muestra el correo preparado para verificación");

                Control tarjeta = ObtenerControl(ventana, "pnlTarjeta");
                bool panelesDentro = new[] { "pnlPaginaAcceso", "pnlPaginaRegistro", "pnlPaginaVerificacion" }
                    .Select(nombre => ObtenerControl(ventana, nombre))
                    .All(panel => panel.Left >= 0 && panel.Top >= 0 &&
                        panel.Right <= tarjeta.ClientSize.Width && panel.Bottom <= tarjeta.ClientSize.Height);
                comprobar(panelesDentro, "Las tres páginas de acceso caben en la tarjeta del login");
                ventana.Close();
            }

            ProbarPaginador(comprobar);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PC_MODO_DEMOSTRACION", modoAnterior);
        }
    }

    private static void ProbarPaginador(Action<bool, string> comprobar)
    {
        using (var ventana = new FormularioSuave())
        {
            ventana.FormBorderStyle = FormBorderStyle.None;
            ventana.ClientSize = new Size(300, 230);
            var contenedor = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(270, 205),
                BackColor = Color.White
            };
            var lista = new BufferedFlowLayoutPanel
            {
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Location = new Point(12, 12),
                Size = new Size(246, 181),
                BackColor = Color.Transparent
            };
            contenedor.Controls.Add(lista);
            ventana.Controls.Add(contenedor);

            lista.Controls.Add(new Label
            {
                Text = "Mostrando aplicaciones",
                Size = new Size(220, 38),
                Margin = new Padding(6)
            });
            for (int i = 1; i <= 4; i++)
            {
                var tarjeta = new Panel { Size = new Size(100, 70), Margin = new Padding(4) };
                tarjeta.Controls.Add(new Label
                {
                    Text = "Aplicación " + i,
                    AutoSize = true,
                    Location = new Point(4, 4)
                });
                lista.Controls.Add(tarjeta);
            }

            ventana.Show();
            Application.DoEvents();
            Application.DoEvents();

            Panel barra = contenedor.Controls.OfType<Panel>()
                .FirstOrDefault(panel => panel.Controls.Cast<Control>().Count(control =>
                    control.GetType().FullName == "AntdUI.Button") == 2);
            comprobar(barra != null, "El layout crea la barra con flechas AntdUI");
            if (barra == null) return;

            var botones = barra.Controls.Cast<Control>()
                .Where(control => control.GetType().FullName == "AntdUI.Button").ToArray();
            comprobar(botones.All(boton => boton.Width >= 48 && boton.Height >= 42 &&
                boton.Left >= 0 && boton.Top >= 0 && boton.Right <= barra.ClientSize.Width &&
                boton.Bottom <= barra.ClientSize.Height),
                "Las flechas redondeadas caben dentro de la barra sin recorte");
            comprobar(botones.All(boton => !String.IsNullOrWhiteSpace(boton.Text)),
                "Las dos flechas muestran su texto completo");

            Label pagina = barra.Controls.OfType<Label>().FirstOrDefault(label => label.Text.Contains("/"));
            comprobar(pagina != null && pagina.Text == "1 / 2",
                "El tamaño de página usa las dimensiones de las tarjetas, no las del encabezado (" + (pagina == null ? "sin contador" : pagina.Text) + ")");
            comprobar(barra.Controls.OfType<Label>().Any(label =>
                label.Text == "Mostrando aplicaciones") && lista.Controls.Count == 2,
                "El encabezado queda en la barra y las tarjetas no pierden espacio de contenido");

            bool nombresDentro = lista.Controls.Cast<Control>().All(tarjeta =>
                tarjeta.Controls.OfType<Label>().Any(label => label.Text.StartsWith("Aplicación ") &&
                    label.Left >= 0 && label.Top >= 0 && label.Right <= tarjeta.ClientSize.Width &&
                    label.Bottom <= tarjeta.ClientSize.Height));
            comprobar(nombresDentro, "Los nombres de tarjetas de la página se ven dentro de cada tarjeta");
            ventana.Close();
        }
    }

    private static T Obtener<T>(Control raiz, string nombre) where T : Control
    {
        Control[] encontrados = raiz.Controls.Find(nombre, true);
        if (encontrados.Length == 0) throw new InvalidOperationException("No existe el control " + nombre);
        T control = encontrados[0] as T;
        if (control == null) throw new InvalidOperationException("Tipo inesperado para " + nombre);
        return control;
    }

    private static Control ObtenerControl(Control raiz, string nombre)
    {
        Control[] encontrados = raiz.Controls.Find(nombre, true);
        if (encontrados.Length == 0) throw new InvalidOperationException("No existe el control " + nombre);
        return encontrados[0];
    }

    private static bool PropiedadBooleana(Control control, string nombre)
    {
        PropertyInfo propiedad = control.GetType().GetProperty(nombre);
        return propiedad != null && propiedad.PropertyType == typeof(bool) &&
            (bool)propiedad.GetValue(control, null);
    }

    private static void Invocar(frmLogin ventana, string nombre, params object[] parametros)
    {
        MethodInfo metodo = typeof(frmLogin).GetMethod(nombre,
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (metodo == null) throw new InvalidOperationException("No existe el método " + nombre);
        metodo.Invoke(ventana, parametros.Length == 0
            ? new object[] { null, EventArgs.Empty }
            : parametros);
    }
}
