using System.Text;
using HeyMom.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace HeyMom.Api.Features.Authentication.ConfirmEmail;

internal sealed class ConfirmEmailCommand(UserManager<ApplicationUser> userManager)
{
    public async Task<IdentityResult> ExecuteAsync(ConfirmEmailRequestDto request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return InvalidConfirmation();
        }

        try
        {
            return await userManager.ConfirmEmailAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code)));
        }
        catch (FormatException)
        {
            return InvalidConfirmation();
        }
    }

    private static IdentityResult InvalidConfirmation() => IdentityResult.Failed(new IdentityError
    {
        Code = "InvalidConfirmation",
        Description = "The confirmation link is invalid or has expired.",
    });
}
