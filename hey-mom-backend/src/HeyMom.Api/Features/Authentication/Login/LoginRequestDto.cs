using System.ComponentModel.DataAnnotations;

namespace HeyMom.Api.Features.Authentication.Login;

public sealed record LoginRequestDto(
    [property: Required, EmailAddress, StringLength(256)] string Email,
    [property: Required, StringLength(256)] string Password);
