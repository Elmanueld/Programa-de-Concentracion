namespace Backend.Registration;

public interface IAccountRegistrationService
{
    Task<int> RegisterAsync(string name, string email, string passwordHash, CancellationToken cancellationToken);
}
