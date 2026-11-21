using HeyMom.Api.Features.Authentication;
using HeyMom.Api.Features.Health;

namespace HeyMom.Api.Features;

internal static class FeatureRegistration
{
    public static void AddFeatures(
        this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddHealthFeature();
        services.AddAuthenticationFeature(configuration, environment);
    }

    public static void MapFeatures(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api");
        HealthEndpoints.MapEndpoints(api);
        AuthenticationEndpoints.MapEndpoints(api);
    }
}
