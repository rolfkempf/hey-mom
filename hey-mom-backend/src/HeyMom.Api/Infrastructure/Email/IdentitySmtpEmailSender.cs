using MailKit;
using HeyMom.Persistence.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HeyMom.Api.Infrastructure.Email;

internal sealed partial class IdentitySmtpEmailSender(
    IOptions<SmtpOptions> options,
    IHttpContextAccessor httpContextAccessor,
    ILogger<IdentitySmtpEmailSender> logger) : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        SendAsync(email, "Confirm your email for Hey Mom", $"Confirm your email address: {confirmationLink}");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendAsync(email, "Reset your Hey Mom password", $"Set a new password: {resetLink}");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        SendAsync(email, "Reset your Hey Mom password", $"Your password reset code: {resetCode}");

    private async Task SendAsync(string email, string subject, string body)
    {
        var settings = options.Value;
        var requestAborted = httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

        if (string.IsNullOrWhiteSpace(settings.Host) ||
            !MailboxAddress.TryParse(settings.FromAddress, out var from) ||
            !MailboxAddress.TryParse(email, out var to) ||
            settings.Port is < 1 or > 65535 ||
            (settings.UserName is not null && (!settings.UseStartTls || string.IsNullOrEmpty(settings.Password))))
        {
            LogMailFailure(logger, "InvalidConfiguration");
            return;
        }

        using var message = new MimeMessage { Subject = subject, Body = new TextPart("plain") { Text = body } };
        message.From.Add(from);
        message.To.Add(to);

        using var smtp = new SmtpClient();
        using var sendTimeout = CancellationTokenSource.CreateLinkedTokenSource(requestAborted);
        sendTimeout.CancelAfter(TimeSpan.FromSeconds(10));
        var cancellationToken = sendTimeout.Token;
        try
        {
            await smtp.ConnectAsync(settings.Host, settings.Port,
                settings.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                cancellationToken);
            if (settings.UserName is { } userName && settings.Password is { } password)
            {
                await smtp.AuthenticateAsync(userName, password, cancellationToken);
            }

            await smtp.SendAsync(message, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
        }
        catch (OperationCanceledException) when (!requestAborted.IsCancellationRequested)
        {
            LogMailFailure(logger, "Timeout");
        }
        catch (Exception exception) when (exception is IOException or System.Net.Sockets.SocketException
            or SmtpCommandException or SmtpProtocolException or MailKit.Security.AuthenticationException
            or SslHandshakeException or NotSupportedException or ServiceNotConnectedException
            or ServiceNotAuthenticatedException)
        {
            // SMTP replies can contain private addresses. Log only the exception type.
            LogMailFailure(logger, exception.GetType().Name);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Identity email delivery failed ({FailureType}).")]
    private static partial void LogMailFailure(ILogger logger, string failureType);
}
