using System;

namespace CapaEntidades
{
    public class E_Aplicacion
    {
        public int IdAplicacion { get; set; }
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Identificador { get; set; }
        public bool EsCritica { get; set; }
        public string SyncId { get; set; }
        public string RutaIcono { get; set; }
        public int IndiceIcono { get; set; }

    }

}
