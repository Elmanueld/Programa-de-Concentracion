using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CapaPresentacion
{
    /// <summary>
    /// Muestra un formulario modal ocultando su propietario y lo restaura con un
    /// repintado completo al cerrar el diálogo.
    /// </summary>
    internal static class NavegacionFormularios
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
        private static extern int GetWindowLong(IntPtr hWnd, int indice);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
        private static extern int SetWindowLong(IntPtr hWnd, int indice, int valor);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertarDespues,
            int x, int y, int ancho, int alto, uint banderas);

        private const int GwlExStyle = -20;
        private const int WsExToolWindow = 0x00000080;
        private const int WsExAppWindow = 0x00040000;
        private const uint SwpNoSize = 0x0001;
        private const uint SwpNoMove = 0x0002;
        private const uint SwpNoZOrder = 0x0004;
        private const uint SwpNoActivate = 0x0010;
        private const uint SwpFrameChanged = 0x0020;

        public static DialogResult MostrarModal(
            Form anterior,
            Form actual)
        {
            if (actual == null)
                throw new ArgumentNullException("actual");

            if (anterior != null && !anterior.IsDisposed && actual.Owner == null)
                actual.Owner = anterior;

            AjustePantalla.Preparar(actual);
            TemaAplicacion.Aplicar(actual, TemaAplicacion.ModoOscuroActual);

            bool restaurarAnterior = anterior != null &&
                !anterior.IsDisposed && anterior.Visible;
            double opacidadAnterior = restaurarAnterior ? anterior.Opacity : 1d;
            int estiloExtendidoAnterior = 0;
            bool estiloTaskbarModificado = false;
            if (restaurarAnterior)
            {
                // No destruye ni saca de la cadena Z al propietario: ocultarlo
                // con Hide() hacía que algunos diálogos quedaran detrás de VS.
                anterior.Opacity = 0;
                estiloTaskbarModificado = OcultarDeBarraTareas(anterior,
                    out estiloExtendidoAnterior);
            }

            EventHandler activarAlMostrar = (s, e) =>
            {
                if (actual.IsDisposed || !actual.IsHandleCreated) return;
                actual.Activate();
                SetForegroundWindow(actual.Handle);
            };
            actual.Shown += activarAlMostrar;

            try
            {
                return actual.ShowDialog();
            }
            finally
            {
                actual.Shown -= activarAlMostrar;
                if (anterior != null && !anterior.IsDisposed &&
                    anterior.IsHandleCreated && restaurarAnterior)
                {
                    frmLogin login = anterior as frmLogin;
                    if (login != null) login.PrepararParaMostrar();
                    else
                    {
                        if (estiloTaskbarModificado)
                            RestaurarEstiloExtendido(anterior, estiloExtendidoAnterior);
                        anterior.Opacity = opacidadAnterior;
                    }
                    // Refresca el árbol tras devolver la opacidad para que no
                    // queden regiones vacías al cerrar el diálogo.
                    anterior.Refresh();
                    anterior.Activate();
                }
            }
        }

        private static bool OcultarDeBarraTareas(Form formulario, out int estiloOriginal)
        {
            estiloOriginal = 0;
            if (formulario == null || formulario.IsDisposed || !formulario.IsHandleCreated)
                return false;

            IntPtr handle = formulario.Handle;
            estiloOriginal = GetWindowLong(handle, GwlExStyle);
            int estiloOculto = (estiloOriginal & ~WsExAppWindow) | WsExToolWindow;
            if (estiloOculto == estiloOriginal) return false;

            SetWindowLong(handle, GwlExStyle, estiloOculto);
            SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0,
                SwpNoMove | SwpNoSize | SwpNoZOrder | SwpNoActivate | SwpFrameChanged);
            return true;
        }

        private static void RestaurarEstiloExtendido(Form formulario, int estiloOriginal)
        {
            if (formulario == null || formulario.IsDisposed || !formulario.IsHandleCreated)
                return;
            IntPtr handle = formulario.Handle;
            SetWindowLong(handle, GwlExStyle, estiloOriginal);
            SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0,
                SwpNoMove | SwpNoSize | SwpNoZOrder | SwpNoActivate | SwpFrameChanged);
        }
    }
}
