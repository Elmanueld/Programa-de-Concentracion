using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace CapaNegocio
{
    internal static class CorreoVerificacion
    {
        private static string Leer(string nombre)
        {
            string valor = Environment.GetEnvironmentVariable("PC_" + nombre.ToUpperInvariant());
            return valor ?? ConfigurationManager.AppSettings[nombre] ?? "";
        }

        public static void ValidarConfiguracion()
        {
            string host = Leer("SmtpHost");
            int puerto;
            bool ssl;
            if (String.IsNullOrWhiteSpace(host) ||
                !Int32.TryParse(Leer("SmtpPuerto"), out puerto) || puerto < 1 || puerto > 65535 ||
                !Boolean.TryParse(Leer("SmtpSsl"), out ssl) ||
                String.IsNullOrWhiteSpace(Leer("SmtpRemitente")))
                throw new InvalidOperationException("Falta configurar el servidor SMTP y el remitente.");
            if (!ssl && host != "localhost" && host != "127.0.0.1" && host != "::1")
                throw new InvalidOperationException("El servidor SMTP debe utilizar TLS.");
            try { new MailAddress(Leer("SmtpRemitente")); }
            catch (FormatException) { throw new InvalidOperationException("El remitente SMTP no es válido."); }
            if (!String.IsNullOrEmpty(Leer("SmtpUsuario")) &&
                String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PC_SMTPCLAVE")))
                throw new InvalidOperationException("Falta la contraseña SMTP en PC_SMTPCLAVE.");
        }

        public static void Enviar(string destino, string codigo)
        {
            ValidarConfiguracion();
            using (var mensaje = new MailMessage(Leer("SmtpRemitente"), destino))
            using (var cliente = new SmtpClient(Leer("SmtpHost"), Int32.Parse(Leer("SmtpPuerto"))))
            {
                mensaje.Subject = "Código de verificación — Programa de Concentración";
                mensaje.Body = "Su código es: " + codigo +
                    "\r\nVence en 10 minutos. Si no creó esta cuenta, ignore este mensaje.";
                cliente.EnableSsl = Boolean.Parse(Leer("SmtpSsl"));
                cliente.UseDefaultCredentials = false;
                cliente.DeliveryMethod = SmtpDeliveryMethod.Network;
                cliente.Timeout = 15000;
                if (!String.IsNullOrEmpty(Leer("SmtpUsuario")))
                    cliente.Credentials = new NetworkCredential(Leer("SmtpUsuario"),
                        Environment.GetEnvironmentVariable("PC_SMTPCLAVE"));
                cliente.Send(mensaje);
            }
        }
    }
}
