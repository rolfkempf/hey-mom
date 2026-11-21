using HeyMom.Persistence.Entities;
using Microsoft.AspNetCore.Identity;

namespace HeyMom.Api.Features.Authentication.Logout;

internal sealed class LogoutCommand(SignInManager<ApplicationUser> signInManager)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await signInManager.SignOutAsync();
    }
}
