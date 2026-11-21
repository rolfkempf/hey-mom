using Microsoft.AspNetCore.Antiforgery;

namespace HeyMom.Api.Features.Authentication;

internal sealed class AuthenticationAntiforgeryFilter(IAntiforgery antiforgery) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        var method = http.Request.Method;
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method)
            || HttpMethods.IsOptions(method) || HttpMethods.IsTrace(method))
        {
            return await next(context);
        }

        http.Response.Headers.CacheControl = "no-store";

        try
        {
            await antiforgery.ValidateRequestAsync(http);
        }
        catch (AntiforgeryValidationException)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Antiforgery"] = ["Invalid antiforgery token."],
            });
        }

        return await next(context);
    }
}
