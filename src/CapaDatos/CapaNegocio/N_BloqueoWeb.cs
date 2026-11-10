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
        public static N_BloqueoWeb Instancia { get; } = new N_BloqueoWeb();

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

        public event Action<string> ErrorAplicandoBloqueo;

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
                string firma = CrearFirma(urlsPermitidas);
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
                if (!String.Equals(_ultimoError, ex.Message, StringComparison.Ordinal))
                {
                    _ultimoError = ex.Message;
                    Action<string> manejador = ErrorAplicandoBloqueo;
                    if (manejador != null)
                    {
                        try { manejador(ex.Message); }
                        catch (Exception eventError) { Trace.TraceWarning("No se pudo avisar el error del proxy: " + eventError.Message); }
                    }
                }
                try
                {
                    if (_idUsuario > 0)
                        _filtro.Actualizar(1); // Si el proxy falla, la regla queda cerrada para evitar un bypass.
                }
                catch (Exception errorFiltro)
                {
                    Trace.TraceError("Tampoco se pudo dejar cerrado el acceso web: " + errorFiltro);
                }
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
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("No se pudo cargar la programación web: " + ex);
                lock (_sync) _proximaRecarga = ahora.AddSeconds(10);
            }
        }

        private static List<ReglaWeb> CargarReglas(int idUsuario)
        {
            List<E_Tarea> tareas = new N_Tarea().BuscarTarea(idUsuario, "");
            List<E_Web> catalogo = new N_Web().BuscarWeb(idUsuario, "");
            var websPorId = catalogo.Where(w => w != null && w.IdWeb > 0)
                .GroupBy(w => w.IdWeb).ToDictionary(g => g.Key, g => g.First());
            var tareaWeb = new N_TareaWeb();
            var descansos = new N_Descanso();
            var resultado = new List<ReglaWeb>();

            foreach (E_Tarea tarea in tareas)
            {
                if (tarea == null || tarea.Estado == 'C' || tarea.NoBloquearWebs)
                    continue;
                var regla = new ReglaWeb
                {
                    FechaInicio = tarea.FechaInicio,
                    FechaFin = tarea.FechaFin,
                    Descanso = descansos.BuscarDescanso(idUsuario, tarea.IdTarea)
                };
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

        private static string CrearFirma(IEnumerable<Uri> urls)
        {
            return String.Join(Environment.NewLine, urls.Select(u => u.AbsoluteUri)
                .OrderBy(u => u, StringComparer.OrdinalIgnoreCase));
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
        }
    }
}
