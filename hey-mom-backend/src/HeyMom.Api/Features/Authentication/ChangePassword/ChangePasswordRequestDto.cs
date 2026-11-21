using System.ComponentModel.DataAnnotations;

namespace HeyMom.Api.Features.Authentication.ChangePassword;

public sealed record ChangePasswordRequestDto(
    [property: Required, StringLength(256)] string CurrentPassword,
    [property: Required, StringLength(256, MinimumLength = 6)] string NewPassword);
