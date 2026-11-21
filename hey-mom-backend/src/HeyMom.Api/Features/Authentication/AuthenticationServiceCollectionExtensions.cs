using System.Threading.RateLimiting;
using HeyMom.Api.Features.Authentication.ChangePassword;
using HeyMom.Api.Features.Authentication.ConfirmEmail;
using HeyMom.Api.Features.Authentication.ConfirmEmailChange;
using HeyMom.Api.Features.Authentication.ForgotPassword;
using HeyMom.Api.Features.Authentication.GetCsrf;
using HeyMom.Api.Features.Authentication.GetCurrentUser;
using HeyMom.Api.Features.Authentication.Login;
using HeyMom.Api.Features.Authentication.Logout;
using HeyMom.Api.Features.Authentication.Register;
using HeyMom.Api.Features.Authentication.RequestEmailChange;
using HeyMom.Api.Features.Authentication.ResendConfirmation;
using HeyMom.Api.Features.Authentication.ResetPassword;
using HeyMom.Api.Infrastructure.Email;
using HeyMom.Persistence;
using HeyMom.Persistence.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;

namespace HeyMom.Api.Features.Authentication;

internal static class AuthenticationServiceCollectionExtensions
{
    internal const string CsrfRateLimit = "auth-csrf";
    internal const string AccountRateLimit = "auth-account";
    internal const string MailRateLimit = "auth-mail";

    public static IServiceCollection AddAuthenticationFeature(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var section = configuration.GetSection("Authentication");
        services.AddOptions<AuthenticationOptions>()
            .Bind(section)
            .Validate(options => Uri.TryCreate(options.PublicOrigin, UriKind.Absolute, out var uri)
                && uri.Scheme == Uri.UriSchemeHttps
                && uri.UserInfo.Length == 0
                && uri.PathAndQuery == "/"
                && uri.Fragment.Length == 0,
                "Authentication:PublicOrigin must be an HTTPS origin without a path, query or fragment.")
            .ValidateOnStart();

        var dataProtection = services.AddDataProtection().SetApplicationName($"HeyMom.Api/{environment.EnvironmentName}");
        if (section["DataProtectionKeyPath"] is { Length: > 0 } keyPath)
        {
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keyPath));
        }

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequiredLength = 12;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<HeyMomDbContext>()
            .AddDefaultTokenProviders();
        services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromHours(1));

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "__Host-HeyMom.Session";
            options.Cookie.Path = "/";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = false;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
            options.Events.OnValidatePrincipal = async context =>
            {
                // Check the current stamp on every request without reissuing the cookie on every check.
                var signIn = context.HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
                if (await signIn.ValidateSecurityStampAsync(context.Principal) is null)
                {
                    context.RejectPrincipal();
                    await signIn.SignOutAsync();
                }
            };
        });
        services.AddAuthorization();
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "__Host-HeyMom.Antiforgery";
            options.Cookie.Path = "/";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });
        services.AddValidation();
        services.AddHttpContextAccessor();
        services.Configure<SmtpOptions>(section.GetSection("Smtp"));
        services.AddTransient<IEmailSender<ApplicationUser>, IdentitySmtpEmailSender>();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            AddLimit(options, CsrfRateLimit, 120);
            AddLimit(options, AccountRateLimit, 20);
            AddLimit(options, MailRateLimit, 5);
        });

        services.AddScoped<GetCsrfQuery>();
        services.AddScoped<GetCurrentUserQuery>();
        services.AddScoped<RegisterCommand>();
        services.AddScoped<ConfirmEmailCommand>();
        services.AddScoped<ResendConfirmationCommand>();
        services.AddScoped<LoginCommand>();
        services.AddScoped<LogoutCommand>();
        services.AddScoped<ForgotPasswordCommand>();
        services.AddScoped<ResetPasswordCommand>();
        services.AddScoped<ChangePasswordCommand>();
        services.AddScoped<RequestEmailChangeCommand>();
        services.AddScoped<ConfirmEmailChangeCommand>();
        return services;
    }

    private static void AddLimit(RateLimiterOptions options, string policyName, int permitLimit)
    {
        options.AddPolicy(policyName, context => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: (context.GetEndpoint()?.DisplayName, context.Connection.RemoteIpAddress?.MapToIPv6()),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
            }));
    }
}
