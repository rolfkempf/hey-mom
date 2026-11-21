using HeyMom.Persistence;

namespace HeyMom.Api.Features.Health.GetHealth;

internal sealed class GetHealthQuery(HeyMomDbContext database)
{
    public Task<bool> ExecuteAsync(CancellationToken cancellationToken) =>
        database.Database.CanConnectAsync(cancellationToken);
}
