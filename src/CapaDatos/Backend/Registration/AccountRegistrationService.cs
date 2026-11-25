namespace Backend.Registration;

public sealed class AccountRegistrationService(IAccountRegistrationRepository repository)
    : IAccountRegistrationService
{
    public async Task<int> RegisterAsync(
        string name,
        string email,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        var normalizedName = name?.Trim().Normalize(System.Text.NormalizationForm.FormC);
        if (string.IsNullOrWhiteSpace(normalizedName))
            throw new InvalidUsernameException("El nombre de usuario es obligatorio.");
        if (normalizedName.Length > 50)
            throw new InvalidUsernameException("El nombre de usuario no puede exceder 50 caracteres.");
        if (!UserNameRules.IsValid(normalizedName))
            throw new InvalidUsernameException("El nombre de usuario no puede contener caracteres especiales.");
        if (!IsValidPasswordHash(passwordHash))
            throw new ArgumentException("El hash de contraseña no tiene un formato permitido.");

        var idUsuario = await repository.RegisterOrGetIdempotentAsync(
            normalizedName!, email.Trim(), passwordHash, cancellationToken);

        return idUsuario ?? throw new AccountRegistrationConflictException();
    }

    private static bool IsValidPasswordHash(string value)
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
}

public static class UserNameRules
{
    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= 50 &&
        System.Text.RegularExpressions.Regex.IsMatch(value, @"^[\p{L}\p{N}_]+$",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);
}

public sealed class InvalidUsernameException(string message) : ArgumentException(message) { }

public sealed class AccountRegistrationConflictException : Exception { }
