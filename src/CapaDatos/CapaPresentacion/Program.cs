using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using CapaEntidades;
using CapaNegocio;

namespace CapaPresentacion
{
    internal static class Program
    {
        internal static bool InicioDesdeWindows { get; private set; }
        internal static bool CierreControladoPorErrorCritico { get; set; }
        internal static bool SalidaSolicitada { get; set; }
        private const string NombreMutexInstancia = "Local\\ProgramaDeConcentracion.InstanciaUnica";
        private const string NombreEventoInstancia = "Local\\ProgramaDeConcentracion.MostrarVentana";
        private static PrincipalForm principalActual;
        private static frmLogin loginActual;

        internal static void RegistrarPrincipal(PrincipalForm formulario) { principalActual = formulario; }
        internal static void QuitarPrincipal(PrincipalForm formulario)
        {
            if (ReferenceEquals(principalActual, formulario)) principalActual = null;
        }
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool instanciaNueva;
            using (var mutexInstancia = new Mutex(true, NombreMutexInstancia, out instanciaNueva))
            using (var eventoInstancia = new EventWaitHandle(false, EventResetMode.AutoReset,
                NombreEventoInstancia, out bool eventoNuevo))
            try
            {
                bool inicioDesdeWindows = Environment.GetCommandLineArgs().Any(a =>
                    String.Equals(a, "--inicio-windows", StringComparison.OrdinalIgnoreCase));
                bool mostrarSolicitado = Environment.GetCommandLineArgs().Any(a =>
                    String.Equals(a, "--mostrar", StringComparison.OrdinalIgnoreCase));
                if (!instanciaNueva)
                {
                    // Solo una apertura explícita desde el acceso directo debe
                    // restaurar una ventana oculta. Lanzadores antiguos o
                    // automáticos sin argumentos nunca deben mostrarla.
                    if (mostrarSolicitado)
                    {
                        try { EventWaitHandle.OpenExisting(NombreEventoInstancia).Set(); }
                        catch (WaitHandleCannotBeOpenedException) { }
                        catch (UnauthorizedAccessException ex) { Trace.TraceError("No se pudo avisar a la instancia activa: " + ex.Message); }
                    }
                    else
                    {
                        Trace.TraceInformation(inicioDesdeWindows
                            ? "Se omitió un inicio automático duplicado; la ventana permanece en la bandeja."
                            : "Se omitió una instancia duplicada sin solicitud explícita para mostrar la ventana.");
                    }
                    return;
                }

                InicioDesdeWindows = inicioDesdeWindows;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += Aplicacion_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += Dominio_UnhandledException;
                TaskScheduler.UnobservedTaskException += Tareas_UnobservedTaskException;
                ConfigurarRegistroDiagnostico();

                var detectorInstancia = new System.Windows.Forms.Timer { Interval = 400 };
                detectorInstancia.Tick += (s, e) =>
                {
                    if (!eventoInstancia.WaitOne(0)) return;
                    if (principalActual != null && !principalActual.IsDisposed)
                        principalActual.RestaurarDesdeInstanciaSecundaria();
                    else if (loginActual != null && !loginActual.IsDisposed)
                        loginActual.RestaurarDesdeInstanciaSecundaria();
                };
                detectorInstancia.Start();

                string avisoInicio = null;
                E_Usuario sesionAutomatica = null;
                try
                {
                    var negocioUsuario = new N_Usuario();
                    // La preferencia se lee de la copia local (sin sesión API).
                    // La configuración remota se verifica después de autenticar.
                    if (negocioUsuario.InicioAutomaticoConfiguradoLocalmente())
                        sesionAutomatica = negocioUsuario.IniciarSesionAutomatico();
                    Trace.TraceInformation(sesionAutomatica == null
                        ? "Arranque: se mostrará el formulario de acceso."
                        : "Arranque: sesión automática preparada.");
                }
                catch (Exception ex)
                {
                    ErroresAplicacion.Registrar(ex, "Comprobación de inicio de sesión automático");
                    avisoInicio = OperacionUI.DescribirError(ex);
                }

                var inicio = new frmLogin(sesionAutomatica);
                loginActual = inicio;
                inicio.FormClosed += (s, e) => { if (ReferenceEquals(loginActual, inicio)) loginActual = null; };
                if (!String.IsNullOrWhiteSpace(avisoInicio)) inicio.MostrarAvisoInicial(avisoInicio);
                AjustePantalla.Preparar(inicio);
                Application.Run(inicio);
                detectorInstancia.Stop();
                detectorInstancia.Dispose();
            }
            catch (Exception ex)
            {
                MostrarErrorDeInicio(ex);
            }
        }

        private static void ConfigurarRegistroDiagnostico()
        {
            try
            {
                string carpeta = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Programa-de-Concentracion", "Logs");
                Directory.CreateDirectory(carpeta);
                Trace.Listeners.Add(new TextWriterTraceListener(
                    Path.Combine(carpeta, "diagnostico.log")));
                Trace.AutoFlush = true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("No se pudo iniciar el registro de diagnóstico: " + ex.GetType().Name);
            }
        }
        private static void MostrarErrorDeInicio(Exception excepcion)
        {
            string referencia = ErroresAplicacion.Registrar(excepcion, "Inicio de la aplicación");
            string mensaje;
            try { mensaje = OperacionUI.DescribirError(excepcion); }
            catch { mensaje = "No se pudo iniciar la aplicación. Vuelve a intentarlo."; }
            try
            {
                MessageBox.Show(mensaje + Environment.NewLine + Environment.NewLine +
                    "Código de ayuda: " + referencia + ". Si necesitas soporte, compártelo con quien te ayude.",
                    "No se pudo iniciar el programa", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
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
                try
                {
                    MessageBox.Show("Ocurrió un error al completar la operación. La aplicación seguirá abierta. " +
                        "Código de ayuda: " + referencia + ".",
                        "No se pudo completar la operación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch { }
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
