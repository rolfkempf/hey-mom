using System.ComponentModel.DataAnnotations;

namespace HeyMom.Api.Features.Authentication.ConfirmEmail;

public sealed record ConfirmEmailRequestDto(
    [property: Required, StringLength(450, MinimumLength = 1)] string UserId,
    [property: Required, StringLength(4096, MinimumLength = 1)] string Code);
