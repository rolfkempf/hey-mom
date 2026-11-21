using HeyMom.Api.Features.Health.GetHealth;

namespace HeyMom.Api.Features.Health;

internal static class HealthServiceCollectionExtensions
{
    public static IServiceCollection AddHealthFeature(this IServiceCollection services) =>
        services.AddScoped<GetHealthQuery>();
}
