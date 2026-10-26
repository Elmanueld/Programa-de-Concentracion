using System;
using System.Security.Cryptography;

namespace CapaNegocio
{
    public static class SeguridadContrasena
    {
        private const int Iteraciones = 600000;

        public static string CrearHash(string contrasena)
        {
            if (String.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 8 || contrasena.Length > 128)
                throw new ArgumentException("La contraseña debe tener entre 8 y 128 caracteres.");
            byte[] sal = new byte[16];
            using (var generador = RandomNumberGenerator.Create()) generador.GetBytes(sal);
            using (var derivacion = new Rfc2898DeriveBytes(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256))
                return "PBKDF2-SHA256$" + Iteraciones + "$" + Convert.ToBase64String(sal) + "$" +
                    Convert.ToBase64String(derivacion.GetBytes(32));
        }

        public static bool Verificar(string contrasena, string hash)
        {
            if (contrasena == null || contrasena.Length > 128 || hash == null) return false;
            string[] partes = hash.Split('$');
            int iteraciones;
            if (partes.Length != 4 || partes[0] != "PBKDF2-SHA256" ||
                !Int32.TryParse(partes[1], out iteraciones) || iteraciones < 100000 || iteraciones > 1000000)
                return false;
            try
            {
                byte[] sal = Convert.FromBase64String(partes[2]);
                byte[] esperado = Convert.FromBase64String(partes[3]);
                if (sal.Length != 16 || esperado.Length != 32) return false;
                using (var derivacion = new Rfc2898DeriveBytes(contrasena, sal, iteraciones, HashAlgorithmName.SHA256))
                {
                    byte[] actual = derivacion.GetBytes(esperado.Length);
                    int diferencia = 0;
                    for (int i = 0; i < esperado.Length; i++) diferencia |= actual[i] ^ esperado[i];
                    return diferencia == 0;
                }
            }
            catch (FormatException) { return false; }
        }
    }
}
