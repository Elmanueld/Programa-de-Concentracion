using System.ComponentModel.DataAnnotations;

namespace Backend.Registration;

public sealed record RegisterAccountRequest(
    [param: Required, StringLength(50)] string? Name,
    [param: Required, EmailAddress, StringLength(254)] string? Email,
    [param: Required, StringLength(255)] string? PasswordHash);
