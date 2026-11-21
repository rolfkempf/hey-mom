using Xunit;

namespace HeyMom.Api.Tests.Features.Authentication;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("HEYMOM_TEST_POSTGRES")))
        {
            Skip = "Set HEYMOM_TEST_POSTGRES to run tests against isolated PostgreSQL databases.";
        }
    }
}
