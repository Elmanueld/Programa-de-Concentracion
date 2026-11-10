using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CapaPresentacion
{
    internal static class ArrastreFormulario
    {
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private static readonly IntPtr HTCAPTION = new IntPtr(2);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam);

        public static void Habilitar(Form formulario, Control panelSuperior)
        {
            if (formulario == null)
                throw new ArgumentNullException("formulario");
            if (panelSuperior == null)
                throw new ArgumentNullException("panelSuperior");

            EnlazarControles(formulario, panelSuperior);
        }

        private static void EnlazarControles(Form formulario, Control control)
        {
            if (EsInteractivo(control))
                return;

            control.MouseDown += (sender, e) =>
            {
                if (e.Button != MouseButtons.Left || formulario.IsDisposed)
                    return;

                ReleaseCapture();
                SendMessage(
                    formulario.Handle,
                    WM_NCLBUTTONDOWN,
                    HTCAPTION,
                    IntPtr.Zero);
            };

            foreach (Control hijo in control.Controls)
                EnlazarControles(formulario, hijo);
        }

        private static bool EsInteractivo(Control control)
        {
            return control is ButtonBase ||
                control is TextBoxBase ||
                control is ComboBox ||
                control is AntdUI.Button;
        }
    }
}
