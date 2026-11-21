using HeyMom.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HeyMom.Api.Features.Authentication.Register;

internal sealed class RegisterCommand(
    UserManager<ApplicationUser> userManager,
    IEmailSender<ApplicationUser> emailSender,
    IOptions<AuthenticationOptions> options)
{
    public async Task<IdentityResult> ExecuteAsync(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            return IdentityResult.Success;
        }

        var user = new ApplicationUser { Email = request.Email, UserName = $"account-{Guid.NewGuid():N}" };
        try
        {
            var createResult = await userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                if (createResult.Errors.Any(error => error.Code == "DuplicateEmail"))
                {
                    return IdentityResult.Success;
                }

                return createResult;
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = AuthenticationLinks.EmailConfirmation(options.Value.PublicOrigin, user.Id, token);
            await emailSender.SendConfirmationLinkAsync(user, request.Email, link);
            return createResult;
        }
        catch (DbUpdateException exception) when (IdentityEmailErrors.IsUniqueEmailViolation(exception))
        {
            return IdentityResult.Success;
        }
    }
}
