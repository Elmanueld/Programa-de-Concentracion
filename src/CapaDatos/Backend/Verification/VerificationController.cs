using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Backend.Verification;

[ApiController]
[Route("api/email-verification")]
public sealed class VerificationController(IVerificationService service) : ControllerBase
{
    /// <summary>Requests an email verification code. The response does not reveal whether the email exists.</summary>
    [HttpPost("send")]
    [EnableRateLimiting("send-code")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Send(SendVerificationRequest request, CancellationToken cancellationToken)
    {
        await service.SendAsync(request.Email!, cancellationToken);
        return Accepted(new { message = "Si la cuenta existe y necesita verificación, recibirá un código." });
    }

    /// <summary>Confirms a verification code without returning account details.</summary>
    [HttpPost("confirm")]
    [EnableRateLimiting("confirm-code")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Confirm(ConfirmVerificationRequest request, CancellationToken cancellationToken)
    {
        var confirmed = await service.ConfirmAsync(request.Email!, request.Code!, cancellationToken);
        return confirmed
            ? NoContent()
            : BadRequest(new { message = "El código es incorrecto, venció o está bloqueado." });
    }
}
