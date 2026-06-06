using FluentAssertions;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Keycloak;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization.Keycloak;

public class KeycloakPermissionProviderTests
{
    private readonly Mock<IKeycloakPermissionResolver> _mockResolver;
    private readonly Mock<ILogger<KeycloakPermissionProvider>> _mockLogger;
    private readonly KeycloakPermissionProvider _provider;

    public KeycloakPermissionProviderTests()
    {
        _mockResolver = new Mock<IKeycloakPermissionResolver>();
        _mockLogger = new Mock<ILogger<KeycloakPermissionProvider>>();
        _provider = new KeycloakPermissionProvider(_mockResolver.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_EmptyUserId_ReturnsEmpty()
    {
        var result = await _provider.GetUserPermissionsAsync(string.Empty);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ResolverReturnsPermissions_ReturnsPermissions()
    {
        var userId = Guid.NewGuid().ToString();
        var permissions = new List<EPermission> { EPermission.UsersRead };
        _mockResolver.Setup(r => r.ResolvePermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var result = await _provider.GetUserPermissionsAsync(userId);
        result.Should().BeEquivalentTo(permissions);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_HttpRequestException_ReturnsEmpty()
    {
        var userId = Guid.NewGuid().ToString();
        _mockResolver.Setup(r => r.ResolvePermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var result = await _provider.GetUserPermissionsAsync(userId);
        result.Should().BeEmpty();
        _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_JsonException_ReturnsEmpty()
    {
        var userId = Guid.NewGuid().ToString();
        _mockResolver.Setup(r => r.ResolvePermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new JsonException("Deserialization error"));

        var result = await _provider.GetUserPermissionsAsync(userId);
        result.Should().BeEmpty();
        _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_UnexpectedException_Throws()
    {
        var userId = Guid.NewGuid().ToString();
        _mockResolver.Setup(r => r.ResolvePermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected"));

        await Assert.ThrowsAsync<Exception>(() => _provider.GetUserPermissionsAsync(userId));
    }
}
