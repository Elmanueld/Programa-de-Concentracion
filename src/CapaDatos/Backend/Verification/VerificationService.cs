using System.Security.Cryptography;
using System.Text;
using Backend.Data;
using Backend.Email;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Backend.Verification;

public sealed class VerificationService(
    IVerificationRepository repository,
    IEmailSender emailSender,
    IOptions<VerificationOptions> options) : IVerificationService
{
    public async Task SendAsync(string email, CancellationToken cancellationToken)
    {
        email = NormalizeEmail(email);
        var settings = GetSettings();
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);
        var hash = HashCode(settings.HashKey, salt, code);

        bool prepared;
        try
        {
            prepared = await repository.PrepareAsync(email, salt, hash, cancellationToken);
        }
        catch (SqlException ex) when (ex.Number == 50006)
        {
            throw new VerificationRateLimitException();
        }

        if (!prepared)
            return;

        try
        {
            await emailSender.SendVerificationCodeAsync(email, code, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new EmailDeliveryException(ex);
        }
    }

    public async Task<bool> ConfirmAsync(string email, string code, CancellationToken cancellationToken)
    {
        email = NormalizeEmail(email);
        if (code.Length != 6 || code.Any(character => character is < '0' or > '9'))
            return false;

        var settings = GetSettings();
        var salt = await repository.GetSaltAsync(email, cancellationToken);
        if (string.IsNullOrEmpty(salt))
            return false;

        return await repository.ConfirmAsync(email, HashCode(settings.HashKey, salt, code), cancellationToken);
    }

    private VerificationOptions GetSettings()
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.HashKey))
            throw new InvalidOperationException("Falta configuración obligatoria: Verification__HashKey.");
        if (settings.CodeLifetimeMinutes != 10)
            throw new InvalidOperationException("La configuración Verification:CodeLifetimeMinutes debe ser 10.");
        return settings;
    }

    private static string NormalizeEmail(string email) => email.Trim();

    private static string HashCode(string secret, string salt, string code)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{salt}:{code}")));
    }
}

public sealed class VerificationRateLimitException : Exception { }
public sealed class EmailDeliveryException(Exception innerException)
    : Exception("No se pudo entregar el correo de verificación.", innerException) { }
