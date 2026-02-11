using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

public static class EndpointTestHelpers
{
    public static DefaultHttpContext CreateHttpContextWithUserId(Guid userId)
    {
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim(AuthConstants.Claims.Subject, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        context.User = new ClaimsPrincipal(identity);
        return context;
    }
}
