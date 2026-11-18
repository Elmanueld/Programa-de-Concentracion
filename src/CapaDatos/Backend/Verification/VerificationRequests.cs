using System.ComponentModel.DataAnnotations;

namespace Backend.Verification;

public sealed record SendVerificationRequest(
    [param: Required, EmailAddress, StringLength(254)] string? Email);

public sealed record ConfirmVerificationRequest(
    [param: Required, EmailAddress, StringLength(254)] string? Email,
    [param: Required, RegularExpression("^[0-9]{6}$")] string? Code);
