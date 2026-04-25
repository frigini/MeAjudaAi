using MeAjudaAi.Modules.Users.Application.Authorization;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Keycloak;
using MeAjudaAi.Shared.Authorization.ValueObjects;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Authorization;

[Trait("Category", "Unit")]
public class UsersPermissionResolverTests
{
    private readonly Mock<ILogger<UsersPermissionResolver>> _loggerMock = new();
    private readonly Mock<IKeycloakPermissionResolver> _keycloakResolverMock = new();

    private IConfiguration CreateConfiguration(bool useKeycloak)
    {
        var inMemorySettings = new Dictionary<string, string?> {
            {"Authorization:UseKeycloak", useKeycloak.ToString()}
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task ResolvePermissionsAsync_WithMock_ShouldReturnPermissionsByUserIdPattern()
    {
        // Arrange
        var configuration = CreateConfiguration(false);
        var sut = new UsersPermissionResolver(_loggerMock.Object, configuration, null);

        // Act
        var result = await sut.ResolvePermissionsAsync(new UserId(Guid.NewGuid()));

        // Assert
        result.Should().Contain(EPermission.UsersRead);
    }

    [Fact]
    public async Task ResolvePermissionsAsync_WithKeycloakEnabled_ShouldCallKeycloakResolver()
    {
        // Arrange
        var configuration = CreateConfiguration(true);
        var userId = new UserId(Guid.NewGuid());
        var permissions = new List<EPermission> { EPermission.UsersRead, EPermission.ProvidersRead };
        _keycloakResolverMock.Setup(r => r.ResolvePermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var sut = new UsersPermissionResolver(_loggerMock.Object, configuration, _keycloakResolverMock.Object);

        // Act
        var result = await sut.ResolvePermissionsAsync(userId);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(EPermission.UsersRead);
        result.Should().NotContain(EPermission.ProvidersRead); // Filtered out because not in Users module
    }

    [Fact]
    public void CanResolve_ShouldReturnTrue_ForUsersPermissions()
    {
        // Arrange
        var configuration = CreateConfiguration(false);
        var sut = new UsersPermissionResolver(_loggerMock.Object, configuration, null);

        // Act & Assert
        sut.CanResolve(EPermission.UsersRead).Should().BeTrue();
        sut.CanResolve(EPermission.ProvidersRead).Should().BeFalse();
    }

    [Fact]
    public async Task ResolvePermissionsAsync_WhenExceptionOccurs_ShouldReturnEmptyList()
    {
        // Arrange
        var configuration = CreateConfiguration(true);
        var userId = new UserId(Guid.NewGuid());
        _keycloakResolverMock.Setup(r => r.ResolvePermissionsAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Keycloak fail"));

        var sut = new UsersPermissionResolver(_loggerMock.Object, configuration, _keycloakResolverMock.Object);

        // Act
        var result = await sut.ResolvePermissionsAsync(userId);

        // Assert
        result.Should().BeEmpty();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to get permissions from Keycloak")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
