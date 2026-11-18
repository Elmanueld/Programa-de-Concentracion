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
        if (!IsValidPasswordHash(passwordHash))
            throw new ArgumentException("El hash de contraseña no tiene un formato permitido.");

        var idUsuario = await repository.RegisterOrGetIdempotentAsync(
            name.Trim(), email.Trim(), passwordHash, cancellationToken);

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

public sealed class AccountRegistrationConflictException : Exception { }
