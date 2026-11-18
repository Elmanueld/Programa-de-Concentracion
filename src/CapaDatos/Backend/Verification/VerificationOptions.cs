namespace Backend.Verification;

public sealed class VerificationOptions
{
    public int CodeLifetimeMinutes { get; set; } = 10;
    public string HashKey { get; set; } = string.Empty;
}
