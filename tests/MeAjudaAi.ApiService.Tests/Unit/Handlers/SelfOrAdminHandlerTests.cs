using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.ApiService.Handlers;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace MeAjudaAi.ApiService.Tests.Unit.Handlers;

[Trait("Category", "Unit")]
[Trait("Layer", "ApiService")]
public class SelfOrAdminHandlerTests
{
    private readonly SelfOrAdminHandler _handler;
    private readonly SelfOrAdminRequirement _requirement;

    public SelfOrAdminHandlerTests()
    {
        _handler = new SelfOrAdminHandler();
        _requirement = new SelfOrAdminRequirement();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithUnauthenticatedUser_ShouldFail()
    {
        // Arrange
        var user = new ClaimsPrincipal();
        var resource = new DefaultHttpContext();
        var context = new AuthorizationHandlerContext(new[] { _requirement }, user, resource);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Theory]
    [InlineData(RoleConstants.Admin, AuthConstants.Claims.Roles)]
    [InlineData(RoleConstants.SystemAdmin, AuthConstants.Claims.Roles)]
    [InlineData(RoleConstants.SuperAdmin, AuthConstants.Claims.Roles)]
    [InlineData(RoleConstants.Admin, ClaimTypes.Role)]
    [InlineData(RoleConstants.SystemAdmin, ClaimTypes.Role)]
    [InlineData(RoleConstants.SuperAdmin, ClaimTypes.Role)]
    [InlineData(RoleConstants.LegacySystemAdmin, AuthConstants.Claims.Roles)]
    [InlineData(RoleConstants.LegacySuperAdmin, ClaimTypes.Role)]
    public async Task HandleRequirementAsync_WithAdminRole_ShouldSucceed(string adminRole, string claimType)
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(AuthConstants.Claims.Subject, "user123"),
            new Claim(claimType, adminRole)
        };
        
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);
        
        var resource = new DefaultHttpContext();
        var context = new AuthorizationHandlerContext([_requirement], user, resource);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithMatchingUserId_ShouldSucceed()
    {
        // Arrange
        var userId = "user123";
        var claims = new[]
        {
            new Claim(AuthConstants.Claims.Subject, userId)
        };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["userId"] = userId;

        var context = new AuthorizationHandlerContext([_requirement], user, httpContext);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithDifferentUserId_ShouldFail()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(AuthConstants.Claims.Subject, "user123")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["userId"] = "differentUser";

        var context = new AuthorizationHandlerContext([_requirement], user, httpContext);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithoutUserIdClaim_ShouldFail()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "test")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);
        var resource = new DefaultHttpContext();
        var context = new AuthorizationHandlerContext([_requirement], user, resource);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNullResource_ShouldFail()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(AuthConstants.Claims.Subject, "user123")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext([_requirement], user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public void SelfOrAdminRequirement_ShouldImplementIAuthorizationRequirement()
    {
        // Arrange & Act
        var requirement = new SelfOrAdminRequirement();

        // Assert
        requirement.Should().BeAssignableTo<IAuthorizationRequirement>();
    }
}
