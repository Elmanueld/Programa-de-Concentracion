using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Backend.Data;

namespace Backend.Registration;

public sealed class SqlAccountRegistrationRepository(VerificationDbContext db)
    : IAccountRegistrationRepository
{
    public async Task<int?> RegisterOrGetIdempotentAsync(
        string name,
        string email,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        var connection = (SqlConnection)db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.sp_RegistrarUsuario";
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = 15;
            command.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.VarChar, 50) { Value = name });
            command.Parameters.Add(new SqlParameter("@Correo", SqlDbType.VarChar, 254) { Value = email });
            command.Parameters.Add(new SqlParameter("@ContrasenaHash", SqlDbType.VarChar, 255) { Value = passwordHash });
            var result = await command.ExecuteScalarAsync(cancellationToken);
            if (result is null or DBNull)
                throw new InvalidOperationException(
                    "sp_RegistrarUsuario no devolvió el IdUsuario esperado. Compruebe la migración de registro remoto.");
            return Convert.ToInt32(result);
        }
        catch (SqlException ex) when (ex.Number == 50019)
        {
            return null;
        }
    }
}
