using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Handlers;
using MeAjudaAi.Shared.Authorization.Services;
using MeAjudaAi.Shared.Constants;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization;

/// <summary>
/// Testes unitários para PermissionClaimsTransformation
/// Cobertura: TransformAsync com diferentes cenários de autenticação e permissões
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Authorization")]
public class PermissionClaimsTransformationTests
{
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly Mock<ILogger<PermissionClaimsTransformation>> _loggerMock;
    private readonly PermissionClaimsTransformation _sut;

    public PermissionClaimsTransformationTests()
    {
        _permissionServiceMock = new Mock<IPermissionService>();
        _loggerMock = new Mock<ILogger<PermissionClaimsTransformation>>();
        _sut = new PermissionClaimsTransformation(_permissionServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task TransformAsync_WithUnauthenticatedUser_ShouldReturnPrincipalUnchanged()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // Not authenticated
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _sut.TransformAsync(principal);

        // Assert
        result.Should().BeSameAs(principal);
        _permissionServiceMock.Verify(s => s.GetUserPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TransformAsync_WithAlreadyProcessedClaims_ShouldReturnPrincipalUnchanged()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-123"),
            new(AuthConstants.Claims.Permission, "*") // Processing marker
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _sut.TransformAsync(principal);

        // Assert
        result.Should().BeSameAs(principal);
        _permissionServiceMock.Verify(s => s.GetUserPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TransformAsync_WithNoUserId_ShouldReturnPrincipalUnchangedAndLogWarning()
    {
        // Arrange
        var identity = new ClaimsIdentity([], "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _sut.TransformAsync(principal);

        // Assert
        result.Should().BeSameAs(principal);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unable to extract user ID")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TransformAsync_WithValidUser_ShouldAddPermissionClaims()
    {
        // Arrange
        var userId = "user-123";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var permissions = new List<EPermission>
        {
            EPermission.UsersRead,
            EPermission.UsersCreate
        };

        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        // Act
        var result = await _sut.TransformAsync(principal);

        // Assert
        result.Should().NotBeSameAs(principal);
        result.HasClaim(AuthConstants.Claims.Permission, "users:read").Should().BeTrue();
        result.HasClaim(AuthConstants.Claims.Permission, "users:create").Should().BeTrue();
        result.HasClaim(AuthConstants.Claims.Permission, "*").Should().BeTrue(); // Processing marker
    }

    [Fact]
    public async Task TransformAsync_WithValidUser_ShouldAddModuleClaims()
    {
        // Arrange
        var userId = "user-123";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var permissions = new List<EPermission>
        {
            EPermission.UsersRead,
            EPermission.ProvidersRead
        };

        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        // Act
        var result = await _sut.TransformAsync(principal);

        // Assert
        result.HasClaim(AuthConstants.Claims.Module, "users").Should().BeTrue();
        result.HasClaim(AuthConstants.Claims.Module, "providers").Should().BeTrue();
    }

    [Fact]
    public async Task TransformAsync_WithAdminPermission_ShouldAddSystemAdminClaim()
    {
        // Arrange
        var userId = "user-123";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var permissions = new List<EPermission>
        {
            EPermission.UsersRead,
            EPermission.AdminSystem // Admin permission (module = "admin")
        };

        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        // Act
        var result = await _sut.TransformAsync(principal);

        // Assert
        result.HasClaim(AuthConstants.Claims.IsSystemAdmin, "true").Should().BeTrue();
    }

    [Fact]
    public async Task TransformAsync_WithNoPermissions_ShouldReturnPrincipalUnchanged()
    {
        // Arrange
        var userId = "user-123";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.TransformAsync(principal);

        // Assert
        result.Should().BeSameAs(principal);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No permissions found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TransformAsync_WhenPermissionServiceThrows_ShouldReturnPrincipalUnchangedAndLogError()
    {
        // Arrange
        var userId = "user-123";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var exception = new InvalidOperationException("Service unavailable");
        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.TransformAsync(principal);

        // Assert
        result.Should().BeSameAs(principal);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to transform claims")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TransformAsync_WithSubjectClaim_ShouldExtractUserId()
    {
        // Arrange
        var userId = "user-456";
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Subject, userId) // Using 'sub' instead of NameIdentifier
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var permissions = new List<EPermission> { EPermission.UsersRead };
        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        // Act
        var result = await _sut.TransformAsync(principal);

        // Assert
        result.HasClaim(AuthConstants.Claims.Permission, "users:read").Should().BeTrue();
        _permissionServiceMock.Verify(s => s.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TransformAsync_WithIdClaim_ShouldExtractUserId()
    {
        // Arrange
        var userId = "user-789";
        var claims = new List<Claim>
        {
            new("id", userId) // Using 'id' claim
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var permissions = new List<EPermission> { EPermission.UsersRead };
        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        // Act
        var result = await _sut.TransformAsync(principal);

        // Assert
        result.HasClaim(AuthConstants.Claims.Permission, "users:read").Should().BeTrue();
        _permissionServiceMock.Verify(s => s.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TransformAsync_WithMultiplePermissions_ShouldLogCorrectCount()
    {
        // Arrange
        var userId = "user-123";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var permissions = new List<EPermission>
        {
            EPermission.UsersRead,
            EPermission.UsersCreate,
            EPermission.UsersUpdate,
            EPermission.UsersDelete
        };

        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        // Act
        await _sut.TransformAsync(principal);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Added 4 permission claims")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

