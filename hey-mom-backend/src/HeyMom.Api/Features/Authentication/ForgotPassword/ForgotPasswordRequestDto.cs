using System.ComponentModel.DataAnnotations;

namespace HeyMom.Api.Features.Authentication.ForgotPassword;

public sealed record ForgotPasswordRequestDto(
    [property: Required, EmailAddress, StringLength(256)] string Email);
