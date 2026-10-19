using System;

namespace CapaEntidades
{
    public class E_Usuario
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; }
        public bool CorreoVerificado { get; set; }
        public string ContrasenaHash { get; set; }
        public bool Eliminado { get; set; }
    }
}
