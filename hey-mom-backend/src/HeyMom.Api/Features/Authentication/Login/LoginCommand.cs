using HeyMom.Persistence.Entities;
using Microsoft.AspNetCore.Identity;

namespace HeyMom.Api.Features.Authentication.Login;

internal sealed class LoginCommand(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
{
    public async Task<bool> ExecuteAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return false;
        }

        var result = await signInManager.PasswordSignInAsync(user, request.Password, isPersistent: false, lockoutOnFailure: true);
        return result.Succeeded;
    }
}
