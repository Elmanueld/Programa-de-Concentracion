using System.Data;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

[ApiController]
[Authorize]
[Route("api/sync/changes")]
public sealed class OfflineChangesController(VerificationDbContext db, ILogger<OfflineChangesController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Apply(ChangeBatch batch, CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("id_usuario")?.Value, out var userId) || userId <= 0)
            return Unauthorized();
        if (batch.Changes is null || batch.Changes.Count > 500 || batch.Changes.Any(c => c.Id <= 0 || !Guid.TryParse(c.SyncId, out _)))
            return BadRequest(new { message = "El lote de sincronización no es válido." });
        var connection = (SqlConnection)db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync(cancellationToken);
        await using (var deviceCommand = connection.CreateCommand())
        {
            deviceCommand.CommandText = @"IF EXISTS(SELECT 1 FROM dbo.SyncDispositivos WHERE IdUsuario=@u AND IdDispositivo=@d)
                UPDATE dbo.SyncDispositivos SET NombreDispositivo=@n WHERE IdUsuario=@u AND IdDispositivo=@d
                ELSE INSERT dbo.SyncDispositivos(IdUsuario,IdDispositivo,NombreDispositivo) VALUES(@u,@d,@n)";
            deviceCommand.Parameters.Add(new SqlParameter("@u", SqlDbType.Int) { Value = userId });
            deviceCommand.Parameters.Add(new SqlParameter("@d", SqlDbType.UniqueIdentifier) { Value = batch.DeviceId });
            deviceCommand.Parameters.Add(new SqlParameter("@n", SqlDbType.NVarChar, 100) { Value = "Cliente WinForms" });
            await deviceCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        var device = batch.DeviceId;
        foreach (var change in batch.Changes.OrderBy(c => c.Id))
        {
            await using var tx = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                await using (var check = Cmd(connection, tx, "SELECT COUNT(*) FROM dbo.SyncCambiosAplicados WHERE IdUsuario=@u AND IdDispositivo=@d AND IdCambioLocal=@c",
                    ("@u", SqlDbType.Int, userId), ("@d", SqlDbType.UniqueIdentifier, device), ("@c", SqlDbType.BigInt, change.Id)))
                {
                    if (Convert.ToInt32(await check.ExecuteScalarAsync(cancellationToken)) == 0)
                    {
                        await ApplyOne(connection, tx, userId, change, cancellationToken);
                        await using var mark = Cmd(connection, tx, "INSERT dbo.SyncCambiosAplicados(IdUsuario,IdDispositivo,IdCambioLocal) VALUES(@u,@d,@c)",
                            ("@u", SqlDbType.Int, userId), ("@d", SqlDbType.UniqueIdentifier, device), ("@c", SqlDbType.BigInt, change.Id));
                        await mark.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
                await tx.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Offline sync failed for account {UserId}, change {ChangeId}, trace {TraceId}", userId, change.Id, HttpContext.TraceIdentifier);
                try
                {
                    await tx.RollbackAsync(CancellationToken.None);
                }
                catch (Exception rollbackException)
                {
                    logger.LogError(rollbackException,
                        "Offline sync transaction rollback failed for account {UserId}, change {ChangeId}, trace {TraceId}",
                        userId, change.Id, HttpContext.TraceIdentifier);
                }
                throw;
            }
        }
        return NoContent();
    }

    private static async Task ApplyOne(SqlConnection cn, SqlTransaction tx, int userId, OfflineChange c, CancellationToken ct)
    {
        var values = Decode(c.Payload);
        var sync = Guid.Parse(c.SyncId);
        if (c.Type is "Tarea" or "Aplicacion" or "Web" or "Descanso")
        {
            await using var tombstone = Cmd(cn, tx,
                "SELECT COUNT(*) FROM dbo.SyncTombstones WHERE IdUsuario=@u AND TipoEntidad=@type AND SyncId=@sync AND EliminadoUtc>=@date",
                ("@u", SqlDbType.Int, userId), ("@type", SqlDbType.VarChar, c.Type),
                ("@sync", SqlDbType.UniqueIdentifier, sync), ("@date", SqlDbType.DateTime2, c.CreatedUtc));
            if (Convert.ToInt32(await tombstone.ExecuteScalarAsync(ct)) > 0) return;
        }
        if (c.Type == "TareaAplicacion" || c.Type == "TareaWeb")
        {
            var app = c.Type == "TareaAplicacion";
            var task = await FindId(cn, tx, userId, "Tareas", "IdTarea", c.RelatedSyncIds[0], ct);
            var entity = await FindId(cn, tx, userId, app ? "Aplicaciones" : "Webs", app ? "IdAplicacion" : "IdWeb", c.RelatedSyncIds[1], ct);
            var table = app ? "Tarea_Aplicacion" : "Tarea_Web";
            var field = app ? "IdAplicacion" : "IdWeb";
            var sql = c.Operation == "D" ? $"DELETE dbo.{table} WHERE IdUsuario=@u AND IdTarea=@task AND {field}=@entity" :
                $"IF NOT EXISTS(SELECT 1 FROM dbo.{table} WHERE IdUsuario=@u AND IdTarea=@task AND {field}=@entity) INSERT dbo.{table}(IdUsuario,IdTarea,{field}) VALUES(@u,@task,@entity)";
            await using var cmd = Cmd(cn, tx, sql, ("@u", SqlDbType.Int, userId), ("@task", SqlDbType.Int, task), ("@entity", SqlDbType.Int, entity));
            await cmd.ExecuteNonQueryAsync(ct); return;
        }
        if (c.Operation == "D")
        {
            var table = c.Type switch { "Tarea" => "Tareas", "Aplicacion" => "Aplicaciones", "Web" => "Webs", "Descanso" => "Descansos", _ => throw new InvalidOperationException("Invalid sync entity") };
            await using var tomb = Cmd(cn, tx, "IF NOT EXISTS(SELECT 1 FROM dbo.SyncTombstones WHERE IdUsuario=@u AND TipoEntidad=@type AND SyncId=@sync) INSERT dbo.SyncTombstones(IdUsuario,TipoEntidad,SyncId,EliminadoUtc) VALUES(@u,@type,@sync,@date)",
                ("@u", SqlDbType.Int, userId), ("@type", SqlDbType.VarChar, c.Type), ("@sync", SqlDbType.UniqueIdentifier, sync), ("@date", SqlDbType.DateTime2, c.CreatedUtc));
            await tomb.ExecuteNonQueryAsync(ct);
            await using var delete = Cmd(cn, tx, $"DELETE dbo.{table} WHERE IdUsuario=@u AND SyncId=@sync AND SyncUpdatedUtc<=@date",
                ("@u", SqlDbType.Int, userId), ("@sync", SqlDbType.UniqueIdentifier, sync), ("@date", SqlDbType.DateTime2, c.CreatedUtc));
            await delete.ExecuteNonQueryAsync(ct); return;
        }
        if (c.Type == "Configuracion")
        {
            await using var cmd = Cmd(cn, tx, "IF EXISTS(SELECT 1 FROM dbo.Configuraciones WHERE IdUsuario=@u) UPDATE dbo.Configuraciones SET AutoSesion=@v,SyncUpdatedUtc=@date WHERE IdUsuario=@u AND SyncUpdatedUtc<=@date ELSE INSERT dbo.Configuraciones(IdUsuario,AutoSesion,SyncId,SyncUpdatedUtc) VALUES(@u,@v,@sync,@date)",
                ("@u", SqlDbType.Int, userId), ("@v", SqlDbType.Bit, B(values[0])), ("@date", SqlDbType.DateTime2, c.CreatedUtc), ("@sync", SqlDbType.UniqueIdentifier, sync));
            await cmd.ExecuteNonQueryAsync(ct); return;
        }
        if (c.Type == "Tarea" && values.Length == 2 && values[0] == "Estado")
        {
            await using var cmd = Cmd(cn, tx, "UPDATE dbo.Tareas SET Estado=@v,SyncUpdatedUtc=@date WHERE IdUsuario=@u AND SyncId=@sync AND SyncUpdatedUtc<=@date",
                ("@v", SqlDbType.Char, values[1]), ("@date", SqlDbType.DateTime2, c.CreatedUtc), ("@u", SqlDbType.Int, userId), ("@sync", SqlDbType.UniqueIdentifier, sync));
            await cmd.ExecuteNonQueryAsync(ct); return;
        }
        string sqlUpsert = c.Type switch
        {
            "Tarea" => "IF EXISTS(SELECT 1 FROM dbo.Tareas WHERE IdUsuario=@u AND SyncId=@sync) UPDATE dbo.Tareas SET Nombre=@a,FechaInicio=@b,FechaFin=@c,Estado=@d,EsPlantilla=@e,NoBloquearApps=@f,NoBloquearWebs=@g,SyncUpdatedUtc=@date WHERE IdUsuario=@u AND SyncId=@sync AND SyncUpdatedUtc<=@date ELSE INSERT dbo.Tareas(IdUsuario,Nombre,FechaInicio,FechaFin,Estado,EsPlantilla,NoBloquearApps,NoBloquearWebs,SyncId,SyncUpdatedUtc) VALUES(@u,@a,@b,@c,@d,@e,@f,@g,@sync,@date)",
            "Aplicacion" => "IF EXISTS(SELECT 1 FROM dbo.Aplicaciones WHERE IdUsuario=@u AND SyncId=@sync) UPDATE dbo.Aplicaciones SET Nombre=@a,Identificador=@b,EsCritica=@c,SyncUpdatedUtc=@date WHERE IdUsuario=@u AND SyncId=@sync AND SyncUpdatedUtc<=@date ELSE IF NOT EXISTS(SELECT 1 FROM dbo.Aplicaciones WHERE IdUsuario=@u AND Identificador=@b) INSERT dbo.Aplicaciones(IdUsuario,Nombre,Identificador,EsCritica,SyncId,SyncUpdatedUtc) VALUES(@u,@a,@b,@c,@sync,@date)",
            "Web" => "IF EXISTS(SELECT 1 FROM dbo.Webs WHERE IdUsuario=@u AND SyncId=@sync) UPDATE dbo.Webs SET Nombre=@a,Link=@b,SyncUpdatedUtc=@date WHERE IdUsuario=@u AND SyncId=@sync AND SyncUpdatedUtc<=@date ELSE IF NOT EXISTS(SELECT 1 FROM dbo.Webs WHERE IdUsuario=@u AND HashLink=CONVERT(VARBINARY(32),HASHBYTES('SHA2_256',@b))) INSERT dbo.Webs(IdUsuario,Nombre,Link,SyncId,SyncUpdatedUtc) VALUES(@u,@a,@b,@sync,@date)",
            "Descanso" => "DECLARE @task INT=(SELECT IdTarea FROM dbo.Tareas WHERE IdUsuario=@u AND SyncId=@taskSync); IF EXISTS(SELECT 1 FROM dbo.Descansos WHERE IdUsuario=@u AND SyncId=@sync) UPDATE dbo.Descansos SET IdTarea=@task,IntervaloDescanso=TRY_CONVERT(int,@a),DuracionDescanso=TRY_CONVERT(int,@b),SyncUpdatedUtc=@date WHERE IdUsuario=@u AND SyncId=@sync AND SyncUpdatedUtc<=@date ELSE IF EXISTS(SELECT 1 FROM dbo.Descansos WHERE IdUsuario=@u AND IdTarea=@task) UPDATE dbo.Descansos SET IntervaloDescanso=TRY_CONVERT(int,@a),DuracionDescanso=TRY_CONVERT(int,@b),SyncUpdatedUtc=@date WHERE IdUsuario=@u AND IdTarea=@task AND SyncUpdatedUtc<=@date ELSE IF @task IS NOT NULL INSERT dbo.Descansos(IdUsuario,IdTarea,IntervaloDescanso,DuracionDescanso,SyncId,SyncUpdatedUtc) VALUES(@u,@task,TRY_CONVERT(int,@a),TRY_CONVERT(int,@b),@sync,@date)",
            _ => throw new InvalidOperationException("Invalid sync entity")
        };
        await using var upsert = cn.CreateCommand(); upsert.Transaction = tx; upsert.CommandText = sqlUpsert;
        Add(upsert, "@u", SqlDbType.Int, userId); Add(upsert, "@sync", SqlDbType.UniqueIdentifier, sync); Add(upsert, "@date", SqlDbType.DateTime2, c.CreatedUtc);
        if (c.Type == "Descanso") Add(upsert, "@taskSync", SqlDbType.UniqueIdentifier, Guid.Parse(c.RelatedSyncIds[0]));
        if (c.Type == "Descanso")
        {
            Add(upsert, "@a", SqlDbType.NVarChar, values[1]);
            Add(upsert, "@b", SqlDbType.NVarChar, values[2]);
        }
        else
            for (var i = 0; i < values.Length; i++) Add(upsert, "@" + (char)('a' + i), SqlDbType.NVarChar, values[i]);
        await upsert.ExecuteNonQueryAsync(ct);
    }

    private static async Task<int> FindId(SqlConnection cn, SqlTransaction tx, int uid, string table, string field, string sync, CancellationToken ct)
    {
        await using var cmd = Cmd(cn, tx, $"SELECT {field} FROM dbo.{table} WHERE IdUsuario=@u AND SyncId=@sync", ("@u", SqlDbType.Int, uid), ("@sync", SqlDbType.UniqueIdentifier, Guid.Parse(sync)));
        var value = await cmd.ExecuteScalarAsync(ct);
        return value is null or DBNull ? throw new InvalidOperationException("A related synchronized record was not found.") : Convert.ToInt32(value);
    }
    private static SqlCommand Cmd(SqlConnection cn, SqlTransaction tx, string text, params (string, SqlDbType, object)[] values)
    {
        var cmd = cn.CreateCommand(); cmd.Transaction = tx; cmd.CommandText = text;
        foreach (var (name, type, value) in values) Add(cmd, name, type, value);
        return cmd;
    }
    private static void Add(SqlCommand cmd, string name, SqlDbType type, object? value) => cmd.Parameters.Add(new SqlParameter(name, type) { Value = value ?? DBNull.Value });
    private static bool B(string value) => value == "1" || bool.TryParse(value, out var b) && b;
    private static string[] Decode(string? payload) => string.IsNullOrEmpty(payload) ? Array.Empty<string>() : payload.Split('.').Select(v => Encoding.UTF8.GetString(Convert.FromBase64String(v))).ToArray();
}

public sealed record ChangeBatch(Guid DeviceId, List<OfflineChange> Changes);
public sealed record OfflineChange(long Id, string Type, string SyncId, string Operation, string Payload, DateTime CreatedUtc, string[] RelatedSyncIds);
