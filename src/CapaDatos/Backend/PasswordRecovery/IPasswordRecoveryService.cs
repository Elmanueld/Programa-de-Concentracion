namespace Backend.PasswordRecovery;

public interface IPasswordRecoveryService
{
    Task SendAsync(string email, CancellationToken cancellationToken);
    Task<bool> ConfirmAsync(
        string email,
        string code,
        string passwordHash,
        CancellationToken cancellationToken);
}
