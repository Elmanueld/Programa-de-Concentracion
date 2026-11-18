using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Backend.PasswordRecovery;

[ApiController]
[Route("api/password-recovery")]
public sealed class PasswordRecoveryController(IPasswordRecoveryService service) : ControllerBase
{
    [HttpPost("send")]
    [EnableRateLimiting("password-recovery-send")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Send(
        SendPasswordRecoveryRequest request,
        CancellationToken cancellationToken)
    {
        await service.SendAsync(request.Email!, cancellationToken);
        return Accepted(new { message = "Si la cuenta existe, recibirá un código para recuperar la contraseña." });
    }

    [HttpPost("confirm")]
    [EnableRateLimiting("password-recovery-confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Confirm(
        ConfirmPasswordRecoveryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await service.ConfirmAsync(
                request.Email!, request.Code!, request.PasswordHash!, cancellationToken);
            return updated
                ? NoContent()
                : BadRequest(new { message = "El código no es válido, venció o ya no puede usarse." });
        }
        catch (ArgumentException)
        {
            return BadRequest(new { message = "Los datos para recuperar la contraseña no son válidos." });
        }
    }
}
