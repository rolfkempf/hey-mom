using Microsoft.AspNetCore.Antiforgery;

namespace HeyMom.Api.Features.Authentication.GetCsrf;

internal sealed class GetCsrfQuery(IAntiforgery antiforgery)
{
    public CsrfResponseDto Execute(HttpContext context) =>
        new(antiforgery.GetAndStoreTokens(context).RequestToken!);
}
