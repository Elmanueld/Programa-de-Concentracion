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
        internal static bool CierreControladoPorErrorCritico { get; set; }
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
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Aplicacion_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += Dominio_UnhandledException;
            TaskScheduler.UnobservedTaskException += Tareas_UnobservedTaskException;

            E_Usuario sesionAutomatica = null;
            string avisoInicio = null;
            if (N_Usuario.ConexionRemotaConfigurada)
            {
                try { sesionAutomatica = new N_Usuario().IniciarSesionAutomatico(); }
                catch (Exception ex)
                {
                    ErroresAplicacion.Registrar(ex, "Inicio automático");
                    avisoInicio = OperacionUI.DescribirError(ex);
                }
            }
            else
                avisoInicio = "No se pudo conectar con tu cuenta en línea. Si ya usaste esta cuenta en este equipo, puedes intentar entrar sin conexión. Comprueba Internet e inténtalo de nuevo.";
            var inicio = new frmLogin(sesionAutomatica);
            if (!String.IsNullOrWhiteSpace(avisoInicio)) inicio.MostrarAvisoInicial(avisoInicio);
            AjustePantalla.Preparar(inicio);
            Application.Run(inicio);
        }

        private static void Aplicacion_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            string referencia = ErroresAplicacion.Registrar(e.Exception, "Hilo de interfaz");
            try
            {
                bool errorCritico = OperacionUI.RequiereCierreControlado(e.Exception);
                MessageBox.Show(
                    (errorCritico
                        ? "No se pudieron guardar los cambios de forma segura. La aplicación se cerrará para proteger tu información. Vuelve a abrirla e inténtalo otra vez."
                        : OperacionUI.DescribirError(e.Exception) + Environment.NewLine + Environment.NewLine +
                            "La aplicación seguirá abierta y no repetirá la acción automáticamente.") +
                    Environment.NewLine + Environment.NewLine + "Código de ayuda: " + referencia +
                    ". Si necesitas soporte, compártelo con quien te ayude.",
                    "No se pudo completar la operación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (errorCritico)
                {
                    CierreControladoPorErrorCritico = true;
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                ErroresAplicacion.Registrar(ex, "Aviso de error de interfaz");
            }
        }

        private static void Dominio_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var excepcion = e.ExceptionObject as Exception ??
                new Exception("El dominio lanzó una excepción que no es de tipo Exception.");
            string referencia = ErroresAplicacion.Registrar(excepcion, "Excepción no controlada del proceso");
            try
            {
                MessageBox.Show(
                    e.IsTerminating
                        ? "Ocurrió un error crítico y Windows cerrará la aplicación para proteger los datos. "
                            + "Código de ayuda: " + referencia + ". Contacta con soporte."
                        : "Ocurrió un problema inesperado. Código de ayuda: " + referencia +
                            ". Contacta con soporte.",
                    "Error crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
        }

        private static void Tareas_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            ErroresAplicacion.Registrar(e.Exception, "Tarea asíncrona sin observar");
            e.SetObserved();
        }
    }
}
