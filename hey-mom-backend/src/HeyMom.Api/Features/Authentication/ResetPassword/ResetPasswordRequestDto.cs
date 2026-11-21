using System.ComponentModel.DataAnnotations;

namespace HeyMom.Api.Features.Authentication.ResetPassword;

public sealed record ResetPasswordRequestDto(
    [property: Required, EmailAddress, StringLength(256)] string Email,
    [property: Required, StringLength(4096, MinimumLength = 1)] string Code,
    [property: Required, StringLength(256, MinimumLength = 6)] string NewPassword);
