using System;
using System.Net.Http;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class N_VerificacionCorreo
    {
        private readonly N_Usuario usuarios = new N_Usuario();

        public bool EstaConfigurado()
        {
            return ClienteApiVerificacion.EstaConfigurado();
        }

        public Exception SolicitarCodigo(string correo)
        {
            bool hayRegistroLocal = false;
            int? idRegistroLocal = null;

            try
            {
                correo = (correo ?? "").Trim();
                N_Usuario.ValidarCorreo(correo);
                hayRegistroLocal = usuarios.HayRegistroPendiente(correo);
                idRegistroLocal = usuarios.PrepararRegistroRemoto(correo);
                ClienteApiVerificacion.SolicitarCodigo(correo);
                if (idRegistroLocal.HasValue)
                    usuarios.CompletarRegistroRemoto(idRegistroLocal.Value);
                return null;
            }
            catch (HttpRequestException ex)
            {
                return ErrorConexionPendiente(hayRegistroLocal, ex);
            }
            catch (ApiVerificacionException ex)
            {
                if (hayRegistroLocal && ((int)ex.StatusCode == 429 || (int)ex.StatusCode >= 500))
                    return new RegistroRemotoPendienteException("El registro local sigue pendiente. " + ex.Message, ex);
                if (hayRegistroLocal)
                    return new InvalidOperationException("El registro local sigue pendiente. " + ex.Message, ex);
                return ex;
            }
            catch (TaskCanceledException ex)
            {
                return ErrorConexionPendiente(hayRegistroLocal, ex);
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public Exception ConfirmarCodigo(string correo, string codigo)
        {
            try
            {
                correo = (correo ?? "").Trim();
                N_Usuario.ValidarCorreo(correo);
                codigo = (codigo ?? "").Trim();
                if (codigo.Length != 6)
                    return new ArgumentException("Introduzca el código de seis dígitos.");
                foreach (char digito in codigo)
                    if (digito < '0' || digito > '9')
                        return new ArgumentException("El código debe contener solo números.");

                ClienteApiVerificacion.ConfirmarCodigo(correo, codigo);
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private static Exception ErrorConexionPendiente(bool hayRegistroLocal, Exception causa)
        {
            if (hayRegistroLocal)
                return new RegistroRemotoPendienteException(
                    "El registro quedó guardado en SQLite y pendiente de conexión.", causa);
            return new InvalidOperationException(
                "No se pudo conectar con el servicio de verificación. Comprueba Internet e inténtalo de nuevo.", causa);
        }

    }

    public sealed class RegistroRemotoPendienteException : InvalidOperationException
    {
        public RegistroRemotoPendienteException(string mensaje, Exception causa) : base(mensaje, causa) { }
    }
}
