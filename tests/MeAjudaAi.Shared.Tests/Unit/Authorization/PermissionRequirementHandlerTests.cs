using System.Security.Claims;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Handlers;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization;

[Trait("Category", "Unit")]
public class PermissionRequirementHandlerTests
{
    private readonly Mock<ILogger<PermissionRequirementHandler>> _loggerMock;
    private readonly PermissionRequirementHandler _handler;

    public PermissionRequirementHandlerTests()
    {
        _loggerMock = new Mock<ILogger<PermissionRequirementHandler>>();
        _handler = new PermissionRequirementHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasPermissionClaim_ShouldSucceed()
    {
        // Arrange
        var permission = EPermission.UsersRead;
        var requirement = new PermissionRequirement(permission);
        
        var claims = new[] 
        { 
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(AuthConstants.Claims.Permission, permission.GetValue()) 
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenUserLacksPermissionClaim_ShouldFail()
    {
        // Arrange
        var requirement = new PermissionRequirement(EPermission.UsersCreate);
        
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldFail()
    {
        // Arrange
        var requirement = new PermissionRequirement(EPermission.UsersRead);
        var principal = new ClaimsPrincipal(new ClaimsIdentity()); // Not authenticated
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}
