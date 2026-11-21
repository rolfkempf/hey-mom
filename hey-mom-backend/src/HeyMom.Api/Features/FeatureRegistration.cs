using HeyMom.Api.Features.Authentication;
using HeyMom.Api.Features.Health;

namespace HeyMom.Api.Features;

internal static class FeatureRegistration
{
    public static IServiceCollection AddFeatures(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment) =>
        services.AddHealthFeature()
            .AddAuthenticationFeature(configuration, environment);

    public static void MapFeatures(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api");
        api.MapHealthEndpoints();
        api.MapAuthenticationEndpoints();
    }
}
