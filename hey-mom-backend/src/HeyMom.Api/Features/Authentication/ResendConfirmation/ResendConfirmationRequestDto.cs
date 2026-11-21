using System.ComponentModel.DataAnnotations;

namespace HeyMom.Api.Features.Authentication.ResendConfirmation;

public sealed record ResendConfirmationRequestDto(
    [property: Required, EmailAddress, StringLength(256)] string Email);
