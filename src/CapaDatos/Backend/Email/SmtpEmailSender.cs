using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;

namespace Backend.Email;

public sealed class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    public async Task SendVerificationCodeAsync(string recipient, string code, CancellationToken cancellationToken)
    {
        await SendCodeAsync(
            recipient,
            "Verifica tu correo - Programa de Concentración",
            $"Tu código de verificación es {code}. Vence en 10 minutos y solo se puede usar una vez.",
            cancellationToken);
    }

    public async Task SendPasswordRecoveryCodeAsync(string recipient, string code, CancellationToken cancellationToken)
    {
        await SendCodeAsync(
            recipient,
            "Recupera tu contraseña - Programa de Concentración",
            $"Tu código para restablecer la contraseña es {code}. Vence en 10 minutos y solo se puede usar una vez. Si no solicitaste este cambio, ignora este mensaje.",
            cancellationToken);
    }

    private async Task SendCodeAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.Host) || settings.Port != 465 ||
            string.IsNullOrWhiteSpace(settings.FromEmail) ||
            string.IsNullOrWhiteSpace(settings.Username) || string.IsNullOrWhiteSpace(settings.Password))
            throw new InvalidOperationException("La configuración SMTP está incompleta.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.FromName, settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        client.Timeout = 15000;
        await client.ConnectAsync(settings.Host, settings.Port, SecureSocketOptions.SslOnConnect, cancellationToken);
        await client.AuthenticateAsync(settings.Username, settings.Password, cancellationToken);
        await client.SendAsync(FormatOptions.Default, new MailKit.SendRequest(message), cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
