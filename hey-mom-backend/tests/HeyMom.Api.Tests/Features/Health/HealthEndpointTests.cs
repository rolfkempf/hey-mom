using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HeyMom.Api.Tests.Features.Health;

public sealed class HealthEndpointTests
{
    [Fact]
    public async Task GetHealthReturnsServiceUnavailableWhenPostgresIsUnreachable()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Authentication:PublicOrigin", "https://localhost");
            builder.UseSetting(
                "ConnectionStrings:Database",
                "Host=127.0.0.1;Port=1;Database=hey_mom;Username=hey_mom;Password=test;Timeout=1");
        });
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());
    }
}
