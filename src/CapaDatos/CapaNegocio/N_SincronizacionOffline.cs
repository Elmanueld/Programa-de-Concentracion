using CapaDatos;
using System.Diagnostics;
using System.Threading;

namespace CapaNegocio
{
    /// <summary>Exposes the synchronized application clock to the presentation layer.</summary>
    public static class RelojNegocio
    {
        public static System.DateTime Ahora { get { return CapaDatos.RelojAplicacion.Ahora; } }
        public static System.DateTime Hoy { get { return CapaDatos.RelojAplicacion.Hoy; } }
    }

    public sealed class N_SincronizacionOffline
    {
        public static event System.Action<System.Exception> ErrorAlActualizarCopiaLocal;
        private readonly D_ProcesadorSincronizacionOffline datos = new D_ProcesadorSincronizacionOffline();
        private static long ultimaConsultaRemotaTick;
        private static readonly long IntervaloConsultaRemotaTicks = Stopwatch.Frequency * 30L;

        static N_SincronizacionOffline()
        {
            D_SincronizacionOffline.ErrorAlActualizarCopiaLocal += error =>
            {
                var manejador = ErrorAlActualizarCopiaLocal;
                if (manejador == null) return;
                foreach (System.Action<System.Exception> receptor in manejador.GetInvocationList())
                {
                    try { receptor(error); }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError(
                            "No se pudo notificar el fallo de la copia offline: " + ex.GetType().Name);
                    }
                }
            };
        }

        public bool HayCambiosPendientes(int idUsuario)
        {
            return D_ProcesadorSincronizacionOffline.HayPendientes(idUsuario);
        }

        public bool EstaEnModoOffline
        {
            get { return D_SincronizacionOffline.ModoOffline; }
        }

        public bool HayCopiaNuevaNecesaria
        {
            get
            {
                long ultima = Interlocked.Read(ref ultimaConsultaRemotaTick);
                return ultima == 0 || Stopwatch.GetTimestamp() - ultima >= IntervaloConsultaRemotaTicks;
            }
        }

        public bool Sincronizar(int idUsuario)
        {
            if (idUsuario <= 0) throw new System.ArgumentException("El usuario no es válido.");
            if (!EstadoApiRemota.TieneSesionValida)
            {
                string contrasena;
                var cuenta = CredencialOfflineLocal.ObtenerSesionRecordada(out contrasena);
                if (cuenta == null || cuenta.IdUsuario != idUsuario || string.IsNullOrEmpty(contrasena))
                    return false;
                try
                {
                    var validada = new D_Usuario().IniciarSesion(cuenta.Correo, contrasena);
                    if (validada == null || validada.IdUsuario != idUsuario) return false;
                }
                catch (ApiRemotaNoDisponibleException) { return false; }
                finally { contrasena = null; }
            }
            bool resultado = datos.Sincronizar(idUsuario);
            if (resultado) Interlocked.Exchange(ref ultimaConsultaRemotaTick, Stopwatch.GetTimestamp());
            return resultado;
        }
    }
}
