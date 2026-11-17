using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace CapaNegocio
{
    internal static class CorreoVerificacion
    {
        private static string Leer(string nombre)
        {
            string valor = Environment.GetEnvironmentVariable("PC_" + nombre.ToUpperInvariant());
            return valor ?? ConfigurationManager.AppSettings[nombre] ?? "";
        }

        public static bool EstaConfigurado()
        {
            return !String.IsNullOrWhiteSpace(Leer("SmtpHost")) &&
                !String.IsNullOrWhiteSpace(Leer("SmtpRemitente")) &&
                (String.IsNullOrEmpty(Leer("SmtpUsuario")) ||
                 !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PC_SMTPCLAVE")));
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

        // Nombre que ve el usuario como remitente
        private static string LeerNombreRemitente()
        {
            string nombre = Leer("SmtpNombre");
            return String.IsNullOrWhiteSpace(nombre) ? "Programa de Concentración" : nombre.Trim();
        }

        public static void Enviar(string destino, string codigo)
        {
            ValidarConfiguracion();
            var remitente = new MailAddress(Leer("SmtpRemitente"), LeerNombreRemitente(), Encoding.UTF8);
            using (var mensaje = new MailMessage(remitente, new MailAddress(destino)))
            using (var cliente = new SmtpClient(Leer("SmtpHost"), Int32.Parse(Leer("SmtpPuerto"))))
            {
                mensaje.Subject = "Verifica tu cuenta - Programa de Concentración";
                mensaje.SubjectEncoding = Encoding.UTF8;
                mensaje.Body = CrearHtml(codigo);
                mensaje.BodyEncoding = Encoding.UTF8;
                mensaje.IsBodyHtml = true;
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

        // Correo sencillo: sin imágenes, sin enlaces y sin seguimiento
        internal static string CrearHtml(string codigo)
        {
            string codigoSeguro = WebUtility.HtmlEncode(codigo);
            return "<!DOCTYPE html>" +
                "<html lang=\"es\"><head><meta charset=\"utf-8\">" +
                "<title>Verifica tu cuenta</title></head>" +
                "<body style=\"margin:0;padding:0;background:#f3f5f9;font-family:Segoe UI,Arial,sans-serif;color:#1f2937;\">" +
                "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:#f3f5f9;padding:24px 0;\">" +
                "<tr><td align=\"center\">" +
                "<table role=\"presentation\" width=\"480\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:480px;width:100%;background:#ffffff;border-radius:8px;\">" +
                "<tr><td style=\"background:#2563eb;color:#ffffff;padding:20px 28px;border-radius:8px 8px 0 0;font-size:18px;font-weight:bold;\">" +
                "Programa de Concentración</td></tr>" +
                "<tr><td style=\"padding:28px;\">" +
                "<h1 style=\"margin:0 0 12px 0;font-size:20px;\">Verifica tu cuenta</h1>" +
                "<p style=\"margin:0 0 20px 0;font-size:15px;line-height:22px;\">" +
                "Gracias por registrarte. Escribe este código en la aplicación para confirmar tu correo:</p>" +
                "<p style=\"margin:0 0 20px 0;text-align:center;\">" +
                "<span style=\"display:inline-block;background:#eef2ff;border:1px solid #c7d2fe;border-radius:8px;padding:14px 24px;" +
                "font-family:Consolas,Courier New,monospace;font-size:32px;font-weight:bold;letter-spacing:8px;color:#1e3a8a;\">" +
                codigoSeguro + "</span></p>" +
                "<p style=\"margin:0 0 8px 0;font-size:14px;line-height:21px;\">" +
                "El código vence en <strong>10 minutos</strong> y solo se puede usar una vez.</p>" +
                "<p style=\"margin:0;font-size:13px;line-height:20px;color:#6b7280;\">" +
                "Si no creaste esta cuenta, ignora este mensaje.</p>" +
                "</td></tr></table></td></tr></table></body></html>";
        }
    }
}
