using System;

namespace CapaEntidades
{
    public class E_Tarea
    {
        public int IdTarea { get; set; }
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public char Estado { get; set; } = 'P';
        public bool EsPlantilla { get; set; }

        public bool TieneFechasValidas => FechaInicio < FechaFin;

        public bool TieneEstadoValido =>
            Estado == 'P' || Estado == 'A' || Estado == 'C';
    }
}
