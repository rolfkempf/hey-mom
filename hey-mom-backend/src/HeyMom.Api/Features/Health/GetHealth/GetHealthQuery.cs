using HeyMom.Persistence;

namespace HeyMom.Api.Features.Health.GetHealth;

internal sealed class GetHealthQuery(HeyMomDbContext database)
{
    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await database.Database.CanConnectAsync(cancellationToken);
    }
}
