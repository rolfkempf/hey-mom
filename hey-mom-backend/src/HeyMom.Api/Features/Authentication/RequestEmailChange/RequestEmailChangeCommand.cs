using System.Security.Claims;
using HeyMom.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HeyMom.Api.Features.Authentication.RequestEmailChange;

internal sealed class RequestEmailChangeCommand(
    UserManager<ApplicationUser> userManager,
    IEmailSender<ApplicationUser> emailSender,
    IOptions<AuthenticationOptions> options)
{
    public async Task<IdentityResult> ExecuteAsync(
        ClaimsPrincipal principal,
        RequestEmailChangeRequestDto request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = "The account could not be found." });
        }

        if (!await userManager.CheckPasswordAsync(user, request.CurrentPassword))
        {
            return IdentityResult.Failed(new IdentityError { Code = "InvalidPassword", Description = "The current password is incorrect." });
        }

        var existingUser = await userManager.FindByEmailAsync(request.NewEmail);
        if (existingUser is not null && existingUser.Id != user.Id)
        {
            return IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "This email address is already in use." });
        }

        var token = await userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
        var link = AuthenticationLinks.EmailChange(options.Value.PublicOrigin, user.Id, request.NewEmail, token);

        await emailSender.SendConfirmationLinkAsync(user, request.NewEmail, link);
        return IdentityResult.Success;
    }
}
