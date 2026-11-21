using HeyMom.Api.Features.Health;

namespace HeyMom.Api.Features;

internal static class FeatureRegistration
{
    public static void AddFeatures(this IServiceCollection services)
    {
        services.AddHealthFeature();
    }

    public static void MapFeatures(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api");
        HealthEndpoints.MapEndpoints(api);
    }
}
