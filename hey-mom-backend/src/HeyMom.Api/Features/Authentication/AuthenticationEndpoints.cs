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
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace HeyMom.Api.Features.Authentication;

internal static class AuthenticationEndpoints
{
    public static void MapEndpoints(RouteGroupBuilder api)
    {
        var auth = api.MapGroup("/auth").WithTags("Authentication");

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

        var writes = auth.MapGroup("").AddEndpointFilter(ValidateAntiforgeryAsync);

        Write(writes, "/register", RegisterAsync, "RegisterAccount", "Creates an account and sends a confirmation link.", sendsMail: true);
        Write(writes, "/resend-confirmation", ResendConfirmationAsync, "ResendAccountConfirmation", "Requests another confirmation link.", sendsMail: true);
        Write(writes, "/confirm-email", ConfirmEmailAsync, "ConfirmAccountEmail", "Confirms a newly registered account.");
        Write(writes, "/login", LoginAsync, "LoginAccount", "Signs in an account and sets its session cookie.")
            .Produces(StatusCodes.Status401Unauthorized);
        Write(writes, "/logout", LogoutAsync, "LogoutAccount", "Clears the session cookie.")
            .RequireAuthorization().Produces(StatusCodes.Status401Unauthorized);
        Write(writes, "/forgot-password", ForgotPasswordAsync, "RequestPasswordReset", "Requests a password reset link.", sendsMail: true);
        Write(writes, "/reset-password", ResetPasswordAsync, "ResetAccountPassword", "Sets a new password using an emailed code.");
        Write(writes, "/change-password", ChangePasswordAsync, "ChangeAccountPassword", "Changes the signed-in account's password.")
            .RequireAuthorization().Produces(StatusCodes.Status401Unauthorized);
        Write(writes, "/request-email-change", RequestEmailChangeAsync, "RequestAccountEmailChange", "Sends a confirmation link to a replacement address.", sendsMail: true)
            .RequireAuthorization().Produces(StatusCodes.Status401Unauthorized);
        Write(writes, "/confirm-email-change", ConfirmEmailChangeAsync, "ConfirmAccountEmailChange", "Confirms and replaces an account's email address.");
    }

    private static RouteHandlerBuilder Write(
        RouteGroupBuilder group, string route, Delegate handler, string name, string summary, bool sendsMail = false) =>
        group.MapPost(route, handler)
            .WithName(name)
            .WithSummary(summary)
            .RequireRateLimiting(sendsMail
                ? AuthenticationServiceCollectionExtensions.MailRateLimit
                : AuthenticationServiceCollectionExtensions.AccountRateLimit)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status429TooManyRequests);

    private static async ValueTask<object?> ValidateAntiforgeryAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        http.Response.Headers.CacheControl = "no-store";
        var antiforgery = http.RequestServices.GetRequiredService<IAntiforgery>();
        try
        {
            await antiforgery.ValidateRequestAsync(http);
        }
        catch (AntiforgeryValidationException)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Antiforgery"] = ["Invalid antiforgery token."],
            });
        }

        return await next(context);
    }

    private static Ok<CsrfResponseDto> GetCsrf(GetCsrfQuery query, HttpContext context) =>
        TypedResults.Ok(query.Execute(context));

    private static async Task<Results<Ok<CurrentUserResponseDto>, UnauthorizedHttpResult>> GetCurrentUserAsync(
        GetCurrentUserQuery query, HttpContext context)
    {
        context.Response.Headers.CacheControl = "no-store";
        var user = await query.ExecuteAsync(context.User);
        return user is null ? TypedResults.Unauthorized() : TypedResults.Ok(user);
    }

    private static async Task<Results<NoContent, ValidationProblem>> RegisterAsync(
        RegisterRequestDto request, RegisterCommand command, CancellationToken cancellationToken) =>
        ToHttpResult(await command.ExecuteAsync(request, cancellationToken));

    private static async Task<NoContent> ResendConfirmationAsync(
        ResendConfirmationRequestDto request, ResendConfirmationCommand command, CancellationToken cancellationToken)
    {
        await command.ExecuteAsync(request, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, ValidationProblem>> ConfirmEmailAsync(
        ConfirmEmailRequestDto request, ConfirmEmailCommand command, CancellationToken cancellationToken) =>
        ToHttpResult(await command.ExecuteAsync(request, cancellationToken));

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> LoginAsync(
        LoginRequestDto request, LoginCommand command, CancellationToken cancellationToken) =>
        await command.ExecuteAsync(request, cancellationToken) ? TypedResults.NoContent() : TypedResults.Unauthorized();

    private static async Task<NoContent> LogoutAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        await command.ExecuteAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> ForgotPasswordAsync(
        ForgotPasswordRequestDto request, ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        await command.ExecuteAsync(request, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, ValidationProblem>> ResetPasswordAsync(
        ResetPasswordRequestDto request, ResetPasswordCommand command, CancellationToken cancellationToken) =>
        ToHttpResult(await command.ExecuteAsync(request, cancellationToken));

    private static async Task<Results<NoContent, ValidationProblem>> ChangePasswordAsync(
        ChangePasswordRequestDto request, ChangePasswordCommand command, HttpContext context, CancellationToken cancellationToken) =>
        ToHttpResult(await command.ExecuteAsync(context.User, request, cancellationToken));

    private static async Task<Results<NoContent, ValidationProblem>> RequestEmailChangeAsync(
        RequestEmailChangeRequestDto request, RequestEmailChangeCommand command, HttpContext context, CancellationToken cancellationToken) =>
        ToHttpResult(await command.ExecuteAsync(context.User, request, cancellationToken));

    private static async Task<Results<NoContent, ValidationProblem>> ConfirmEmailChangeAsync(
        ConfirmEmailChangeRequestDto request, ConfirmEmailChangeCommand command, CancellationToken cancellationToken) =>
        ToHttpResult(await command.ExecuteAsync(request, cancellationToken));

    private static Results<NoContent, ValidationProblem> ToHttpResult(IdentityResult result) =>
        result.Succeeded
            ? TypedResults.NoContent()
            : TypedResults.ValidationProblem(result.Errors.GroupBy(error => error.Code)
                .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray()));
}
