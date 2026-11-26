using System;
using System.Diagnostics;

namespace CapaDatos
{
    /// <summary>
    /// A shared clock anchored to the API's UTC time and advanced with a
    /// monotonic counter, so local clock changes do not affect task timing.
    /// </summary>
    public static class RelojAplicacion
    {
        private static readonly object Candado = new object();
        private static bool sincronizado;
        private static long anclaStopwatch;
        private static DateTime anclaUtc;

        public static DateTime UtcAhora
        {
            get
            {
                lock (Candado)
                {
                    if (!sincronizado) return DateTime.UtcNow;
                    return anclaUtc.AddSeconds((Stopwatch.GetTimestamp() - anclaStopwatch) /
                        (double)Stopwatch.Frequency);
                }
            }
        }

        public static DateTime Ahora => UtcAhora.ToLocalTime();
        public static DateTime Hoy => Ahora.Date;

        internal static void Sincronizar(DateTimeOffset horaServidorUtc, long inicioSolicitud, long recepcionRespuesta)
        {
            // The server timestamp is produced while sending the response.
            // Add half the round-trip time to estimate the time at receipt.
            double idaYVuelta = Math.Max(0, (recepcionRespuesta - inicioSolicitud) /
                (double)Stopwatch.Frequency);
            DateTime utcEstimado = horaServidorUtc.UtcDateTime.AddSeconds(idaYVuelta / 2d);

            lock (Candado)
            {
                if (sincronizado)
                {
                    DateTime utcActual = anclaUtc.AddSeconds((recepcionRespuesta - anclaStopwatch) /
                        (double)Stopwatch.Frequency);
                    // Ignore stale responses from slower concurrent requests.
                    if (utcEstimado < utcActual.AddSeconds(-2)) return;
                }

                anclaUtc = DateTime.SpecifyKind(utcEstimado, DateTimeKind.Utc);
                anclaStopwatch = recepcionRespuesta;
                sincronizado = true;
            }
        }
    }
}
