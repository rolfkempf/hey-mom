using System.Text;
using HeyMom.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace HeyMom.Api.Features.Authentication.ConfirmEmailChange;

internal sealed class ConfirmEmailChangeCommand(UserManager<ApplicationUser> userManager)
{
    public async Task<IdentityResult> ExecuteAsync(ConfirmEmailChangeRequestDto request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return InvalidConfirmation();
        }

        try
        {
            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            return await userManager.ChangeEmailAsync(user, request.NewEmail, token);
        }
        catch (FormatException)
        {
            return InvalidConfirmation();
        }
        catch (DbUpdateException exception) when (IdentityEmailErrors.IsUniqueEmailViolation(exception))
        {
            return IdentityResult.Failed(IdentityEmailErrors.DuplicateEmail());
        }
    }

    private static IdentityResult InvalidConfirmation() => IdentityResult.Failed(new IdentityError
    {
        Code = "InvalidConfirmation",
        Description = "The confirmation link is invalid or has expired.",
    });
}
