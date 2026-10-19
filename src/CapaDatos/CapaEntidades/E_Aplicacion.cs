using System;

namespace CapaEntidades
{
    public class E_Aplicacion
    {
        public int IdAplicacion { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Identificador { get; set; }
        public bool EsCritica { get; set; }
    }
}
