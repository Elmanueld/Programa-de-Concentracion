using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CapaEntidades;

namespace CapaNegocio
{
    /// <summary>Aplica y actualiza las reglas de conexión web durante las tareas.</summary>
    public sealed class N_BloqueoWeb : IDisposable
    {
        private static readonly Lazy<N_BloqueoWeb> InstanciaLazy =
            new Lazy<N_BloqueoWeb>(() => new N_BloqueoWeb());
        public static N_BloqueoWeb Instancia { get { return InstanciaLazy.Value; } }

        private const int IntervaloRevisionMs = 3000;
        private const int IntervaloRecargaSegundos = 30;
        private readonly object _sync = new object();
        private readonly System.Threading.Timer _timer;
        private readonly N_FiltroWebWfp _filtro = new N_FiltroWebWfp();
        private readonly N_ProxyWebLocal _proxy = new N_ProxyWebLocal();
        private int _idUsuario;
        private int _cicloActivo;
        private bool _disposed;
        private DateTime _proximaRecarga = DateTime.MinValue;
        private List<ReglaWeb> _reglas = new List<ReglaWeb>();
        private string _ultimaFirma;
        private string _ultimoError;

        public event Action<Exception, string> ErrorAplicandoBloqueo;

        public static void RecuperarProxySiQuedoHuerfano()
        {
            N_ProxyWebLocal.RecuperarProxySiQuedoHuerfano();
        }

        private N_BloqueoWeb()
        {
            _timer = new System.Threading.Timer(RevisarEstado, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Iniciar(int idUsuario)
        {
            if (idUsuario <= 0) throw new ArgumentException("El ID de usuario no es válido.");
            lock (_sync)
            {
                if (_disposed) throw new ObjectDisposedException("N_BloqueoWeb");
                _idUsuario = idUsuario;
                _reglas = new List<ReglaWeb>();
                _proximaRecarga = DateTime.MinValue;
                _ultimaFirma = null;
            }
            _timer.Change(0, IntervaloRevisionMs);
        }

        public void ActualizarProgramacion()
        {
            lock (_sync)
            {
                if (_disposed) return;
                _proximaRecarga = DateTime.MinValue;
                _ultimaFirma = null;
            }
            _timer.Change(0, IntervaloRevisionMs);
        }

        private void RevisarEstado(object estado)
        {
            if (Interlocked.Exchange(ref _cicloActivo, 1) != 0) return;
            try
            {
                int idUsuario;
                lock (_sync)
                {
                    if (_disposed) return;
                    idUsuario = _idUsuario;
                }
                if (idUsuario <= 0) return;

                DateTime ahora = DateTime.Now;
                if (ahora >= _proximaRecarga) RecargarProgramacion(idUsuario, ahora);
                List<ReglaWeb> reglas;
                lock (_sync) reglas = _reglas.ToList();

                List<ReglaWeb> tareasEnHorario = reglas
                    .Where(r => r.FechaInicio <= ahora && ahora < r.FechaFin)
                    .ToList();
                List<ReglaWeb> tareasTrabajando = tareasEnHorario
                    .Where(r => !EstaEnDescanso(r, ahora))
                    .ToList();

                if (tareasTrabajando.Count == 0)
                {
                    _filtro.Limpiar();
                    _proxy.Detener();
                    _ultimaFirma = null;
                    return;
                }

                List<Uri> urlsPermitidas = tareasTrabajando
                    .SelectMany(r => r.UrlsPermitidas)
                    .GroupBy(uri => uri.AbsoluteUri, StringComparer.OrdinalIgnoreCase)
                    .Select(grupo => grupo.First())
                    .ToList();
                List<E_Aplicacion> aplicacionesPermitidas = tareasTrabajando
                    .SelectMany(r => r.AplicacionesPermitidas)
                    .Where(app => app != null)
                    .GroupBy(app => app.IdAplicacion != 0
                        ? "id:" + app.IdAplicacion
                        : (app.Identificador ?? app.Nombre ?? String.Empty), StringComparer.OrdinalIgnoreCase)
                    .Select(grupo => grupo.First())
                    .ToList();
                _proxy.ActualizarAplicacionesPermitidas(aplicacionesPermitidas);
                string firma = CrearFirma(urlsPermitidas, aplicacionesPermitidas);
                if (String.Equals(firma, _ultimaFirma, StringComparison.Ordinal)) return;

                List<string> hosts = urlsPermitidas
                    .Select(uri => uri.IdnHost)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                try
                {
                    _proxy.Iniciar(hosts);
                    _filtro.Actualizar(_proxy.Puerto);
                }
                catch
                {
                    _filtro.Limpiar();
                    _proxy.Detener();
                    throw;
                }
                _ultimaFirma = firma;
                _ultimoError = null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error en el bloqueo de webs WFP: " + ex);
                Exception errorNotificado = ex;
                string contexto = "Se intentó activar la regla de cierre seguro para evitar un acceso sin filtrar.";
                try
                {
                    if (_idUsuario > 0)
                        _filtro.Actualizar(1); // Si el proxy falla, la regla queda cerrada para evitar un bypass.
                }
                catch (Exception errorFiltro)
                {
                    Trace.TraceError("Tampoco se pudo dejar cerrado el acceso web: " + errorFiltro);
                    errorNotificado = new AggregateException(
                        "Falló el filtro web y tampoco se pudo activar la regla de cierre seguro.", ex, errorFiltro);
                    contexto = "No se pudo confirmar que el bloqueo web siga activo. Evita depender de este filtro hasta corregir el error.";
                }
                NotificarError(errorNotificado, contexto);
            }
            finally
            {
                Interlocked.Exchange(ref _cicloActivo, 0);
            }
        }

        private void RecargarProgramacion(int idUsuario, DateTime ahora)
        {
            try
            {
                List<ReglaWeb> nuevas = CargarReglas(idUsuario);
                lock (_sync)
                {
                    if (_disposed || _idUsuario != idUsuario) return;
                    _reglas = nuevas;
                    _proximaRecarga = ahora.AddSeconds(IntervaloRecargaSegundos);
                    _ultimoError = null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("No se pudo cargar la programación web: " + ex);
                NotificarError(ex,
                    "Se conserva la última programación válida y el filtro volverá a intentar cargar las reglas.");
                lock (_sync) _proximaRecarga = ahora.AddSeconds(10);
            }
        }

        private void NotificarError(Exception error, string contexto)
        {
            string clave = error.GetType().FullName + "|" + error.Message;
            if (String.Equals(_ultimoError, clave, StringComparison.Ordinal)) return;
            _ultimoError = clave;
            Action<Exception, string> manejador = ErrorAplicandoBloqueo;
            if (manejador == null) return;
            try { manejador(error, contexto); }
            catch (Exception eventError)
            {
                Trace.TraceWarning("No se pudo avisar el error del filtro web: " + eventError.GetType().Name);
            }
        }

        private static List<ReglaWeb> CargarReglas(int idUsuario)
        {
            // Mantener el mismo camino que la interfaz y la versión que ya
            // aplicaba el filtro correctamente: remoto en línea, copia local
            // como fallback cuando el servicio no está disponible.
            List<E_Tarea> tareas = new N_Tarea().BuscarTarea(idUsuario, "");
            List<E_Web> catalogo = new N_Web().BuscarWeb(idUsuario, "");
            List<E_Aplicacion> aplicaciones = new N_Aplicacion().BuscarAplicacion(idUsuario, "");
            var websPorId = catalogo.Where(w => w != null && w.IdWeb > 0)
                .GroupBy(w => w.IdWeb).ToDictionary(g => g.Key, g => g.First());
            var aplicacionesPorId = aplicaciones.Where(app => app != null && app.IdAplicacion != 0)
                .GroupBy(app => app.IdAplicacion).ToDictionary(g => g.Key, g => g.First());
            var tareaWeb = new N_TareaWeb();
            var tareaAplicacion = new N_TareaAplicacion();
            var descansos = new N_Descanso();
            List<E_Aplicacion> aplicacionesCriticas = aplicaciones.Where(app => app.EsCritica).ToList();
            var resultado = new List<ReglaWeb>();

            foreach (E_Tarea tarea in tareas)
            {
                if (tarea == null || tarea.Estado == 'C' || tarea.Estado == 'X' || tarea.NoBloquearWebs)
                    continue;
                var regla = new ReglaWeb
                {
                    FechaInicio = tarea.FechaInicio,
                    FechaFin = tarea.FechaFin,
                    Descanso = descansos.BuscarDescanso(idUsuario, tarea.IdTarea)
                };
                regla.AplicacionesPermitidas.AddRange(aplicacionesCriticas);
                foreach (E_TareaAplicacion relacion in tareaAplicacion.Buscar(idUsuario, tarea.IdTarea))
                {
                    E_Aplicacion aplicacion;
                    if (relacion != null && aplicacionesPorId.TryGetValue(relacion.IdAplicacion, out aplicacion))
                        regla.AplicacionesPermitidas.Add(aplicacion);
                }
                foreach (E_TareaWeb relacion in tareaWeb.Buscar(idUsuario, tarea.IdTarea))
                {
                    E_Web web;
                    Uri uri;
                    if (relacion == null || !websPorId.TryGetValue(relacion.IdWeb, out web) ||
                        String.IsNullOrWhiteSpace(web.Link) ||
                        !Uri.TryCreate(web.Link, UriKind.Absolute, out uri) ||
                        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                        continue;
                    regla.UrlsPermitidas.Add(uri);
                }
                resultado.Add(regla);
            }
            return resultado;
        }

        private static bool EstaEnDescanso(ReglaWeb regla, DateTime ahora)
        {
            if (regla.Descanso == null || !regla.Descanso.IntervaloDescanso.HasValue ||
                !regla.Descanso.DuracionDescanso.HasValue) return false;
            double intervalo = regla.Descanso.IntervaloDescanso.Value * 60.0;
            double duracion = regla.Descanso.DuracionDescanso.Value * 60.0;
            if (intervalo <= 0 || duracion <= 0) return false;
            double transcurrido = (ahora - regla.FechaInicio).TotalSeconds;
            if (transcurrido < intervalo) return false;
            return transcurrido % (intervalo + duracion) >= intervalo;
        }

        private static string CrearFirma(IEnumerable<Uri> urls, IEnumerable<E_Aplicacion> aplicaciones)
        {
            return String.Join(Environment.NewLine,
                urls.Select(u => "WEB:" + u.AbsoluteUri)
                    .Concat(aplicaciones.Select(a => "APP:" + (a.Identificador ?? a.Nombre ?? String.Empty)))
                    .OrderBy(valor => valor, StringComparer.OrdinalIgnoreCase));
        }

        public void Detener()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            lock (_sync)
            {
                if (_disposed) return;
                _idUsuario = 0;
                _reglas.Clear();
                _ultimaFirma = null;
                _proximaRecarga = DateTime.MinValue;
            }
            while (Interlocked.CompareExchange(ref _cicloActivo, 0, 0) != 0) Thread.Sleep(10);
            _filtro.Limpiar();
            _proxy.Detener();
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed) return;
                _disposed = true;
            }
            _timer.Dispose();
            while (Interlocked.CompareExchange(ref _cicloActivo, 0, 0) != 0) Thread.Sleep(10);
            _filtro.Dispose();
            _proxy.Dispose();
        }

        private sealed class ReglaWeb
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public E_Descanso Descanso { get; set; }
            public List<Uri> UrlsPermitidas { get; } = new List<Uri>();
            public List<E_Aplicacion> AplicacionesPermitidas { get; } = new List<E_Aplicacion>();
        }
    }
}
