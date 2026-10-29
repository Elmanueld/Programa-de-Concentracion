using System;
using System.Security.Cryptography;
using System.Text;
using CapaDatos;

namespace CapaNegocio
{
    public class N_VerificacionCorreo
    {
        private readonly D_VerificacionCorreo datos = new D_VerificacionCorreo();

        public void SolicitarCodigo(string correo)
        {
            correo = (correo ?? "").Trim();
            N_Usuario.ValidarCorreo(correo);
            CorreoVerificacion.ValidarConfiguracion();
            string codigo = CrearCodigo();
            byte[] bytes = new byte[16];
            using (var generador = RandomNumberGenerator.Create()) generador.GetBytes(bytes);
            string sal = Convert.ToBase64String(bytes);
            if (datos.Preparar(correo, sal, CalcularHash(sal, codigo)))
                CorreoVerificacion.Enviar(correo, codigo);
        }

        public void ConfirmarCodigo(string correo, string codigo)
        {
            correo = (correo ?? "").Trim();
            N_Usuario.ValidarCorreo(correo);
            codigo = (codigo ?? "").Trim();
            if (codigo.Length != 6)
                throw new ArgumentException("Introduzca el código de seis dígitos.");
            foreach (char digito in codigo)
                if (digito < '0' || digito > '9')
                    throw new ArgumentException("El código debe contener solo números.");
            string sal = datos.ConsultarSal(correo);
            if (String.IsNullOrEmpty(sal) || !datos.Confirmar(correo, CalcularHash(sal, codigo)))
                throw new ArgumentException("Código incorrecto, vencido o bloqueado. Solicite uno nuevo.");
        }

        internal static string CrearCodigo()
        {
            byte[] bytes = new byte[4];
            uint numero;
            using (var generador = RandomNumberGenerator.Create())
            {
                do
                {
                    generador.GetBytes(bytes);
                    numero = BitConverter.ToUInt32(bytes, 0);
                } while (numero >= UInt32.MaxValue - UInt32.MaxValue % 1000000);
            }
            return (numero % 1000000).ToString("D6");
        }

        internal static string CalcularHash(string sal, string codigo)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(sal + ":" + codigo));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}
