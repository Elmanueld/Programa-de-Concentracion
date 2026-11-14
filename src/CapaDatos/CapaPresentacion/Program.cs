using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaEntidades;
using CapaNegocio;

namespace CapaPresentacion
{
    internal static class Program
    {
        internal static bool InicioDesdeWindows { get; private set; }
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            InicioDesdeWindows = Environment.GetCommandLineArgs().Any(a =>
                String.Equals(a, "--inicio-windows", StringComparison.OrdinalIgnoreCase));
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            E_Usuario sesionAutomatica = null;
            try { sesionAutomatica = new N_Usuario().IniciarSesionAutomatico(); }
            catch { /* Si la sesión automática falla, se muestra el acceso normal. */ }
            var inicio = new frmLogin(sesionAutomatica);
            AjustePantalla.Preparar(inicio);
            Application.Run(inicio);
        }
    }
}
