using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Backend.Registration;

[ApiController]
[Route("api/accounts")]
public sealed class AccountRegistrationController(IAccountRegistrationService service) : ControllerBase
{
    [HttpPost("register")]
    [EnableRateLimiting("register-account")]
    [ProducesResponseType(typeof(RegisterAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Register(
        RegisterAccountRequest request,
        CancellationToken cancellationToken)
    {
        int idUsuario;
        try
        {
            idUsuario = await service.RegisterAsync(
                request.Name!, request.Email!, request.PasswordHash!, cancellationToken);
        }
        catch (AccountRegistrationConflictException)
        {
            return Conflict(new
            {
                message = "Ya existe una cuenta con ese correo, o el registro anterior no coincide. Inicia sesión o recupera la contraseña."
            });
        }
        catch (InvalidUsernameException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (ArgumentException)
        {
            return BadRequest(new { message = "Los datos de registro no son válidos." });
        }

        return Ok(new RegisterAccountResponse(idUsuario));
    }
}

public sealed record RegisterAccountResponse(int IdUsuario);
