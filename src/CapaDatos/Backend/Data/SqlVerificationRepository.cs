using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

/// <summary>
/// Calls the verification procedures already defined in database/04_CrearProcedimientos.sql.
/// This service deliberately does not create or migrate database objects.
/// </summary>
public sealed class SqlVerificationRepository(VerificationDbContext db) : IVerificationRepository
{
    public Task<bool> PrepareAsync(string email, string salt, string codeHash, CancellationToken cancellationToken) =>
        ExecuteBooleanAsync("dbo.sp_PrepararVerificacionCorreo", cancellationToken,
            ("@Correo", email, 254), ("@Sal", salt, 24), ("@CodigoHash", codeHash, 64));

    public async Task<string?> GetSaltAsync(string email, CancellationToken cancellationToken)
    {
        await using var command = await CreateCommandAsync("dbo.sp_ConsultarSalVerificacion", cancellationToken);
        command.Parameters.Add(new SqlParameter("@Correo", SqlDbType.VarChar, 254) { Value = email });
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return value is null or DBNull ? null : Convert.ToString(value);
    }

    public Task<bool> ConfirmAsync(string email, string codeHash, CancellationToken cancellationToken) =>
        ExecuteBooleanAsync("dbo.sp_ConfirmarVerificacionCorreo", cancellationToken,
            ("@Correo", email, 254), ("@CodigoHash", codeHash, 64));

    private async Task<bool> ExecuteBooleanAsync(
        string procedure,
        CancellationToken cancellationToken,
        params (string Name, string Value, int Size)[] values)
    {
        await using var command = await CreateCommandAsync(procedure, cancellationToken);
        foreach (var (name, value, size) in values)
            command.Parameters.Add(new SqlParameter(name, SqlDbType.VarChar, size) { Value = value });

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null and not DBNull && Convert.ToBoolean(result);
    }

    private async Task<SqlCommand> CreateCommandAsync(string procedure, CancellationToken cancellationToken)
    {
        var connection = (SqlConnection)db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = procedure;
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 15;
        return command;
    }
}
