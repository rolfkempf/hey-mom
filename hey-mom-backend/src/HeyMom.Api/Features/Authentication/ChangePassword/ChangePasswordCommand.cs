using System.Security.Claims;
using HeyMom.Persistence.Entities;
using Microsoft.AspNetCore.Identity;

namespace HeyMom.Api.Features.Authentication.ChangePassword;

internal sealed class ChangePasswordCommand(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager)
{
    public async Task<IdentityResult> ExecuteAsync(
        ClaimsPrincipal principal,
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = "The account could not be found." });
        }

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (result.Succeeded)
        {
            await signInManager.RefreshSignInAsync(user);
        }

        return result;
    }
}
