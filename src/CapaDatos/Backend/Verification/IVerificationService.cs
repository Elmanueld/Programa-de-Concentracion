namespace Backend.Verification;

public interface IVerificationService
{
    Task SendAsync(string email, CancellationToken cancellationToken);
    Task<bool> ConfirmAsync(string email, string code, CancellationToken cancellationToken);
}
