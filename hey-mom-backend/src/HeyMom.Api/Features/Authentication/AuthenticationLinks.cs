using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace HeyMom.Api.Features.Authentication;

internal static class AuthenticationLinks
{
    public static string EmailConfirmation(string publicOrigin, string userId, string token) =>
        BuildFragmentLink(
            publicOrigin,
            "/web/confirm-email",
            new Dictionary<string, string?>
            {
                ["userId"] = userId,
                ["code"] = EncodeToken(token),
            });

    public static string PasswordReset(string publicOrigin, string email, string token) =>
        BuildFragmentLink(
            publicOrigin,
            "/web/reset-password",
            new Dictionary<string, string?>
            {
                ["email"] = email,
                ["code"] = EncodeToken(token),
            });

    public static string EmailChange(string publicOrigin, string userId, string newEmail, string token) =>
        BuildFragmentLink(
            publicOrigin,
            "/web/confirm-email-change",
            new Dictionary<string, string?>
            {
                ["userId"] = userId,
                ["newEmail"] = newEmail,
                ["code"] = EncodeToken(token),
            });

    private static string BuildFragmentLink(string publicOrigin, string path, Dictionary<string, string?> values)
    {
        var fragment = QueryHelpers.AddQueryString(string.Empty, values).TrimStart('?');
        return $"{publicOrigin.TrimEnd('/')}{path}#{fragment}";
    }

    private static string EncodeToken(string token) =>
        WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
}
