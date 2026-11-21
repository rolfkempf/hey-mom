using HeyMom.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HeyMom.Api.Features.Authentication.ResendConfirmation;

internal sealed class ResendConfirmationCommand(
    UserManager<ApplicationUser> userManager,
    IEmailSender<ApplicationUser> emailSender,
    IOptions<AuthenticationOptions> options)
{
    public async Task ExecuteAsync(ResendConfirmationRequestDto request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || await userManager.IsEmailConfirmedAsync(user))
        {
            return;
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var link = AuthenticationLinks.EmailConfirmation(options.Value.PublicOrigin, user.Id, token);
        await emailSender.SendConfirmationLinkAsync(user, request.Email, link);
    }
}
