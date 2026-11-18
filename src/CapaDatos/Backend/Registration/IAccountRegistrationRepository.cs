namespace Backend.Registration;

public interface IAccountRegistrationRepository
{
    Task<int?> RegisterOrGetIdempotentAsync(
        string name,
        string email,
        string passwordHash,
        CancellationToken cancellationToken);
}
