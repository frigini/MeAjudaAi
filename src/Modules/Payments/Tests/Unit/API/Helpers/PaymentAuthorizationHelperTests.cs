using System.Security.Claims;
using MeAjudaAi.Modules.Payments.API.Helpers;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.API.Helpers;

public class PaymentAuthorizationHelperTests
{
    private readonly Guid _providerId = Guid.NewGuid();

    [Fact]
    public void AuthorizeProviderAccess_WhenSystemAdmin_ShouldReturnNull()
    {
        var httpContext = CreateHttpContext(new Claim(AuthConstants.Claims.IsSystemAdmin, "true"));

        var result = PaymentAuthorizationHelper.AuthorizeProviderAccess(httpContext, _providerId);

        result.Should().BeNull();
    }

    [Fact]
    public void AuthorizeProviderAccess_WhenSystemAdminCaseInsensitive_ShouldReturnNull()
    {
        var httpContext = CreateHttpContext(new Claim(AuthConstants.Claims.IsSystemAdmin, "TRUE"));

        var result = PaymentAuthorizationHelper.AuthorizeProviderAccess(httpContext, _providerId);

        result.Should().BeNull();
    }

    [Fact]
    public void AuthorizeProviderAccess_WhenMatchingProviderId_ShouldReturnNull()
    {
        var httpContext = CreateHttpContext(new Claim(AuthConstants.Claims.ProviderId, _providerId.ToString()));

        var result = PaymentAuthorizationHelper.AuthorizeProviderAccess(httpContext, _providerId);

        result.Should().BeNull();
    }

    [Fact]
    public void AuthorizeProviderAccess_WhenDifferentProviderId_ShouldReturnForbid()
    {
        var differentProviderId = Guid.NewGuid();
        var httpContext = CreateHttpContext(new Claim(AuthConstants.Claims.ProviderId, differentProviderId.ToString()));

        var result = PaymentAuthorizationHelper.AuthorizeProviderAccess(httpContext, _providerId);

        result.Should().BeOfType<ForbidHttpResult>();
    }

    [Fact]
    public void AuthorizeProviderAccess_WhenNoProviderIdClaim_ShouldReturnUnauthorized()
    {
        var httpContext = CreateHttpContext();

        var result = PaymentAuthorizationHelper.AuthorizeProviderAccess(httpContext, _providerId);

        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public void AuthorizeProviderAccess_WhenEmptyProviderIdClaim_ShouldReturnUnauthorized()
    {
        var httpContext = CreateHttpContext(new Claim(AuthConstants.Claims.ProviderId, ""));

        var result = PaymentAuthorizationHelper.AuthorizeProviderAccess(httpContext, _providerId);

        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public void AuthorizeProviderAccess_WhenInvalidProviderIdClaim_ShouldReturnForbid()
    {
        var httpContext = CreateHttpContext(new Claim(AuthConstants.Claims.ProviderId, "invalid-guid"));

        var result = PaymentAuthorizationHelper.AuthorizeProviderAccess(httpContext, _providerId);

        result.Should().BeOfType<ForbidHttpResult>();
    }

    [Fact]
    public void AuthorizeProviderAccess_WhenNullUser_ShouldReturnUnauthorized()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };

        var result = PaymentAuthorizationHelper.AuthorizeProviderAccess(httpContext, _providerId);

        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public void AuthorizeProviderAccess_WhenAdminAndMatchingProviderId_ShouldReturnNull()
    {
        var httpContext = CreateHttpContext(
            new Claim(AuthConstants.Claims.IsSystemAdmin, "true"),
            new Claim(AuthConstants.Claims.ProviderId, _providerId.ToString()));

        var result = PaymentAuthorizationHelper.AuthorizeProviderAccess(httpContext, _providerId);

        result.Should().BeNull();
    }

    [Fact]
    public void AuthorizeProviderAccess_WhenNotAdminAndNoProviderId_ShouldReturnUnauthorized()
    {
        var httpContext = CreateHttpContext(new Claim(AuthConstants.Claims.IsSystemAdmin, "false"));

        var result = PaymentAuthorizationHelper.AuthorizeProviderAccess(httpContext, _providerId);

        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    private static HttpContext CreateHttpContext(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        return new DefaultHttpContext { User = principal };
    }
}
