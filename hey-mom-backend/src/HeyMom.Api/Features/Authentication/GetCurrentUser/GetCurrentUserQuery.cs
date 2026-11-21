using System.Security.Claims;
using HeyMom.Persistence.Entities;
using Microsoft.AspNetCore.Identity;

namespace HeyMom.Api.Features.Authentication.GetCurrentUser;

internal sealed class GetCurrentUserQuery(UserManager<ApplicationUser> users)
{
    public async Task<CurrentUserResponseDto?> ExecuteAsync(ClaimsPrincipal principal)
    {
        var user = await users.GetUserAsync(principal);
        return user?.Email is { } email ? new CurrentUserResponseDto(user.Id, email) : null;
    }
}
