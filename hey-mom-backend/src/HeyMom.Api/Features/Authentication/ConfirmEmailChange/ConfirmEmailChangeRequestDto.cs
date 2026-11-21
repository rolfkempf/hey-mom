using System.ComponentModel.DataAnnotations;

namespace HeyMom.Api.Features.Authentication.ConfirmEmailChange;

public sealed record ConfirmEmailChangeRequestDto(
    [property: Required, StringLength(450, MinimumLength = 1)] string UserId,
    [property: Required, EmailAddress, StringLength(256)] string NewEmail,
    [property: Required, StringLength(4096, MinimumLength = 1)] string Code);
