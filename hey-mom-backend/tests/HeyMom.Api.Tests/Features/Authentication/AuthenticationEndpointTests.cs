using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HeyMom.Persistence;
using HeyMom.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace HeyMom.Api.Tests.Features.Authentication;

public sealed class AuthenticationEndpointTests(PostgresDatabaseFixture database) : IClassFixture<PostgresDatabaseFixture>, IDisposable
{
    private readonly TestEmailSender _emailSender = new();
    private readonly string _dataProtectionKeyPath = Path.Combine(Path.GetTempPath(), $"hey-mom-tests-{Guid.NewGuid():N}");

    private WebApplicationFactory<Program> CreateFactory(TimeSpan? tokenLifespan = null)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Database", database.ConnectionString);
            builder.UseSetting("Authentication:PublicOrigin", "https://localhost");
            builder.UseSetting("Authentication:DataProtectionKeyPath", _dataProtectionKeyPath);
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IEmailSender<ApplicationUser>>();
                services.AddSingleton<IEmailSender<ApplicationUser>>(_emailSender);
                if (tokenLifespan is { } lifespan)
                {
                    services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = lifespan);
                }
            });
        });
    }

    [PostgresFact]
    public async Task RegistrationConfirmationLoginAndLogoutUseTheDatabaseSession()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var email = $"{Guid.NewGuid():N}@example.test";
        const string password = "First-password-42!";

        using var anonymousUserResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousUserResponse.StatusCode);

        using var registration = await PostAsync(client, "/api/auth/register", new { email, password });
        Assert.Equal(HttpStatusCode.NoContent, registration.StatusCode);

        using var unconfirmedLogin = await PostAsync(client, "/api/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.Unauthorized, unconfirmedLogin.StatusCode);

        var confirmation = _emailSender.ConfirmationLinks.Last();
        using var confirmResponse = await PostAsync(client, "/api/auth/confirm-email", ReadLinkValues(confirmation));
        Assert.Equal(HttpStatusCode.NoContent, confirmResponse.StatusCode);

        using var login = await PostAsync(client, "/api/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        var sessionCookie = Assert.Single(login.Headers.GetValues("Set-Cookie"), value => value.StartsWith("__Host-HeyMom.Session=", StringComparison.Ordinal));
        Assert.Contains("; secure", sessionCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", sessionCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", sessionCookie, StringComparison.OrdinalIgnoreCase);

        using var meResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        using var me = JsonDocument.Parse(await meResponse.Content.ReadAsStringAsync());
        Assert.Equal(email, me.RootElement.GetProperty("email").GetString());
        Assert.False(string.IsNullOrWhiteSpace(me.RootElement.GetProperty("id").GetString()));

        using var logout = await PostAsync(client, "/api/auth/logout", new { });
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        using var afterLogout = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, afterLogout.StatusCode);
    }

    [PostgresFact]
    public async Task PasswordResetIsNeutralForUnknownEmailsAndCannotBeReplayed()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var email = $"{Guid.NewGuid():N}@example.test";
        const string oldPassword = "First-password-42!";
        const string newPassword = "Second-password-42!";

        using var registration = await PostAsync(client, "/api/auth/register", new { email, password = oldPassword });
        Assert.Equal(HttpStatusCode.NoContent, registration.StatusCode);
        using var confirm = await PostAsync(client, "/api/auth/confirm-email", ReadLinkValues(_emailSender.ConfirmationLinks.Last()));
        Assert.Equal(HttpStatusCode.NoContent, confirm.StatusCode);
        using var login = await PostAsync(client, "/api/auth/login", new { email, password = oldPassword });
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

        using var knownEmailResponse = await PostAsync(client, "/api/auth/forgot-password", new { email });
        using var unknownEmailResponse = await PostAsync(client, "/api/auth/forgot-password", new { email = $"missing-{Guid.NewGuid():N}@example.test" });
        Assert.Equal(knownEmailResponse.StatusCode, unknownEmailResponse.StatusCode);
        Assert.Equal(await knownEmailResponse.Content.ReadAsStringAsync(), await unknownEmailResponse.Content.ReadAsStringAsync());

        var resetValues = ReadLinkValues(_emailSender.PasswordResetLinks.Last());
        resetValues["newPassword"] = newPassword;
        using var reset = await PostAsync(client, "/api/auth/reset-password", resetValues);
        Assert.Equal(HttpStatusCode.NoContent, reset.StatusCode);
        using var invalidatedSession = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, invalidatedSession.StatusCode);

        using var replay = await PostAsync(client, "/api/auth/reset-password", resetValues);
        Assert.Equal(HttpStatusCode.BadRequest, replay.StatusCode);
        using var oldPasswordLogin = await PostAsync(client, "/api/auth/login", new { email, password = oldPassword });
        Assert.Equal(HttpStatusCode.Unauthorized, oldPasswordLogin.StatusCode);
        using var newPasswordLogin = await PostAsync(client, "/api/auth/login", new { email, password = newPassword });
        Assert.Equal(HttpStatusCode.NoContent, newPasswordLogin.StatusCode);
    }

    [PostgresFact]
    public async Task EmailChangeTakesEffectOnlyAfterConfirmation()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var email = $"{Guid.NewGuid():N}@example.test";
        var newEmail = $"changed-{Guid.NewGuid():N}@example.test";
        const string password = "First-password-42!";

        using var registration = await PostAsync(client, "/api/auth/register", new { email, password });
        Assert.Equal(HttpStatusCode.NoContent, registration.StatusCode);
        using var confirmation = await PostAsync(client, "/api/auth/confirm-email", ReadLinkValues(_emailSender.ConfirmationLinks.Last()));
        Assert.Equal(HttpStatusCode.NoContent, confirmation.StatusCode);
        using var login = await PostAsync(client, "/api/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

        using var changeRequest = await PostAsync(client, "/api/auth/request-email-change", new { newEmail, currentPassword = password });
        Assert.Equal(HttpStatusCode.NoContent, changeRequest.StatusCode);

        using var beforeConfirmation = await client.GetAsync("/api/auth/me");
        using var currentUser = JsonDocument.Parse(await beforeConfirmation.Content.ReadAsStringAsync());
        Assert.Equal(email, currentUser.RootElement.GetProperty("email").GetString());
        var userId = currentUser.RootElement.GetProperty("id").GetString();

        using var confirmChange = await PostAsync(client, "/api/auth/confirm-email-change", ReadLinkValues(_emailSender.ConfirmationLinks.Last()));
        Assert.Equal(HttpStatusCode.NoContent, confirmChange.StatusCode);
        using var afterConfirmation = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, afterConfirmation.StatusCode);

        using var changedEmailLogin = await PostAsync(client, "/api/auth/login", new { email = newEmail, password });
        Assert.Equal(HttpStatusCode.NoContent, changedEmailLogin.StatusCode);
        using var updatedUserResponse = await client.GetAsync("/api/auth/me");
        using var updatedUser = JsonDocument.Parse(await updatedUserResponse.Content.ReadAsStringAsync());
        Assert.Equal(userId, updatedUser.RootElement.GetProperty("id").GetString());
        Assert.Equal(newEmail, updatedUser.RootElement.GetProperty("email").GetString());
    }

    [PostgresFact]
    public async Task RepeatedFailedLoginsLockOutTheAccount()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var email = $"{Guid.NewGuid():N}@example.test";
        const string password = "First-password-42!";

        using var registration = await PostAsync(client, "/api/auth/register", new { email, password });
        Assert.Equal(HttpStatusCode.NoContent, registration.StatusCode);
        using var confirmation = await PostAsync(client, "/api/auth/confirm-email", ReadLinkValues(_emailSender.ConfirmationLinks.Last()));
        Assert.Equal(HttpStatusCode.NoContent, confirmation.StatusCode);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var failedLogin = await PostAsync(client, "/api/auth/login", new { email, password = "Incorrect-password-42!" });
            Assert.Equal(HttpStatusCode.Unauthorized, failedLogin.StatusCode);
        }

        using var correctPasswordWhileLocked = await PostAsync(client, "/api/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.Unauthorized, correctPasswordWhileLocked.StatusCode);
    }

    [PostgresFact]
    public async Task RegistrationAndResendResponsesDoNotRevealWhetherAnEmailExists()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var email = $"{Guid.NewGuid():N}@example.test";
        const string password = "First-password-42!";

        using var registration = await PostAsync(client, "/api/auth/register", new { email, password });
        Assert.Equal(HttpStatusCode.NoContent, registration.StatusCode);
        var confirmationCount = _emailSender.ConfirmationLinks.Count;

        using var knownResend = await PostAsync(client, "/api/auth/resend-confirmation", new { email });
        using var unknownResend = await PostAsync(client, "/api/auth/resend-confirmation", new { email = $"missing-{Guid.NewGuid():N}@example.test" });
        Assert.Equal(knownResend.StatusCode, unknownResend.StatusCode);
        Assert.Equal(await knownResend.Content.ReadAsStringAsync(), await unknownResend.Content.ReadAsStringAsync());
        Assert.Equal(confirmationCount + 1, _emailSender.ConfirmationLinks.Count);

        using var duplicate = await PostAsync(client, "/api/auth/register", new { email, password });
        Assert.Equal(HttpStatusCode.NoContent, duplicate.StatusCode);
        Assert.Equal(confirmationCount + 1, _emailSender.ConfirmationLinks.Count);
    }

    [PostgresFact]
    public async Task ExpiredAndTamperedPasswordResetTokensAreRejected()
    {
        using (var factory = CreateFactory())
        using (var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") }))
        {
            var registeredEmail = $"{Guid.NewGuid():N}@example.test";
            const string password = "First-password-42!";
            using var registration = await PostAsync(client, "/api/auth/register", new { email = registeredEmail, password });
            Assert.Equal(HttpStatusCode.NoContent, registration.StatusCode);
            using var confirmation = await PostAsync(client, "/api/auth/confirm-email", ReadLinkValues(_emailSender.ConfirmationLinks.Last()));
            Assert.Equal(HttpStatusCode.NoContent, confirmation.StatusCode);
        }

        using var expiredFactory = CreateFactory(TimeSpan.FromSeconds(-1));
        using var expiredClient = expiredFactory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var emailAddress = ReadLinkValues(_emailSender.ConfirmationLinks.Last())["userId"];
        string resetEmail;
        using (var scope = expiredFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HeyMomDbContext>();
            resetEmail = (await db.Users.SingleAsync(user => user.Id == emailAddress)).Email!;
        }

        using var requestReset = await PostAsync(expiredClient, "/api/auth/forgot-password", new { email = resetEmail });
        Assert.Equal(HttpStatusCode.NoContent, requestReset.StatusCode);
        var resetValues = ReadLinkValues(_emailSender.PasswordResetLinks.Last());
        resetValues["newPassword"] = "Second-password-42!";
        using var expiredReset = await PostAsync(expiredClient, "/api/auth/reset-password", resetValues);
        Assert.Equal(HttpStatusCode.BadRequest, expiredReset.StatusCode);

        resetValues["code"] = "!not-a-valid-token!";
        using var tamperedReset = await PostAsync(expiredClient, "/api/auth/reset-password", resetValues);
        Assert.Equal(HttpStatusCode.BadRequest, tamperedReset.StatusCode);
    }

    [PostgresFact]
    public async Task MailRateLimitRejectsRequestsAfterFiveInOneWindow()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var email = $"missing-{Guid.NewGuid():N}@example.test";

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var response = await PostAsync(client, "/api/auth/forgot-password", new { email });
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        using var rateLimited = await PostAsync(client, "/api/auth/forgot-password", new { email });
        Assert.Equal(HttpStatusCode.TooManyRequests, rateLimited.StatusCode);
    }

    [PostgresFact]
    public async Task WritesRequireAValidAntiforgeryToken()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var body = JsonContent.Create(new { email = $"{Guid.NewGuid():N}@example.test", password = "First-password-42!" });

        using var missingToken = await client.PostAsync("/api/auth/register", body);
        Assert.Equal(HttpStatusCode.BadRequest, missingToken.StatusCode);

        using var tamperedRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register")
        {
            Content = JsonContent.Create(new { email = $"{Guid.NewGuid():N}@example.test", password = "First-password-42!" }),
        };
        tamperedRequest.Headers.Add("X-CSRF-TOKEN", "invalid-token");
        using var invalidToken = await client.SendAsync(tamperedRequest);
        Assert.Equal(HttpStatusCode.BadRequest, invalidToken.StatusCode);
    }

    [PostgresFact]
    public async Task ConcurrentRegistrationsForOneEmailCreateOnlyOneAccount()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var email = $"{Guid.NewGuid():N}@example.test";
        const string password = "First-password-42!";
        using var csrfResponse = await client.GetAsync("/api/auth/csrf");
        csrfResponse.EnsureSuccessStatusCode();
        using var csrf = JsonDocument.Parse(await csrfResponse.Content.ReadAsStringAsync());
        var requestToken = csrf.RootElement.GetProperty("requestToken").GetString();

        using var firstRequest = CreatePostRequest("/api/auth/register", new { email, password }, requestToken);
        using var secondRequest = CreatePostRequest("/api/auth/register", new { email, password }, requestToken);
        var responses = await Task.WhenAll(client.SendAsync(firstRequest), client.SendAsync(secondRequest));
        using var firstResponse = responses[0];
        using var secondResponse = responses[1];
        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HeyMomDbContext>();
        Assert.Equal(1, await db.Users.CountAsync(user => user.Email == email));
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string path, object body)
    {
        using var csrfResponse = await client.GetAsync("/api/auth/csrf");
        csrfResponse.EnsureSuccessStatusCode();
        using var csrf = JsonDocument.Parse(await csrfResponse.Content.ReadAsStringAsync());
        using var request = CreatePostRequest(path, body, csrf.RootElement.GetProperty("requestToken").GetString());
        return await client.SendAsync(request);
    }

    private static HttpRequestMessage CreatePostRequest(string path, object body, string? requestToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Add("X-CSRF-TOKEN", requestToken);
        return request;
    }

    private static Dictionary<string, string> ReadLinkValues(string link)
    {
        var fragment = new Uri(link).Fragment.TrimStart('#');
        return fragment.Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => value.Split('=', 2))
            .ToDictionary(pair => Uri.UnescapeDataString(pair[0]), pair => Uri.UnescapeDataString(pair[1]));
    }

    private sealed class TestEmailSender : IEmailSender<ApplicationUser>
    {
        public ConcurrentQueue<string> ConfirmationLinks { get; } = new();
        public ConcurrentQueue<string> PasswordResetLinks { get; } = new();

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            ConfirmationLinks.Enqueue(confirmationLink);
            return Task.CompletedTask;
        }

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            PasswordResetLinks.Enqueue(resetLink);
            return Task.CompletedTask;
        }

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            PasswordResetLinks.Enqueue(resetCode);
            return Task.CompletedTask;
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_dataProtectionKeyPath))
        {
            Directory.Delete(_dataProtectionKeyPath, recursive: true);
        }
    }
}
