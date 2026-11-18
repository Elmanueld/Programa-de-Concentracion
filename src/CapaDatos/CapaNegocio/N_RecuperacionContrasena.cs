using System;

namespace CapaNegocio
{
    public sealed class N_RecuperacionContrasena
    {
        public bool EstaConfigurado()
        {
            return ClienteApiVerificacion.EstaConfigurado();
        }

        public Exception SolicitarCodigo(string correo)
        {
            try
            {
                N_Usuario.ValidarCorreo(correo);
                ClienteApiVerificacion.SolicitarCodigoRecuperacion(correo.Trim());
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public Exception RestablecerContrasena(string correo, string codigo, string nuevaContrasena)
        {
            try
            {
                N_Usuario.ValidarCorreo(correo);
                codigo = (codigo ?? String.Empty).Trim();
                if (codigo.Length != 6)
                    return new ArgumentException("Introduzca el código de seis dígitos.");
                foreach (char digito in codigo)
                    if (digito < '0' || digito > '9')
                        return new ArgumentException("El código debe contener solo números.");

                string hash = SeguridadContrasena.CrearHash(nuevaContrasena);
                ClienteApiVerificacion.ConfirmarRecuperacion(correo.Trim(), codigo, hash);
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}
