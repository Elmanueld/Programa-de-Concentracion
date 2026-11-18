using System.Data;
using System.Security.Cryptography;
using System.Text;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Backend.Auth;

[ApiController]
[Route("api/accounts")]
public sealed class SessionController(VerificationDbContext db, ApiTokenService tokens) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            (string.IsNullOrEmpty(request.Password) && string.IsNullOrEmpty(request.PasswordHash)))
            return BadRequest(new { message = "Escribe el correo y la contraseña." });

        var connection = (SqlConnection)db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.sp_IniciarSesion";
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 15;
        command.Parameters.Add(new SqlParameter("@Correo", SqlDbType.VarChar, 254) { Value = request.Email.Trim() });
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return Unauthorized(new { message = "Correo o contraseña incorrectos, o la cuenta no está verificada." });

        var storedHash = Convert.ToString(reader["ContrasenaHash"]);
        var isValid = request.PasswordHash is not null
            ? FixedTimeEquals(request.PasswordHash, storedHash)
            : VerifyPassword(request.Password!, storedHash);
        if (!isValid)
            return Unauthorized(new { message = "Correo o contraseña incorrectos, o la cuenta no está verificada." });

        var idUsuario = Convert.ToInt32(reader["IdUsuario"]);
        var name = Convert.ToString(reader["Nombre"]);
        var email = Convert.ToString(reader["Correo"]);
        return Ok(new LoginResponse(tokens.Create(idUsuario), idUsuario, name, email));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("id_usuario")?.Value, out var idUsuario) || idUsuario <= 0)
            return Unauthorized(new { message = "La sesión no es válida." });
        var connection = (SqlConnection)db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Nombre,Correo FROM dbo.Usuarios WHERE IdUsuario=@IdUsuario AND Eliminado=0 AND CorreoVerificado=1";
        command.Parameters.Add(new SqlParameter("@IdUsuario", SqlDbType.Int) { Value = idUsuario });
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return NotFound(new { message = "La cuenta no está disponible." });
        return Ok(new { idUsuario, name = reader.GetString(0), email = reader.GetString(1) });
    }

    private static bool FixedTimeEquals(string supplied, string? stored)
    {
        if (stored is null) return false;
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(supplied), Encoding.UTF8.GetBytes(stored));
    }

    private static bool VerifyPassword(string password, string? storedHash)
    {
        if (storedHash is null) return false;
        var parts = storedHash.Split('$');
        if (parts.Length != 4 || parts[0] != "PBKDF2-SHA256" ||
            !int.TryParse(parts[1], out var iterations) || iterations is < 100000 or > 1000000) return false;
        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            if (salt.Length != 16 || expected.Length != 32) return false;
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
    }
}

public sealed record LoginRequest(string? Email, string? Password, string? PasswordHash = null);
public sealed record LoginResponse(string AccessToken, int IdUsuario, string? Name, string? Email);
