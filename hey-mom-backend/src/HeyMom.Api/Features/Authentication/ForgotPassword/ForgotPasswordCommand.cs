using HeyMom.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HeyMom.Api.Features.Authentication.ForgotPassword;

internal sealed class ForgotPasswordCommand(
    UserManager<ApplicationUser> userManager,
    IEmailSender<ApplicationUser> emailSender,
    IOptions<AuthenticationOptions> options)
{
    public async Task ExecuteAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.IsEmailConfirmedAsync(user))
        {
            return;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var link = AuthenticationLinks.PasswordReset(options.Value.PublicOrigin, request.Email, token);
        await emailSender.SendPasswordResetLinkAsync(user, request.Email, link);
    }
}
