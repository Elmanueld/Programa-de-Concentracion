using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

[ApiController]
[Authorize]
[Route("api/sync")]
public sealed class OfflineSnapshotController(VerificationDbContext db) : ControllerBase
{
    [HttpGet("snapshot")]
    public async Task<IActionResult> Snapshot(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("id_usuario")?.Value, out var idUsuario) || idUsuario <= 0)
            return Unauthorized(new { message = "La sesión no es válida." });
        var connection = (SqlConnection)db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        var result = new Dictionary<string, List<Dictionary<string, object?>>>
        {
            ["configuraciones"] = await ReadAsync(connection, transaction,
                "SELECT IdConfiguracion,AutoSesion,CONVERT(varchar(36),SyncId) SyncId,SyncUpdatedUtc FROM dbo.Configuraciones WHERE IdUsuario=@IdUsuario", idUsuario, cancellationToken),
            ["tareas"] = await ReadAsync(connection, transaction,
                "SELECT IdTarea,Nombre,FechaInicio,FechaFin,Estado,EsPlantilla,NoBloquearApps,NoBloquearWebs,CONVERT(varchar(36),SyncId) SyncId,SyncUpdatedUtc FROM dbo.Tareas WHERE IdUsuario=@IdUsuario", idUsuario, cancellationToken),
            ["aplicaciones"] = await ReadAsync(connection, transaction,
                "SELECT IdAplicacion,Nombre,Identificador,EsCritica,CONVERT(varchar(36),SyncId) SyncId,SyncUpdatedUtc FROM dbo.Aplicaciones WHERE IdUsuario=@IdUsuario", idUsuario, cancellationToken),
            ["webs"] = await ReadAsync(connection, transaction,
                "SELECT IdWeb,Nombre,Link,CONVERT(varchar(36),SyncId) SyncId,SyncUpdatedUtc FROM dbo.Webs WHERE IdUsuario=@IdUsuario", idUsuario, cancellationToken),
            ["descansos"] = await ReadAsync(connection, transaction,
                "SELECT IdDescanso,IdTarea,IntervaloDescanso,DuracionDescanso,CONVERT(varchar(36),SyncId) SyncId,SyncUpdatedUtc FROM dbo.Descansos WHERE IdUsuario=@IdUsuario", idUsuario, cancellationToken),
            ["tareaAplicacion"] = await ReadAsync(connection, transaction,
                "SELECT IdTarea,IdAplicacion,SyncUpdatedUtc FROM dbo.Tarea_Aplicacion WHERE IdUsuario=@IdUsuario", idUsuario, cancellationToken),
            ["tareaWeb"] = await ReadAsync(connection, transaction,
                "SELECT IdTarea,IdWeb,SyncUpdatedUtc FROM dbo.Tarea_Web WHERE IdUsuario=@IdUsuario", idUsuario, cancellationToken)
        };
        await transaction.CommitAsync(cancellationToken);
        return Ok(result);
    }

    private static async Task<List<Dictionary<string, object?>>> ReadAsync(
        SqlConnection connection, SqlTransaction transaction, string sql, int idUsuario, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.Parameters.Add(new SqlParameter("@IdUsuario", SqlDbType.Int) { Value = idUsuario });
        var rows = new List<Dictionary<string, object?>>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++) row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            rows.Add(row);
        }
        return rows;
    }
}
