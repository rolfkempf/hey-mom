using System.Security.Claims;
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
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace HeyMom.Api.Features.Authentication;

internal static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/auth")
            .WithTags("Authentication")
            .AddEndpointFilter<AuthenticationAntiforgeryFilter>();

        auth.MapGet("/csrf", GetCsrf)
            .WithName("GetAuthCsrf")
            .WithSummary("Issues an antiforgery token for the current browser identity.")
            .RequireRateLimiting(AuthenticationServiceCollectionExtensions.CsrfRateLimit)
            .Produces(StatusCodes.Status429TooManyRequests);

        auth.MapGet("/me", GetCurrentUserAsync)
            .WithName("GetAuthCurrentUser")
            .WithSummary("Returns the signed-in account.")
            .RequireAuthorization()
            .Produces(StatusCodes.Status401Unauthorized);

        auth.MapPost("/register", RegisterAsync)
            .WithName("RegisterAccount")
            .WithSummary("Creates an account and sends a confirmation link.")
            .RequireRateLimiting(AuthenticationServiceCollectionExtensions.MailRateLimit)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status429TooManyRequests);

        auth.MapPost("/resend-confirmation", ResendConfirmationAsync)
            .WithName("ResendAccountConfirmation")
            .WithSummary("Requests another confirmation link.")
            .RequireRateLimiting(AuthenticationServiceCollectionExtensions.MailRateLimit)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status429TooManyRequests);

        auth.MapPost("/confirm-email", ConfirmEmailAsync)
            .WithName("ConfirmAccountEmail")
            .WithSummary("Confirms a newly registered account.")
            .RequireRateLimiting(AuthenticationServiceCollectionExtensions.AccountRateLimit)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status429TooManyRequests);

        auth.MapPost("/login", LoginAsync)
            .WithName("LoginAccount")
            .WithSummary("Signs in an account and sets its session cookie.")
            .RequireRateLimiting(AuthenticationServiceCollectionExtensions.AccountRateLimit)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status429TooManyRequests)
            .Produces(StatusCodes.Status401Unauthorized);

        auth.MapPost("/logout", LogoutAsync)
            .WithName("LogoutAccount")
            .WithSummary("Clears the session cookie.")
            .RequireAuthorization()
            .RequireRateLimiting(AuthenticationServiceCollectionExtensions.AccountRateLimit)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status429TooManyRequests)
            .Produces(StatusCodes.Status401Unauthorized);

        auth.MapPost("/forgot-password", ForgotPasswordAsync)
            .WithName("RequestPasswordReset")
            .WithSummary("Requests a password reset link.")
            .RequireRateLimiting(AuthenticationServiceCollectionExtensions.MailRateLimit)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status429TooManyRequests);

        auth.MapPost("/reset-password", ResetPasswordAsync)
            .WithName("ResetAccountPassword")
            .WithSummary("Sets a new password using an emailed code.")
            .RequireRateLimiting(AuthenticationServiceCollectionExtensions.AccountRateLimit)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status429TooManyRequests);

        auth.MapPost("/change-password", ChangePasswordAsync)
            .WithName("ChangeAccountPassword")
            .WithSummary("Changes the signed-in account's password.")
            .RequireAuthorization()
            .RequireRateLimiting(AuthenticationServiceCollectionExtensions.AccountRateLimit)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status429TooManyRequests)
            .Produces(StatusCodes.Status401Unauthorized);

        auth.MapPost("/request-email-change", RequestEmailChangeAsync)
            .WithName("RequestAccountEmailChange")
            .WithSummary("Sends a confirmation link to a replacement address.")
            .RequireAuthorization()
            .RequireRateLimiting(AuthenticationServiceCollectionExtensions.MailRateLimit)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status429TooManyRequests)
            .Produces(StatusCodes.Status401Unauthorized);

        auth.MapPost("/confirm-email-change", ConfirmEmailChangeAsync)
            .WithName("ConfirmAccountEmailChange")
            .WithSummary("Confirms and replaces an account's email address.")
            .RequireRateLimiting(AuthenticationServiceCollectionExtensions.AccountRateLimit)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status429TooManyRequests);
    }

    private static Ok<CsrfResponseDto> GetCsrf(GetCsrfQuery query, HttpContext context) =>
        TypedResults.Ok(query.Execute(context));

    private static async Task<Results<Ok<CurrentUserResponseDto>, UnauthorizedHttpResult>> GetCurrentUserAsync(
        GetCurrentUserQuery query,
        ClaimsPrincipal user,
        HttpResponse response)
    {
        response.Headers.CacheControl = "no-store";
        var account = await query.ExecuteAsync(user);
        return account is null ? TypedResults.Unauthorized() : TypedResults.Ok(account);
    }

    private static async Task<Results<NoContent, ValidationProblem>> RegisterAsync(
        RegisterRequestDto request,
        RegisterCommand command,
        CancellationToken cancellationToken) =>
        ToHttpResult(await command.ExecuteAsync(request, cancellationToken));

    private static async Task<NoContent> ResendConfirmationAsync(
        ResendConfirmationRequestDto request,
        ResendConfirmationCommand command,
        CancellationToken cancellationToken)
    {
        await command.ExecuteAsync(request, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, ValidationProblem>> ConfirmEmailAsync(
        ConfirmEmailRequestDto request,
        ConfirmEmailCommand command,
        CancellationToken cancellationToken) =>
        ToHttpResult(await command.ExecuteAsync(request, cancellationToken));

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> LoginAsync(
        LoginRequestDto request,
        LoginCommand command,
        CancellationToken cancellationToken) =>
        await command.ExecuteAsync(request, cancellationToken) ? TypedResults.NoContent() : TypedResults.Unauthorized();

    private static async Task<NoContent> LogoutAsync(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        await command.ExecuteAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> ForgotPasswordAsync(
        ForgotPasswordRequestDto request,
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        await command.ExecuteAsync(request, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, ValidationProblem>> ResetPasswordAsync(
        ResetPasswordRequestDto request,
        ResetPasswordCommand command,
        CancellationToken cancellationToken) =>
        ToHttpResult(await command.ExecuteAsync(request, cancellationToken));

    private static async Task<Results<NoContent, ValidationProblem>> ChangePasswordAsync(
        ChangePasswordRequestDto request,
        ChangePasswordCommand command,
        ClaimsPrincipal user,
        CancellationToken cancellationToken) =>
        ToHttpResult(await command.ExecuteAsync(user, request, cancellationToken));

    private static async Task<Results<NoContent, ValidationProblem>> RequestEmailChangeAsync(
        RequestEmailChangeRequestDto request,
        RequestEmailChangeCommand command,
        ClaimsPrincipal user,
        CancellationToken cancellationToken) =>
        ToHttpResult(await command.ExecuteAsync(user, request, cancellationToken));

    private static async Task<Results<NoContent, ValidationProblem>> ConfirmEmailChangeAsync(
        ConfirmEmailChangeRequestDto request,
        ConfirmEmailChangeCommand command,
        CancellationToken cancellationToken) =>
        ToHttpResult(await command.ExecuteAsync(request, cancellationToken));

    private static Results<NoContent, ValidationProblem> ToHttpResult(IdentityResult result) =>
        result.Succeeded
            ? TypedResults.NoContent()
            : TypedResults.ValidationProblem(result.Errors.GroupBy(error => error.Code)
                .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray()));
}
