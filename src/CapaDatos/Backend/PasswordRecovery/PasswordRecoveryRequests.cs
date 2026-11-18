using System.ComponentModel.DataAnnotations;

namespace Backend.PasswordRecovery;

public sealed record SendPasswordRecoveryRequest(
    [param: Required, EmailAddress, StringLength(254)] string? Email);

public sealed record ConfirmPasswordRecoveryRequest(
    [param: Required, EmailAddress, StringLength(254)] string? Email,
    [param: Required, RegularExpression("^[0-9]{6}$")] string? Code,
    [param: Required, StringLength(255)] string? PasswordHash);
