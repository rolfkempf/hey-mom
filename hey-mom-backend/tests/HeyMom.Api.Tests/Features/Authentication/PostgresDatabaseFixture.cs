using HeyMom.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace HeyMom.Api.Tests.Features.Authentication;

public sealed class PostgresDatabaseFixture : IAsyncLifetime
{
    private readonly string? _adminConnectionString;
    private string? _databaseName;

    public PostgresDatabaseFixture()
    {
        var configuredConnectionString = Environment.GetEnvironmentVariable("HEYMOM_TEST_POSTGRES");
        if (!string.IsNullOrWhiteSpace(configuredConnectionString))
        {
            var connection = new NpgsqlConnectionStringBuilder(configuredConnectionString);
            _databaseName = $"hey_mom_auth_{Guid.NewGuid():N}";
            connection.Database = "postgres";
            _adminConnectionString = connection.ConnectionString;

            var applicationConnection = new NpgsqlConnectionStringBuilder(configuredConnectionString)
            {
                Database = _databaseName,
            };
            ConnectionString = applicationConnection.ConnectionString;
        }
    }

    public string? ConnectionString { get; }

    public async Task InitializeAsync()
    {
        if (_adminConnectionString is null || _databaseName is null)
        {
            return;
        }

        await using (var admin = new NpgsqlConnection(_adminConnectionString))
        {
            await admin.OpenAsync();
            await using var create = new NpgsqlCommand($"CREATE DATABASE \"{_databaseName}\"", admin);
            await create.ExecuteNonQueryAsync();
        }

        var options = new DbContextOptionsBuilder<HeyMomDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        await using var context = new HeyMomDbContext(options);
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_adminConnectionString is null || _databaseName is null)
        {
            return;
        }

        await using var admin = new NpgsqlConnection(_adminConnectionString);
        await admin.OpenAsync();
        await using var terminate = new NpgsqlCommand(
            "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = @database AND pid <> pg_backend_pid()",
            admin);
        terminate.Parameters.AddWithValue("database", _databaseName);
        await terminate.ExecuteNonQueryAsync();

        await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_databaseName}\"", admin);
        await drop.ExecuteNonQueryAsync();
    }
}
