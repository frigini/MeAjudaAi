using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

public static class EndpointTestHelpers
{
    public static DefaultHttpContext CreateHttpContextWithUserId(Guid userId)
    {
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        context.User = new ClaimsPrincipal(identity);
        return context;
    }
}
