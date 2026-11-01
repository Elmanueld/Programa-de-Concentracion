using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace CapaNegocio
{
    /// <summary>
    /// Solicita el cierre normal de procesos de usuario mientras está activo.
    /// No fuerza la terminación de procesos.
    /// </summary>
    public sealed class N_BloqueoApp : IDisposable
    {
        private static readonly HashSet<string> ProcesosExcluidos =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "System", "Registry", "Idle", "csrss", "wininit",
                "winlogon", "services", "lsass", "smss", "svchost",
                "explorer"
            };

        private readonly HashSet<int> procesosNotificados = new HashSet<int>();
        private readonly object sincronizacion = new object();
        private Timer temporizador;
        private int revisionEnCurso;
        private bool disposed;

        public bool EstaActivo
        {
            get { return temporizador != null; }
        }

        public void Iniciar(int intervaloMilisegundos = 2000)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(N_BloqueoApp));

            if (intervaloMilisegundos < 500)
                throw new ArgumentOutOfRangeException(nameof(intervaloMilisegundos),
                    "El intervalo mínimo es de 500 milisegundos.");

            if (temporizador != null)
                return;

            temporizador = new Timer(RevisionProcesos, null, 0, intervaloMilisegundos);
        }

        public void Detener()
        {
            Timer timer = temporizador;
            temporizador = null;

            if (timer != null)
                timer.Dispose();

            lock (sincronizacion)
            {
                procesosNotificados.Clear();
            }
        }

        private void RevisionProcesos(object estado)
        {
            if (Interlocked.Exchange(ref revisionEnCurso, 1) == 1)
                return;

            try
            {
                int miPid;
                using (Process procesoActual = Process.GetCurrentProcess())
                {
                    miPid = procesoActual.Id;
                }

                foreach (Process proceso in Process.GetProcesses())
                {
                    using (proceso)
                    {
                        try
                        {
                            if (proceso.Id == miPid ||
                                ProcesosExcluidos.Contains(proceso.ProcessName))
                            {
                                continue;
                            }

                            // Se solicita una sola vez por PID para no repetir diálogos
                            // de confirmación mientras la aplicación siga abierta.
                            lock (sincronizacion)
                            {
                                if (!procesosNotificados.Add(proceso.Id))
                                    continue;
                            }

                            proceso.CloseMainWindow();
                        }
                        catch (InvalidOperationException)
                        {
                            // El proceso pudo terminar durante la revisión.
                        }
                        catch (System.ComponentModel.Win32Exception)
                        {
                            // Windows puede denegar el acceso a ciertos procesos.
                        }
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // La lista de procesos puede cambiar durante la enumeración.
            }
            finally
            {
                Interlocked.Exchange(ref revisionEnCurso, 0);
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            Detener();
            disposed = true;
        }
    }
}
