using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Shared.Utilities;

public static class ClaimHelpers
{
    public static string? GetUserId(ClaimsPrincipal? user)
    {
        return user?.FindFirst("sub")?.Value 
               ?? user?.FindFirst("id")?.Value 
               ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public static Guid? GetUserIdGuid(ClaimsPrincipal? user)
    {
        var id = GetUserId(user);
        return Guid.TryParse(id, out var guid) ? guid : null;
    }

    public static Guid? GetUserIdGuid(HttpContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        return GetUserIdGuid(context.User);
    }
}
