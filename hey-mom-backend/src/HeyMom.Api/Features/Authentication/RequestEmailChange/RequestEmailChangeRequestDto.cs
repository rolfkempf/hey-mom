using System.ComponentModel.DataAnnotations;

namespace HeyMom.Api.Features.Authentication.RequestEmailChange;

public sealed record RequestEmailChangeRequestDto(
    [property: Required, EmailAddress, StringLength(256)] string NewEmail,
    [property: Required, StringLength(256)] string CurrentPassword);
