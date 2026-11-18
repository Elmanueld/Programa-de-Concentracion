using System.Data;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

[ApiController]
[Authorize]
[Route("api/data/procedure")]
public sealed class ProcedureController(VerificationDbContext db, ILogger<ProcedureController> logger) : ControllerBase
{
    private static readonly HashSet<string> AllowedProcedures = new(StringComparer.OrdinalIgnoreCase)
    {
        "sp_BuscarAplicacion", "sp_BuscarConfiguracion", "sp_BuscarDescanso", "sp_BuscarPlantilla",
        "sp_BuscarTarea", "sp_BuscarTareaAplicacion", "sp_BuscarTareaWeb", "sp_BuscarWeb",
        "sp_CambiarEstadoTarea", "sp_EliminarAplicacion", "sp_EliminarDescanso", "sp_EliminarTarea",
        "sp_EliminarTareaAplicacion", "sp_EliminarTareaWeb", "sp_EliminarUsuario", "sp_EliminarWeb",
        "sp_InsertarAplicacion", "sp_InsertarConfiguracion", "sp_InsertarDescanso", "sp_InsertarTarea",
        "sp_InsertarTareaAplicacion", "sp_InsertarTareaWeb", "sp_InsertarWeb", "sp_ModificarAplicacion",
        "sp_ModificarConfiguracion", "sp_ModificarDescanso", "sp_ModificarTarea", "sp_ModificarUsuario",
        "sp_ModificarWeb", "sp_ObtenerPlantilla", "sp_RecuperarCuenta", "sp_CambiarContrasena"
    };

    [HttpPost]
    public async Task<IActionResult> Execute(ProcedureRequest request, CancellationToken cancellationToken)
    {
        if (request is null || !AllowedProcedures.Contains(request.Name ?? string.Empty) ||
            request.Mode is not ("execute" or "scalar" or "query"))
            return BadRequest(new { message = "La operación solicitada no está permitida." });

        if (!int.TryParse(User.FindFirst("id_usuario")?.Value, out var idUsuario) || idUsuario <= 0)
            return Unauthorized(new { message = "La sesión no es válida." });

        var connection = (SqlConnection)db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "dbo." + request.Name;
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 20;

        var specs = await ReadParametersAsync(connection, request.Name!, cancellationToken);
        if (!specs.Any(p => string.Equals(p.Name, "@IdUsuario", StringComparison.OrdinalIgnoreCase)) && request.Name != "sp_EliminarUsuario")
            return BadRequest(new { message = "El procedimiento no tiene alcance de usuario y no se puede ejecutar por la API." });

        var requested = request.Parameters ?? new Dictionary<string, JsonElement>();
        if (request.Name == "sp_EliminarUsuario")
        {
            await using var deleteAccount = connection.CreateCommand();
            deleteAccount.CommandText = "dbo.sp_EliminarUsuario";
            deleteAccount.CommandType = CommandType.StoredProcedure;
            deleteAccount.Parameters.Add(new SqlParameter("@IdUsuario", SqlDbType.Int) { Value = idUsuario });
            await deleteAccount.ExecuteNonQueryAsync(cancellationToken);
            return NoContent();
        }
        var allowedNames = specs.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (requested.Keys.Any(key => !allowedNames.Contains(NormalizeName(key))))
            return BadRequest(new { message = "Los parámetros enviados no coinciden con el procedimiento." });

        foreach (var spec in specs)
        {
            if (spec.IsOutput) continue;
            if (string.Equals(spec.Name, "@IdUsuario", StringComparison.OrdinalIgnoreCase))
            {
                command.Parameters.Add(new SqlParameter(spec.Name, spec.Type) { Value = idUsuario });
                continue;
            }
            var pair = requested.FirstOrDefault(p => string.Equals(NormalizeName(p.Key), spec.Name, StringComparison.OrdinalIgnoreCase));
            if (pair.Key is null)
            {
                if (!spec.HasDefault) return BadRequest(new { message = "Falta un parámetro requerido para la operación." });
                continue;
            }
            var parameter = new SqlParameter(spec.Name, spec.Type)
            {
                Value = ReadValue(pair.Value, spec.Type) ?? DBNull.Value
            };
            if (spec.Size != 0) parameter.Size = spec.Size;
            command.Parameters.Add(parameter);
        }

        SqlTransaction? transaction = null;
        var transactionCompleted = false;
        try
        {
            if (request.Name is "sp_ModificarTarea" or "sp_EliminarTarea" or "sp_CambiarEstadoTarea")
            {
                transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
                command.Transaction = transaction;
                await ValidateTaskMutationAsync(connection, transaction, command, idUsuario, cancellationToken);
            }

            switch (request.Mode)
            {
                case "execute":
                    await command.ExecuteNonQueryAsync(cancellationToken);
                    if (transaction is not null)
                    {
                        await transaction.CommitAsync(cancellationToken);
                        transactionCompleted = true;
                    }
                    return NoContent();
                case "scalar":
                    var scalar = await command.ExecuteScalarAsync(cancellationToken);
                    if (transaction is not null)
                    {
                        await transaction.CommitAsync(cancellationToken);
                        transactionCompleted = true;
                    }
                    return Ok(scalar is null or DBNull ? null : scalar);
                default:
                    var rows = new List<Dictionary<string, object?>>();
                    await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                            for (var i = 0; i < reader.FieldCount; i++)
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            rows.Add(row);
                        }
                    }
                    if (transaction is not null)
                    {
                        await transaction.CommitAsync(cancellationToken);
                        transactionCompleted = true;
                    }
                    return Ok(rows);
            }
        }
        catch (DataRuleException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (SqlException ex) when (ex.Number is >= 50001 and <= 50019)
        {
            return Conflict(new { message = SafeBusinessMessage(ex.Number) });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Remote data procedure failed. Procedure: {Procedure}; UserId: {UserId}; TraceId: {TraceId}",
                request.Name, idUsuario, HttpContext.TraceIdentifier);
            throw;
        }
        finally
        {
            if (transaction is not null)
            {
                if (!transactionCompleted)
                {
                    try
                    {
                        await transaction.RollbackAsync(CancellationToken.None);
                    }
                    catch (Exception rollbackException)
                    {
                        logger.LogError(rollbackException,
                            "Could not roll back remote data procedure transaction. Procedure: {Procedure}; UserId: {UserId}; TraceId: {TraceId}",
                            request.Name, idUsuario, HttpContext.TraceIdentifier);
                    }
                }
                await transaction.DisposeAsync();
            }
        }
    }

    private static async Task<List<ParameterSpec>> ReadParametersAsync(SqlConnection connection, string procedure, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT p.name, TYPE_NAME(p.user_type_id), p.max_length, p.is_output, p.has_default_value
            FROM sys.parameters p WHERE p.object_id=OBJECT_ID(@ObjectName) AND p.parameter_id>0 ORDER BY p.parameter_id";
        command.Parameters.Add(new SqlParameter("@ObjectName", SqlDbType.NVarChar, 261) { Value = "dbo." + procedure });
        var result = new List<ParameterSpec>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var sqlType = ParseType(reader.GetString(1));
            var maxLength = reader.GetInt16(2);
            if (sqlType is SqlDbType.NChar or SqlDbType.NVarChar) maxLength /= 2;
            result.Add(new ParameterSpec(reader.GetString(0), sqlType, maxLength, reader.GetBoolean(3), reader.GetBoolean(4)));
        }
        return result;
    }

    private static async Task ValidateTaskMutationAsync(SqlConnection connection, SqlTransaction transaction, SqlCommand command, int idUsuario, CancellationToken cancellationToken)
    {
        var taskId = Convert.ToInt32(command.Parameters["@IdTarea"].Value);
        await using var check = connection.CreateCommand();
        check.CommandText = @"SELECT Estado FROM dbo.Tareas WITH (UPDLOCK,HOLDLOCK)
            WHERE IdTarea=@IdTarea AND IdUsuario=@IdUsuario AND EsPlantilla=0
            AND EXISTS(SELECT 1 FROM dbo.Usuarios WHERE IdUsuario=@IdUsuario AND Eliminado=0)";
        check.Transaction = transaction;
        check.Parameters.Add(new SqlParameter("@IdTarea", SqlDbType.Int) { Value = taskId });
        check.Parameters.Add(new SqlParameter("@IdUsuario", SqlDbType.Int) { Value = idUsuario });
        var state = Convert.ToString(await check.ExecuteScalarAsync(cancellationToken));
        if (string.IsNullOrEmpty(state)) throw new DataRuleException("La tarea no existe o no pertenece al usuario.");
        if (command.CommandText.EndsWith("sp_CambiarEstadoTarea", StringComparison.OrdinalIgnoreCase))
        {
            var next = Convert.ToString(command.Parameters["@Estado"].Value);
            if (!((state == "P" && (next == "A" || next == "X")) || (state == "A" && (next == "C" || next == "X"))))
                throw new DataRuleException("La transición de estado no es válida.");
        }
        else if (state is "A" or "X" or "C")
            throw new DataRuleException("Una tarea activa, completada o detenida no se puede editar ni eliminar.");
    }

    private static object? ReadValue(JsonElement value, SqlDbType type)
    {
        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) return null;

        switch (type)
        {
            case SqlDbType.UniqueIdentifier:
                return Guid.Parse(value.GetString() ?? throw new ArgumentException("GUID no válido."));
            case SqlDbType.Date:
            case SqlDbType.DateTime:
            case SqlDbType.DateTime2:
                return DateTime.Parse(value.GetString() ?? throw new ArgumentException("Fecha no válida."),
                    CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            case SqlDbType.Bit:
                return value.ValueKind switch
                {
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Number => value.GetInt32() != 0,
                    JsonValueKind.String => bool.Parse(value.GetString()!),
                    _ => throw new ArgumentException("Valor booleano no válido.")
                };
            case SqlDbType.Int: return value.ValueKind == JsonValueKind.String ? int.Parse(value.GetString()!, CultureInfo.InvariantCulture) : value.GetInt32();
            case SqlDbType.BigInt: return value.ValueKind == JsonValueKind.String ? long.Parse(value.GetString()!, CultureInfo.InvariantCulture) : value.GetInt64();
            case SqlDbType.SmallInt: return value.ValueKind == JsonValueKind.String ? short.Parse(value.GetString()!, CultureInfo.InvariantCulture) : value.GetInt16();
            case SqlDbType.TinyInt: return value.ValueKind == JsonValueKind.String ? byte.Parse(value.GetString()!, CultureInfo.InvariantCulture) : value.GetByte();
            case SqlDbType.Decimal:
            case SqlDbType.Money:
            case SqlDbType.SmallMoney:
                return value.ValueKind == JsonValueKind.String ? decimal.Parse(value.GetString()!, CultureInfo.InvariantCulture) : value.GetDecimal();
            case SqlDbType.Float: return value.ValueKind == JsonValueKind.String ? double.Parse(value.GetString()!, CultureInfo.InvariantCulture) : value.GetDouble();
            case SqlDbType.Real: return value.ValueKind == JsonValueKind.String ? float.Parse(value.GetString()!, CultureInfo.InvariantCulture) : value.GetSingle();
            case SqlDbType.VarBinary:
            case SqlDbType.Binary:
                return Convert.FromBase64String(value.GetString() ?? string.Empty);
            default:
                return value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString(),
                    JsonValueKind.Number when value.TryGetInt64(out var n) => n,
                    JsonValueKind.Number => value.GetDecimal(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => throw new ArgumentException("Parámetro JSON inválido.")
                };
        }
    }

    private static string NormalizeName(string name) => name.StartsWith('@') ? name : "@" + name;

    private static SqlDbType ParseType(string type) => type.ToLowerInvariant() switch
    {
        "varchar" => SqlDbType.VarChar, "nvarchar" => SqlDbType.NVarChar, "char" => SqlDbType.Char,
        "nchar" => SqlDbType.NChar, "int" => SqlDbType.Int, "bigint" => SqlDbType.BigInt,
        "smallint" => SqlDbType.SmallInt, "tinyint" => SqlDbType.TinyInt, "bit" => SqlDbType.Bit,
        "datetime" => SqlDbType.DateTime, "datetime2" => SqlDbType.DateTime2, "date" => SqlDbType.Date,
        "uniqueidentifier" => SqlDbType.UniqueIdentifier, "decimal" => SqlDbType.Decimal,
        "float" => SqlDbType.Float, "real" => SqlDbType.Real,
        "money" => SqlDbType.Money, "smallmoney" => SqlDbType.SmallMoney, "varbinary" => SqlDbType.VarBinary,
        _ => throw new InvalidOperationException("El procedimiento contiene un tipo de parámetro no permitido.")
    };

    private static string SafeBusinessMessage(int number) => number switch
    {
        50001 => "La cuenta no está disponible para esta operación.",
        50002 or 50003 => "La tarea no existe, no pertenece a la cuenta o su estado impide el cambio.",
        _ => "La base de datos rechazó la operación. Revisa los datos y vuelve a intentarlo."
    };

    private sealed record ParameterSpec(string Name, SqlDbType Type, int Size, bool IsOutput, bool HasDefault);
}

public sealed record ProcedureRequest(string Name, string Mode, Dictionary<string, JsonElement>? Parameters);

internal sealed class DataRuleException(string message) : Exception(message);
