namespace Backend.PasswordRecovery;

public interface IPasswordRecoveryRepository
{
    Task<bool> PrepareAsync(string email, string salt, string codeHash, CancellationToken cancellationToken);
    Task<string?> GetSaltAsync(string email, CancellationToken cancellationToken);
    Task<bool> ConfirmAsync(
        string email,
        string codeHash,
        string passwordHash,
        CancellationToken cancellationToken);
}
