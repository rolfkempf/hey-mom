using System.Text;
using HeyMom.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace HeyMom.Api.Features.Authentication.ResetPassword;

internal sealed class ResetPasswordCommand(UserManager<ApplicationUser> userManager)
{
    public async Task<IdentityResult> ExecuteAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError { Code = "InvalidReset", Description = "The reset link is invalid or has expired." });
        }

        try
        {
            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            return await userManager.ResetPasswordAsync(user, token, request.NewPassword);
        }
        catch (FormatException)
        {
            return IdentityResult.Failed(new IdentityError { Code = "InvalidReset", Description = "The reset link is invalid or has expired." });
        }
    }
}
