namespace Backend.Email;

public interface IEmailSender
{
    Task SendVerificationCodeAsync(string recipient, string code, CancellationToken cancellationToken);
    Task SendPasswordRecoveryCodeAsync(string recipient, string code, CancellationToken cancellationToken);
}
