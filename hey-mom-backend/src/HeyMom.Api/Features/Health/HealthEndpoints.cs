using HeyMom.Api.Features.Health.GetHealth;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HeyMom.Api.Features.Health;

internal static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", GetHealthAsync)
            .WithName("GetHealth")
            .WithSummary("Checks whether the API and PostgreSQL are available.")
            .WithTags("System")
            .Produces(StatusCodes.Status503ServiceUnavailable);
    }

    private static async Task<Results<Ok<GetHealthResponseDto>, StatusCodeHttpResult>> GetHealthAsync(
        GetHealthQuery query,
        CancellationToken cancellationToken)
    {
        var canConnect = await query.ExecuteAsync(cancellationToken);

        return canConnect
            ? TypedResults.Ok(new GetHealthResponseDto("healthy"))
            : TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
}
