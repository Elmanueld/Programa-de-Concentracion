using CapaDatos;

namespace CapaNegocio
{
    public sealed class N_SincronizacionOffline
    {
        public static event System.Action<System.Exception> ErrorAlActualizarCopiaLocal;
        private readonly D_ProcesadorSincronizacionOffline datos = new D_ProcesadorSincronizacionOffline();
        private static System.DateTime ultimaConsultaRemotaUtc = System.DateTime.MinValue;

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
            get { return System.DateTime.UtcNow - ultimaConsultaRemotaUtc >= System.TimeSpan.FromMinutes(2); }
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
            if (resultado) ultimaConsultaRemotaUtc = System.DateTime.UtcNow;
            return resultado;
        }
    }
}
