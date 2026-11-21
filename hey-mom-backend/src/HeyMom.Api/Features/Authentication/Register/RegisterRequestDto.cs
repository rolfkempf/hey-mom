using System.ComponentModel.DataAnnotations;

namespace HeyMom.Api.Features.Authentication.Register;

public sealed record RegisterRequestDto(
    [property: Required, EmailAddress, StringLength(256)] string Email,
    [property: Required, StringLength(256, MinimumLength = 6)] string Password);
