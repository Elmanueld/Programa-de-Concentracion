using CapaDatos;

namespace CapaNegocio
{
    public sealed class N_SincronizacionOffline
    {
        private readonly D_ProcesadorSincronizacionOffline datos = new D_ProcesadorSincronizacionOffline();
        private static System.DateTime ultimaConsultaRemotaUtc = System.DateTime.MinValue;

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
            bool resultado = datos.Sincronizar(idUsuario);
            if (resultado) ultimaConsultaRemotaUtc = System.DateTime.UtcNow;
            return resultado;
        }
    }
}
