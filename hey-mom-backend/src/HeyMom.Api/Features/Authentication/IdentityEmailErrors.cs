using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace HeyMom.Api.Features.Authentication;

internal static class IdentityEmailErrors
{
    public static bool IsUniqueEmailViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "EmailIndex",
        };

    public static IdentityError DuplicateEmail() => new()
    {
        Code = "DuplicateEmail",
        Description = "This email address is already in use.",
    };
}
