using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Handlers;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization;

/// <summary>
/// Testes unitários para PermissionRequirementHandler
/// Cobertura: HandleAsync com diferentes cenários de autorização
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Authorization")]
public class PermissionRequirementHandlerTests
{
    private readonly Mock<ILogger<PermissionRequirementHandler>> _loggerMock;
    private readonly PermissionRequirementHandler _sut;

    public PermissionRequirementHandlerTests()
    {
        _loggerMock = new Mock<ILogger<PermissionRequirementHandler>>();
        _sut = new PermissionRequirementHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithUnauthenticatedUser_ShouldFail()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // Not authenticated
        var requirement = new PermissionRequirement(EPermission.UsersRead);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await ((IAuthorizationHandler)_sut).HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithNullUser_ShouldFail()
    {
        // Arrange
        ClaimsPrincipal? user = null;
        var requirement = new PermissionRequirement(EPermission.UsersRead);
        var context = new AuthorizationHandlerContext([requirement], user!, null);

        // Act
        await ((IAuthorizationHandler)_sut).HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithNoUserId_ShouldFailAndLogWarning()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity([], "TestAuth"));
        var requirement = new PermissionRequirement(EPermission.UsersRead);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await ((IAuthorizationHandler)_sut).HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Could not extract user ID")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithUserHavingPermission_ShouldSucceed()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-123"),
            new(AuthConstants.Claims.Permission, "users:read")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PermissionRequirement(EPermission.UsersRead);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await ((IAuthorizationHandler)_sut).HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithUserLackingPermission_ShouldFail()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-123"),
            new(AuthConstants.Claims.Permission, "users:read")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PermissionRequirement(EPermission.UsersDelete); // Different permission
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await ((IAuthorizationHandler)_sut).HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithUserHavingPermission_ShouldLogSuccess()
    {
        // Arrange
        var userId = "user-123";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(AuthConstants.Claims.Permission, "users:create")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PermissionRequirement(EPermission.UsersCreate);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await ((IAuthorizationHandler)_sut).HandleAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("has required permission")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithUserLackingPermission_ShouldLogFailure()
    {
        // Arrange
        var userId = "user-123";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(AuthConstants.Claims.Permission, "users:read")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PermissionRequirement(EPermission.UsersDelete);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await ((IAuthorizationHandler)_sut).HandleAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("lacks required permission")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithSubClaim_ShouldExtractUserIdAndSucceed()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("sub", "user-456"), // Using 'sub' claim
            new(AuthConstants.Claims.Permission, "providers:read")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PermissionRequirement(EPermission.ProvidersRead);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await ((IAuthorizationHandler)_sut).HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithIdClaim_ShouldExtractUserIdAndSucceed()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("id", "user-789"), // Using 'id' claim
            new(AuthConstants.Claims.Permission, "providers:read")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PermissionRequirement(EPermission.ProvidersRead);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await ((IAuthorizationHandler)_sut).HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithMultiplePermissions_ShouldOnlyCheckRequiredOne()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-123"),
            new(AuthConstants.Claims.Permission, "users:read"),
            new(AuthConstants.Claims.Permission, "users:create"),
            new(AuthConstants.Claims.Permission, "providers:read")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PermissionRequirement(EPermission.UsersCreate);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await ((IAuthorizationHandler)_sut).HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithNameIdentifierPriority_ShouldUseNameIdentifier()
    {
        // Arrange - NameIdentifier should take priority
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-primary"),
            new("sub", "user-secondary"),
            new("id", "user-tertiary"),
            new(AuthConstants.Claims.Permission, "users:read")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PermissionRequirement(EPermission.UsersRead);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await ((IAuthorizationHandler)_sut).HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        // Verify log contains "user-primary" (the NameIdentifier value)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("user-primary")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCompleteTaskSynchronously()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-123"),
            new(AuthConstants.Claims.Permission, "users:read")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PermissionRequirement(EPermission.UsersRead);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        var task = ((IAuthorizationHandler)_sut).HandleAsync(context);

        // Assert
        task.IsCompleted.Should().BeTrue();
        await task; // Should complete immediately
    }
}
