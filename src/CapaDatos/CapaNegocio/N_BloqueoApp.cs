using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using CapaDatos;
using CapaEntidades;
using System.Runtime.InteropServices;

namespace CapaNegocio
{
    /// <summary>
    /// Supervisa las tareas del usuario y cierra procesos que no estén
    /// permitidos durante el tiempo de trabajo.
    /// </summary>
    public sealed class N_BloqueoApp : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        public static N_BloqueoApp Instancia { get; } =
            new N_BloqueoApp();
        public event EventHandler<AplicacionBloqueadaEventArgs>
        AplicacionBloqueada;
        public event EventHandler<DescansoIniciadoEventArgs>
        DescansoIniciado;

        private const int IntervaloRevisionProcesosMs = 2000;
        private const int IntervaloRecargaProgramacionSegundos = 30;
        private const int SegundosAntesDeForzarCierre = 4;

        private readonly object _sincronizacion = new object();
        private readonly Timer _temporizador;

        private int _idUsuario;
        private int _cicloEnEjecucion;
        private bool _disposed;
        private bool _descansoActivo;
        private DateTime _proximaRecarga = DateTime.MinValue;

        private Programacion _programacion = new Programacion();

        // Solo guarda temporalmente cuándo se pidió el cierre normal.
        // No es una lista persistente de aplicaciones bloqueadas.
        private readonly Dictionary<int, DateTime> _cierresSolicitados =
            new Dictionary<int, DateTime>();

        private static readonly HashSet<string> ProcesosVitales =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "system",
                "registry",
                "smss",
                "csrss",
                "wininit",
                "services",
                "lsass",
                "svchost",
                "winlogon",
                "fontdrvhost",
                "dwm",
                "explorer",
                "sihost",
                "taskhostw",
                "ctfmon",
                "conhost",
                "runtimebroker",
                "startmenuexperiencehost",
                "shellexperiencehost",
                "searchhost",
                "searchapp",
                "searchui",
                "searchindexer",
                "searchprotocolhost",
                "searchfilterhost",
                "applicationframehost",
                "textinputhost",
                "lockapp",
                "logonui",
                "securityhealthservice",
                "msmpeng",
                "audiodg"
            };

        private sealed class DescansoActual
        {
            public string NombreTarea { get; set; }
            public DateTime FinDescanso { get; set; }
            public int DescansosRestantes { get; set; }
        }

        private N_BloqueoApp()
        {
            _temporizador = new Timer(
                RevisarEstado,
                null,
                Timeout.Infinite,
                Timeout.Infinite);
        }

        /// <summary>
        /// Inicia o cambia la supervisión al usuario indicado.
        /// La primera revisión empieza inmediatamente.
        /// </summary>
        public void Iniciar(int idUsuario)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El ID de usuario no es válido.");

            lock (_sincronizacion)
            {
                if (_disposed)
                    throw new ObjectDisposedException("N_BloqueoApp");

                _idUsuario = idUsuario;
                _descansoActivo = false;
                _programacion = new Programacion();
                _proximaRecarga = DateTime.MinValue;
            }

            _temporizador.Change(
                0,
                IntervaloRevisionProcesosMs);
        }

        /// <summary>
        /// Fuerza una recarga tras crear, modificar o borrar una tarea.
        /// </summary>
        public void ActualizarProgramacion()
        {
            lock (_sincronizacion)
            {
                if (_disposed)
                    return;

                _proximaRecarga = DateTime.MinValue;
            }

            _temporizador.Change(
                0,
                IntervaloRevisionProcesosMs);
        }

        private void RevisarEstado(object estado)
        {

            if (Interlocked.Exchange(ref _cicloEnEjecucion, 1) != 0)
                return;

            try
            {
                int idUsuario;

                lock (_sincronizacion)
                {
                    if (_disposed)
                        return;

                    idUsuario = _idUsuario;
                }

                if (idUsuario <= 0)
                    return;

                DateTime ahora = DateTime.Now;

                if (ahora >= _proximaRecarga)
                    RecargarProgramacion(idUsuario, ahora);

                Programacion programacion;

                lock (_sincronizacion)
                {
                    programacion = _programacion;
                }

                List<ReglaTarea> tareasEnHorario = programacion.Tareas
                    .Where(regla =>
                        regla.Tarea.FechaInicio <= ahora &&
                        ahora < regla.Tarea.FechaFin)
                    .ToList();

                if (tareasEnHorario.Count == 0)
                {
                    _descansoActivo = false;
                    _cierresSolicitados.Clear();
                    return;
                }

                List<ReglaTarea> tareasTrabajando = tareasEnHorario
                    .Where(regla => !EstaEnDescanso(regla, ahora))
                    .ToList();

                if (tareasTrabajando.Count == 0)
                {
                    _cierresSolicitados.Clear();

                    if (!_descansoActivo)
                    {
                        DescansoActual descanso = tareasEnHorario
                            .Select(regla => ObtenerDescansoActual(regla, ahora))
                            .Where(actual => actual != null)
                            .OrderBy(actual => actual.FinDescanso)
                            .FirstOrDefault();

                        if (descanso != null)
                            NotificarDescansoIniciado(descanso);

                        _descansoActivo = true;
                    }

                    return;
                }

                _descansoActivo = false;

                List<ReglaTarea> tareasQueBloqueanApps = tareasTrabajando
                    .Where(regla => !regla.Tarea.NoBloquearApps)
                    .ToList();

                if (tareasQueBloqueanApps.Count == 0)
                {
                    _cierresSolicitados.Clear();
                    return;
                }


                var permitidas = new IdentidadesAplicacion();

                permitidas.Agregar(programacion.AplicacionesCriticas);

                permitidas.AgregarNombreProceso("chrome");
                permitidas.AgregarNombreProceso("msedge");
                permitidas.AgregarNombreProceso("brave");
                permitidas.AgregarNombreProceso("firefox");

                foreach (ReglaTarea regla in tareasQueBloqueanApps)
                    permitidas.Agregar(regla.AplicacionesPermitidas);

                string nombreTarea = tareasQueBloqueanApps[0].Tarea.Nombre;

                CerrarProcesosNoPermitidos(
                    permitidas,
                    ahora,
                    nombreTarea);
            }
            catch (Exception ex)
            {
                // Un fallo del supervisor no debe cerrar la aplicación WinForms.
                Trace.TraceError("Error en el bloqueo de aplicaciones: " + ex);
            }
            finally
            {
                Interlocked.Exchange(ref _cicloEnEjecucion, 0);
            }

        }

        private void RecargarProgramacion(int idUsuario, DateTime ahora)
        {
            try
            {
                Programacion nueva = CargarProgramacion(idUsuario);

                lock (_sincronizacion)
                {
                    if (_disposed || _idUsuario != idUsuario)
                        return;

                    _programacion = nueva;
                    _proximaRecarga = ahora.AddSeconds(
                        IntervaloRecargaProgramacionSegundos);
                }
            }
            catch (Exception ex)
            {
                // Conserva la última programación válida y reintenta pronto.
                Trace.TraceError("No se pudo recargar la programación: " + ex);

                lock (_sincronizacion)
                {
                    _proximaRecarga = ahora.AddSeconds(10);
                }
            }
        }

        private static Programacion CargarProgramacion(int idUsuario)
        {
            var resultado = new Programacion();

            List<E_Aplicacion> catalogoApps =
                new N_Aplicacion().BuscarAplicacion(idUsuario, "");

            var appsPorId = catalogoApps
                .Where(app => app.IdAplicacion > 0)
                .GroupBy(app => app.IdAplicacion)
                .ToDictionary(grupo => grupo.Key, grupo => grupo.First());

            foreach (E_Aplicacion app in catalogoApps.Where(a => a.EsCritica))
                resultado.AplicacionesCriticas.Agregar(app);

            List<E_Tarea> tareas =
                new N_Tarea().BuscarTarea(idUsuario, "");

            var negocioTareaApp = new N_TareaAplicacion();
            var negocioDescanso = new N_Descanso();

            foreach (E_Tarea tarea in tareas)
            {
                // Una tarea completada no debe volver a bloquear.
                if (tarea.Estado == 'C')
                    continue;

                var regla = new ReglaTarea
                {
                    Tarea = tarea,
                    Descanso = negocioDescanso.BuscarDescanso(idUsuario, tarea.IdTarea)
                };

                List<E_TareaAplicacion> asociaciones =
                    negocioTareaApp.Buscar(idUsuario, tarea.IdTarea);

                foreach (E_TareaAplicacion asociacion in asociaciones)
                {
                    E_Aplicacion app;

                    if (appsPorId.TryGetValue(
                        asociacion.IdAplicacion,
                        out app))
                    {
                        regla.AplicacionesPermitidas.Agregar(app);
                    }
                }

                resultado.Tareas.Add(regla);
            }

            return resultado;
        }

        private static bool EstaEnDescanso(
            ReglaTarea regla,
            DateTime ahora)
        {
            if (regla.Descanso == null ||
                !regla.Descanso.IntervaloDescanso.HasValue ||
                !regla.Descanso.DuracionDescanso.HasValue)
            {
                return false;
            }

            double intervaloSegundos =
                regla.Descanso.IntervaloDescanso.Value * 60.0;

            double duracionSegundos =
                regla.Descanso.DuracionDescanso.Value * 60.0;

            if (intervaloSegundos <= 0 || duracionSegundos <= 0)
                return false;

            double segundosDesdeInicio =
                (ahora - regla.Tarea.FechaInicio).TotalSeconds;

            if (segundosDesdeInicio < intervaloSegundos)
                return false;

            // El descanso se repite después de cada intervalo de trabajo.
            double cicloSegundos =
                intervaloSegundos + duracionSegundos;

            double posicionEnCiclo =
                segundosDesdeInicio % cicloSegundos;

            return posicionEnCiclo >= intervaloSegundos;
        }

        private static DescansoActual ObtenerDescansoActual(
     ReglaTarea regla,
     DateTime ahora)
        {
            if (regla.Descanso == null ||
                !regla.Descanso.IntervaloDescanso.HasValue ||
                !regla.Descanso.DuracionDescanso.HasValue)
            {
                return null;
            }

            // Los valores guardados están expresados en minutos.
            double intervaloSegundos =
                regla.Descanso.IntervaloDescanso.Value * 60.0;

            double duracionSegundos =
                regla.Descanso.DuracionDescanso.Value * 60.0;

            double cicloSegundos =
                intervaloSegundos + duracionSegundos;

            double segundosDesdeInicio =
                (ahora - regla.Tarea.FechaInicio).TotalSeconds;

            if (intervaloSegundos <= 0 ||
                duracionSegundos <= 0 ||
                cicloSegundos <= 0 ||
                segundosDesdeInicio < intervaloSegundos)
            {
                return null;
            }

            double posicionEnCiclo =
                segundosDesdeInicio % cicloSegundos;

            // Solo calcula la cuenta si ahora mismo estamos dentro del descanso.
            if (posicionEnCiclo < intervaloSegundos ||
                posicionEnCiclo >= cicloSegundos)
            {
                return null;
            }

            double inicioDescansoEnSegundos =
                segundosDesdeInicio -
                (posicionEnCiclo - intervaloSegundos);

            DateTime inicioDescanso =
                regla.Tarea.FechaInicio.AddSeconds(
                    inicioDescansoEnSegundos);

            DateTime finDescanso =
                inicioDescanso.AddSeconds(duracionSegundos);

            if (finDescanso > regla.Tarea.FechaFin)
                finDescanso = regla.Tarea.FechaFin;

            // Incluye el descanso actual y cuenta los siguientes cuyo inicio
            // ocurra antes de que termine la tarea.
            int descansosRestantes = 1;

            DateTime proximoDescanso =
                inicioDescanso.AddSeconds(cicloSegundos);

            while (proximoDescanso < regla.Tarea.FechaFin)
            {
                descansosRestantes++;
                proximoDescanso =
                    proximoDescanso.AddSeconds(cicloSegundos);
            }

            return new DescansoActual
            {
                NombreTarea = regla.Tarea.Nombre,
                FinDescanso = finDescanso,
                DescansosRestantes = descansosRestantes
            };
        }
        private void NotificarDescansoIniciado(DescansoActual descanso)
        {
            var manejador = DescansoIniciado;

            if (manejador == null || descanso == null)
                return;

            try
            {
                manejador(
                    this,
                    new DescansoIniciadoEventArgs(
                        descanso.NombreTarea,
                        descanso.FinDescanso,
                        descanso.DescansosRestantes));
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "No se pudo mostrar el aviso de descanso: " + ex);
            }
        }



        private void NotificarAplicacionBloqueada(
        string nombreAplicacion,
        string nombreTarea)
        {
            var manejador = AplicacionBloqueada;

            if (manejador == null)
                return;

            if (String.IsNullOrWhiteSpace(nombreAplicacion))
                nombreAplicacion = "esta aplicación";

            if (String.IsNullOrWhiteSpace(nombreTarea))
                nombreTarea = "una tarea activa";

            try
            {
                manejador(
                    this,
                    new AplicacionBloqueadaEventArgs(
                        nombreAplicacion,
                        nombreTarea));
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "No se pudo mostrar el aviso de bloqueo: " + ex);
            }
        }
        private void CerrarProcesosNoPermitidos(
        IdentidadesAplicacion permitidas,
        DateTime ahora,
     string nombreTarea)
        {
            int idProcesoActual = Process.GetCurrentProcess().Id;
            int sesionActual = Process.GetCurrentProcess().SessionId;
            var procesosVistos = new HashSet<int>();

            foreach (Process proceso in Process.GetProcesses())
            {
                using (proceso)
                {
                    try
                    {
                        if (proceso.Id == idProcesoActual)
                            continue;

                        // Solo actuar sobre una ventana principal visible.
                        // Los procesos de inicio que quedan en segundo plano se conservan.
                        IntPtr ventana = proceso.MainWindowHandle;

                        if (ventana == IntPtr.Zero || !IsWindowVisible(ventana))
                        {
                            _cierresSolicitados.Remove(proceso.Id);
                            continue;
                        }

                        if (EsProcesoVital(proceso, sesionActual))
                            continue;

                        string nombreProceso = proceso.ProcessName;
                        string rutaEjecutable;

                        try
                        {
                            rutaEjecutable = proceso.MainModule == null
                                ? null
                                : proceso.MainModule.FileName;
                        }
                        catch
                        {
                            // Si no se puede identificar con seguridad, no se cierra.
                            continue;
                        }

                        if (permitidas.Contiene(nombreProceso, rutaEjecutable))
                        {
                            _cierresSolicitados.Remove(proceso.Id);
                            continue;
                        }

                        procesosVistos.Add(proceso.Id);

                        DateTime solicitudAnterior;
                        if (_cierresSolicitados.TryGetValue(
                            proceso.Id,
                            out solicitudAnterior) &&
                            (ahora - solicitudAnterior).TotalSeconds <
                            SegundosAntesDeForzarCierre)
                        {
                            continue;
                        }

                        string nombreParaAviso = proceso.MainWindowTitle;

                        if (String.IsNullOrWhiteSpace(nombreParaAviso))
                            nombreParaAviso = nombreProceso;

                        NotificarAplicacionBloqueada(
                            nombreParaAviso,
                            nombreTarea);

                        if (proceso.CloseMainWindow())
                            _cierresSolicitados[proceso.Id] = ahora;
                        else
                            _cierresSolicitados.Remove(proceso.Id);
                        // Solicita cerrar la ventana. No termina el proceso a la fuerza.
                        if (proceso.CloseMainWindow())
                            _cierresSolicitados[proceso.Id] = ahora;
                        else
                            _cierresSolicitados.Remove(proceso.Id);
                    }
                    catch
                    {
                        // El proceso pudo terminar mientras se revisaba.
                    }
                }
            }

            foreach (int idProceso in _cierresSolicitados.Keys.ToList())
            {
                if (!procesosVistos.Contains(idProceso))
                    _cierresSolicitados.Remove(idProceso);
            }
        }
        private static HashSet<int> ObtenerPidsPermitidosYDescendientes(
    IdentidadesAplicacion permitidas)
        {
            var resultado = new HashSet<int>();

            if (permitidas == null)
                return resultado;

            Dictionary<int, int> padres = ObtenerMapaProcesosPadre();
            var hijosPorPadre = new Dictionary<int, List<int>>();

            foreach (KeyValuePair<int, int> relacion in padres)
            {
                List<int> hijos;

                if (!hijosPorPadre.TryGetValue(relacion.Value, out hijos))
                {
                    hijos = new List<int>();
                    hijosPorPadre[relacion.Value] = hijos;
                }

                hijos.Add(relacion.Key);
            }

            // Busca los procesos que coinciden con las apps autorizadas.
            foreach (Process proceso in Process.GetProcesses())
            {
                using (proceso)
                {
                    try
                    {
                        string nombre = proceso.ProcessName;

                        // Primero intenta por nombre; evita leer MainModule
                        // cuando el nombre ya coincide.
                        if (permitidas.Contiene(nombre, null))
                        {
                            resultado.Add(proceso.Id);
                            continue;
                        }

                        try
                        {
                            string ruta = proceso.MainModule == null
                                ? null
                                : proceso.MainModule.FileName;

                            if (permitidas.Contiene(nombre, ruta))
                                resultado.Add(proceso.Id);
                        }
                        catch
                        {
                            // No se pudo inspeccionar este proceso.
                        }
                    }
                    catch
                    {
                        // El proceso pudo terminar durante la búsqueda.
                    }
                }
            }

            // Recorre el árbol y agrega todos los descendientes.
            var pendientes = new Queue<int>(resultado);

            while (pendientes.Count > 0)
            {
                int pidPadre = pendientes.Dequeue();
                List<int> hijos;

                if (!hijosPorPadre.TryGetValue(pidPadre, out hijos))
                    continue;

                foreach (int pidHijo in hijos)
                {
                    if (resultado.Add(pidHijo))
                        pendientes.Enqueue(pidHijo);
                }
            }

            return resultado;
        }

        private static Dictionary<int, int> ObtenerMapaProcesosPadre()
        {
            const uint TH32CS_SNAPPROCESS = 0x00000002;

            var resultado = new Dictionary<int, int>();
            IntPtr snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

            if (snapshot == new IntPtr(-1))
                return resultado;

            try
            {
                var entrada = new PROCESSENTRY32();
                entrada.dwSize =
                    (uint)Marshal.SizeOf(typeof(PROCESSENTRY32));

                if (!Process32First(snapshot, ref entrada))
                    return resultado;

                do
                {
                    int pid = unchecked((int)entrada.th32ProcessID);
                    int pidPadre = unchecked((int)entrada.th32ParentProcessID);

                    if (pid > 0)
                        resultado[pid] = pidPadre;

                    entrada.dwSize =
                        (uint)Marshal.SizeOf(typeof(PROCESSENTRY32));
                }
                while (Process32Next(snapshot, ref entrada));
            }
            finally
            {
                CloseHandle(snapshot);
            }

            return resultado;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(
            uint dwFlags,
            uint th32ProcessID);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool Process32First(
            IntPtr hSnapshot,
            ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool Process32Next(
            IntPtr hSnapshot,
            ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);
        private static bool EsProcesoVital(
            Process proceso,
            int sesionActual)
        {
            try
            {
                if (proceso.SessionId != sesionActual)
                    return true;

#if DEBUG
                if (EsProcesoDeVisualStudio(proceso.ProcessName))
                    return true;
#endif

                if (ProcesosVitales.Contains(proceso.ProcessName))
                    return true;

                string ruta = proceso.MainModule == null
                    ? null
                    : proceso.MainModule.FileName;

                if (String.IsNullOrWhiteSpace(ruta))
                    return true;

                string carpetaWindows =
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.Windows);

                string prefijoWindows =
                    Path.GetFullPath(carpetaWindows)
                        .TrimEnd(Path.DirectorySeparatorChar) +
                    Path.DirectorySeparatorChar;

                return Path.GetFullPath(ruta).StartsWith(
                    prefijoWindows,
                    StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                string nombre;

                try
                {
                    nombre = proceso.ProcessName;
                }
                catch
                {
                    nombre = "(nombre no disponible)";
                }

                System.Diagnostics.Debug.WriteLine(
                    "[Bloqueador] No se pudo inspeccionar el proceso: " +
                    nombre +
                    " | PID: " + proceso.Id +
                    " | " + ex.GetType().Name +
                    ": " + ex.Message);

                // Se conserva por seguridad, igual que antes.
                return true;
            }
        }

        private static bool EsProcesoDeVisualStudio(string nombre)
        {
            return String.Equals(nombre, "devenv",
                       StringComparison.OrdinalIgnoreCase) ||
                   String.Equals(nombre, "msysmon",
                       StringComparison.OrdinalIgnoreCase) ||
                   String.Equals(nombre, "msvsmon",
                       StringComparison.OrdinalIgnoreCase) ||
                   String.Equals(nombre, "vsdebugconsole",
                       StringComparison.OrdinalIgnoreCase) ||
                   nombre.StartsWith("ServiceHub.",
                       StringComparison.OrdinalIgnoreCase) ||
                   nombre.StartsWith("vsdbg",
                       StringComparison.OrdinalIgnoreCase) ||
                   nombre.EndsWith(".vshost",
                       StringComparison.OrdinalIgnoreCase);
        }



        public void Dispose()
        {
            lock (_sincronizacion)
            {
                if (_disposed)
                    return;

                _disposed = true;
            }

            _temporizador.Dispose();
            _cierresSolicitados.Clear();
        }

        private sealed class Programacion
        {
            public List<ReglaTarea> Tareas { get; } =
                new List<ReglaTarea>();

            public IdentidadesAplicacion AplicacionesCriticas { get; } =
                new IdentidadesAplicacion();
        }

        private sealed class ReglaTarea
        {
            public E_Tarea Tarea { get; set; }
            public E_Descanso Descanso { get; set; }

            public IdentidadesAplicacion AplicacionesPermitidas { get; } =
                new IdentidadesAplicacion();
        }

        private sealed class IdentidadesAplicacion
        {

            private readonly HashSet<string> _rutas =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            private readonly HashSet<string> _nombres =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            private readonly HashSet<string> _familiasPaquete =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public void AgregarNombreProceso(string nombreProceso)
            {
                AgregarNombre(nombreProceso);
            }

            public void Agregar(E_Aplicacion app)
            {
                if (app == null)
                    return;

                AgregarNombre(app.Nombre);

                string identificador = app.Identificador;

                if (String.IsNullOrWhiteSpace(identificador))
                    return;

                int separadorAumid = identificador.IndexOf('!');

                if (separadorAumid > 0)
                {
                    _familiasPaquete.Add(
                        identificador.Substring(0, separadorAumid));

                    AgregarNombre(
                        identificador.Substring(separadorAumid + 1));
                }

                if (Path.IsPathRooted(identificador))
                {
                    try
                    {
                        string rutaCompleta =
                            Path.GetFullPath(identificador);

                        _rutas.Add(rutaCompleta);
                        AgregarNombre(
                            Path.GetFileNameWithoutExtension(rutaCompleta));
                    }
                    catch
                    {
                        // Se conserva la coincidencia por nombre.
                    }
                }
                else
                {
                    AgregarNombre(
                        Path.GetFileNameWithoutExtension(identificador));
                }
            }

            public void Agregar(IdentidadesAplicacion otras)
            {
                if (otras == null)
                    return;

                _rutas.UnionWith(otras._rutas);
                _nombres.UnionWith(otras._nombres);
                _familiasPaquete.UnionWith(otras._familiasPaquete);
            }

            public bool Contiene(string nombreProceso, string rutaEjecutable)
            {
                if (!String.IsNullOrWhiteSpace(nombreProceso) &&
                    _nombres.Contains(Normalizar(nombreProceso)))
                {
                    return true;
                }

                if (String.IsNullOrWhiteSpace(rutaEjecutable))
                    return false;

                string rutaCompleta;

                try
                {
                    rutaCompleta = Path.GetFullPath(rutaEjecutable);
                }
                catch
                {
                    return false;
                }

                if (_rutas.Contains(rutaCompleta))
                    return true;

                foreach (string familia in _familiasPaquete)
                {
                    if (rutaCompleta.IndexOf(
                        familia,
                        StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }

                string nombreArchivo =
                    Path.GetFileNameWithoutExtension(rutaCompleta);

                return _nombres.Contains(Normalizar(nombreArchivo));
            }

            private void AgregarNombre(string nombre)
            {
                string normalizado = Normalizar(nombre);

                if (!String.IsNullOrWhiteSpace(normalizado))
                    _nombres.Add(normalizado);
            }

            private static string Normalizar(string valor)
            {
                if (String.IsNullOrWhiteSpace(valor))
                    return String.Empty;

                return new string(
                    valor
                        .Where(Char.IsLetterOrDigit)
                        .Select(Char.ToUpperInvariant)
                        .ToArray());
            }
        }
    }
    public sealed class AplicacionBloqueadaEventArgs : EventArgs
    {
        public string NombreAplicacion { get; private set; }
        public string NombreTarea { get; private set; }

        public AplicacionBloqueadaEventArgs(
            string nombreAplicacion,
            string nombreTarea)
        {
            NombreAplicacion = nombreAplicacion;
            NombreTarea = nombreTarea;
        }
    }
    public sealed class DescansoIniciadoEventArgs : EventArgs
    {
        public string NombreTarea { get; private set; }
        public DateTime FinDescanso { get; private set; }
        public int DescansosRestantes { get; private set; }

        public DescansoIniciadoEventArgs(
            string nombreTarea,
            DateTime finDescanso,
            int descansosRestantes)
        {
            NombreTarea = nombreTarea;
            FinDescanso = finDescanso;
            DescansosRestantes = descansosRestantes;
        }
    }
}
