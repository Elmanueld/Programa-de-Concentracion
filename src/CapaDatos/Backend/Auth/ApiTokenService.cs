using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Security.Claims;

namespace Backend.Auth;

public sealed class ApiTokenService(IConfiguration configuration)
{
    private readonly byte[] signingKey = Encoding.UTF8.GetBytes(
        configuration["Authentication:TokenSigningKey"] ?? string.Empty);

    public string Create(int idUsuario)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(new TokenPayload(idUsuario,
            DateTimeOffset.UtcNow.AddHours(12).ToUnixTimeSeconds()));
        var data = Base64Url(payload);
        using var hmac = new HMACSHA256(signingKey);
        return data + "." + Base64Url(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)));
    }

    public ClaimsPrincipal? Validate(string? token)
    {
        if (signingKey.Length < 32 || string.IsNullOrWhiteSpace(token)) return null;
        var parts = token.Split('.');
        if (parts.Length != 2) return null;
        try
        {
            using var hmac = new HMACSHA256(signingKey);
            var expected = hmac.ComputeHash(Encoding.UTF8.GetBytes(parts[0]));
            var supplied = FromBase64Url(parts[1]);
            if (!CryptographicOperations.FixedTimeEquals(expected, supplied)) return null;
            var payload = JsonSerializer.Deserialize<TokenPayload>(FromBase64Url(parts[0]));
            if (payload is null || payload.IdUsuario <= 0 || payload.Expires <= DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return null;
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, payload.IdUsuario.ToString()),
                new Claim("id_usuario", payload.IdUsuario.ToString())
            }, "Bearer");
            return new ClaimsPrincipal(identity);
        }
        catch (FormatException) { return null; }
        catch (JsonException) { return null; }
    }

    private static string Base64Url(byte[] value) => Convert.ToBase64String(value)
        .TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] FromBase64Url(string value)
    {
        var normalized = value.Replace('-', '+').Replace('_', '/');
        normalized += new string('=', (4 - normalized.Length % 4) % 4);
        return Convert.FromBase64String(normalized);
    }

    private sealed record TokenPayload(int IdUsuario, long Expires);
}
