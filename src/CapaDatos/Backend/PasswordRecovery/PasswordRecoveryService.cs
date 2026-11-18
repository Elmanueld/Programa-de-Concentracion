using System.Security.Cryptography;
using System.Text;
using Backend.Email;
using Microsoft.Extensions.Options;
using Backend.Verification;

namespace Backend.PasswordRecovery;

public sealed class PasswordRecoveryService(
    IPasswordRecoveryRepository repository,
    IEmailSender emailSender,
    IOptions<VerificationOptions> options) : IPasswordRecoveryService
{
    public async Task SendAsync(string email, CancellationToken cancellationToken)
    {
        email = NormalizeEmail(email);
        var settings = GetSettings();
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        var codeHash = HashCode(settings.HashKey, salt, code);

        var prepared = await repository.PrepareAsync(email, salt, codeHash, cancellationToken);

        if (!prepared)
            return;

        try
        {
            await emailSender.SendPasswordRecoveryCodeAsync(email, code, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new PasswordRecoveryEmailDeliveryException(ex);
        }
    }

    public async Task<bool> ConfirmAsync(
        string email,
        string code,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        email = NormalizeEmail(email);
        if (code.Length != 6 || code.Any(character => character is < '0' or > '9'))
            return false;
        if (!IsValidPasswordHash(passwordHash))
            throw new ArgumentException("El hash de contraseña no tiene un formato permitido.");

        var settings = GetSettings();
        var salt = await repository.GetSaltAsync(email, cancellationToken);
        if (string.IsNullOrEmpty(salt))
            return false;

        return await repository.ConfirmAsync(
            email, HashCode(settings.HashKey, salt, code), passwordHash, cancellationToken);
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

    private static bool IsValidPasswordHash(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var parts = value.Split('$');
        if (parts.Length != 4 || parts[0] != "PBKDF2-SHA256" || parts[1] != "600000") return false;

        try
        {
            return Convert.FromBase64String(parts[2]).Length == 16 &&
                   Convert.FromBase64String(parts[3]).Length == 32;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string NormalizeEmail(string email) => email.Trim();

    private static string HashCode(string secret, string salt, string code)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{salt}:{code}")));
    }
}

public sealed class PasswordRecoveryEmailDeliveryException(Exception innerException)
    : Exception("No se pudo entregar el correo de recuperación.", innerException) { }
