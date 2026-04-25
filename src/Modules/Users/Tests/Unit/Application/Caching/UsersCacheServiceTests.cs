using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Services.Implementations;
using MeAjudaAi.Modules.Users.Application.Services.Interfaces;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Caching;

[Trait("Category", "Unit")]
public class UsersCacheServiceTests
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly UsersCacheService _usersCacheService;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public UsersCacheServiceTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _usersCacheService = new UsersCacheService(_cacheServiceMock.Object);
    }

    [Fact]
    public async Task GetOrCacheUserByIdAsync_ShouldCallCacheService_WithCorrectParameters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new UserDto(
            Id: userId,
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User",
            KeycloakId: "keycloak123",
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null
        );
        Func<CancellationToken, ValueTask<UserDto?>> factory = ct => ValueTask.FromResult<UserDto?>(expectedUser);

        // Setup GetAsync to return cache miss (not cached)
        _cacheServiceMock
            .Setup(x => x.GetAsync<UserDto?>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((default(UserDto), false));

        // Act
        var result = await _usersCacheService.GetOrCacheUserByIdAsync(userId, factory, _cancellationToken);

        // Assert
        result.Should().Be(expectedUser);
        _cacheServiceMock.Verify(
            x => x.GetAsync<UserDto?>(
                UsersCacheKeys.UserById(userId),
                _cancellationToken),
            Times.Once);

        var expectedTags = new HashSet<string> { CacheTags.Users, CacheTags.UserById, CacheTags.UserTag(userId), CacheTags.UsersList };
        _cacheServiceMock.Verify(
            x => x.SetAsync(
                UsersCacheKeys.UserById(userId),
                expectedUser,
                It.IsAny<TimeSpan?>(),
                It.IsAny<HybridCacheEntryOptions?>(),
                It.Is<IReadOnlyCollection<string>?>(tags => tags != null && new HashSet<string>(tags).SetEquals(expectedTags)),
                _cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetOrCacheUserByIdAsync_WhenCacheHit_ShouldReturnCachedValue_AndNotCallFactory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cachedUser = new UserDto(
            Id: userId,
            Username: "cacheduser",
            Email: "cached@example.com",
            FirstName: "Cached",
            LastName: "User",
            FullName: "Cached User",
            KeycloakId: "keycloak456",
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null
        );
        var factoryCalled = false;
        Func<CancellationToken, ValueTask<UserDto?>> factory = ct => {
            factoryCalled = true;
            return ValueTask.FromResult<UserDto?>(null);
        };

        _cacheServiceMock
            .Setup(x => x.GetAsync<UserDto?>(
                UsersCacheKeys.UserById(userId),
                _cancellationToken))
            .ReturnsAsync((cachedUser, true));

        // Act
        var result = await _usersCacheService.GetOrCacheUserByIdAsync(userId, factory, _cancellationToken);

        // Assert
        result.Should().Be(cachedUser);
        factoryCalled.Should().BeFalse();
        _cacheServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<UserDto>(), It.IsAny<TimeSpan?>(), It.IsAny<HybridCacheEntryOptions?>(), It.IsAny<IReadOnlyCollection<string>?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCacheUserByIdAsync_WhenFactoryReturnsNull_ShouldNotSetCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var factoryCalled = false;
        Func<CancellationToken, ValueTask<UserDto?>> factory = ct => 
        {
            factoryCalled = true;
            return ValueTask.FromResult<UserDto?>(null);
        };

        _cacheServiceMock
            .Setup(x => x.GetAsync<UserDto?>(
                UsersCacheKeys.UserById(userId),
                _cancellationToken))
            .ReturnsAsync((null, false));

        // Act
        var result = await _usersCacheService.GetOrCacheUserByIdAsync(userId, factory, _cancellationToken);

        // Assert
        result.Should().BeNull();
        factoryCalled.Should().BeTrue();
        _cacheServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<UserDto>(), It.IsAny<TimeSpan?>(), It.IsAny<HybridCacheEntryOptions?>(), It.IsAny<IReadOnlyCollection<string>?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCacheUserByIdAsync_WhenCachedFlagTrueButValueNull_ShouldCallFactoryAndCacheResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserDto(
            Id: userId,
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User",
            KeycloakId: "keycloak123",
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null
        );

        _cacheServiceMock.Setup(x => x.GetAsync<UserDto?>(
                UsersCacheKeys.UserById(userId),
                _cancellationToken))
            .ReturnsAsync((null, true));

        var factoryCalled = false;
        Func<CancellationToken, ValueTask<UserDto?>> factory = ct =>
        {
            factoryCalled = true;
            return ValueTask.FromResult<UserDto?>(user);
        };

        // Act
        var result = await _usersCacheService.GetOrCacheUserByIdAsync(userId, factory, _cancellationToken);

        // Assert
        result.Should().Be(user);
        factoryCalled.Should().BeTrue();

        var expectedTags = new HashSet<string> { CacheTags.Users, CacheTags.UserById, CacheTags.UserTag(userId), CacheTags.UsersList };
        _cacheServiceMock.Verify(
            x => x.SetAsync(
                UsersCacheKeys.UserById(userId),
                user,
                UsersCacheService.DefaultExpiration,
                It.IsAny<HybridCacheEntryOptions?>(),
                It.Is<IReadOnlyCollection<string>?>(tags => tags != null && new HashSet<string>(tags).SetEquals(expectedTags)),
                _cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetOrCacheSystemConfigAsync_ShouldCallCacheService_WithCorrectKey()
    {
        // Arrange
        var configData = new { Setting = "Value" };
        Func<CancellationToken, ValueTask<object>> factory = ct => ValueTask.FromResult<object>(configData);

        _cacheServiceMock
            .Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<object>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<HybridCacheEntryOptions?>(),
                It.IsAny<IReadOnlyCollection<string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(configData);

        // Act
        var result = await _usersCacheService.GetOrCacheSystemConfigAsync(factory, _cancellationToken);

        // Assert
        result.Should().Be(configData);
        _cacheServiceMock.Verify(
            x => x.GetOrCreateAsync(
                UsersCacheKeys.UserSystemConfig,
                It.IsAny<Func<CancellationToken, ValueTask<object>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<HybridCacheEntryOptions?>(),
                It.IsAny<IReadOnlyCollection<string>?>(),
                _cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task SetUserAsync_ShouldCallCacheService_WithCorrectParameters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserDto(
            Id: userId,
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User",
            KeycloakId: "keycloak123",
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null
        );

        // Act
        await _usersCacheService.SetUserAsync(user, _cancellationToken);

        // Assert
        var expectedTags = new HashSet<string> { CacheTags.Users, CacheTags.UserById, CacheTags.UserTag(userId), CacheTags.UserByEmail, CacheTags.UserEmailTag(user.Email), CacheTags.UsersList };
        _cacheServiceMock.Verify(
            x => x.SetAsync(
                UsersCacheKeys.UserById(userId),
                user,
                UsersCacheService.DefaultExpiration,
                It.IsAny<HybridCacheEntryOptions?>(),
                It.Is<IReadOnlyCollection<string>?>(tags => tags != null && new HashSet<string>(tags).SetEquals(expectedTags)),
                _cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task InvalidateUserAsync_ShouldRemoveUserSpecificCaches_WhenEmailNotProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        await _usersCacheService.InvalidateUserAsync(userId, cancellationToken: _cancellationToken);

        // Assert
        _cacheServiceMock.Verify(
            x => x.RemoveAsync(UsersCacheKeys.UserById(userId), _cancellationToken),
            Times.Once);

        _cacheServiceMock.Verify(
            x => x.RemoveAsync(UsersCacheKeys.UserRoles(userId), _cancellationToken),
            Times.Once);

        _cacheServiceMock.Verify(
            x => x.RemoveByPatternAsync(CacheTags.UsersList, _cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task InvalidateUserAsync_ShouldRemoveAllUserCaches_WhenEmailProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";

        // Act
        await _usersCacheService.InvalidateUserAsync(userId, email, _cancellationToken);

        // Assert
        _cacheServiceMock.Verify(
            x => x.RemoveAsync(UsersCacheKeys.UserById(userId), _cancellationToken),
            Times.Once);

        _cacheServiceMock.Verify(
            x => x.RemoveAsync(UsersCacheKeys.UserByEmail(email), _cancellationToken),
            Times.Once);

        _cacheServiceMock.Verify(
            x => x.RemoveAsync(UsersCacheKeys.UserRoles(userId), _cancellationToken),
            Times.Once);

        _cacheServiceMock.Verify(
            x => x.RemoveByPatternAsync(CacheTags.UsersList, _cancellationToken),
            Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task InvalidateUserAsync_ShouldNotRemoveEmailCache_WhenEmailIsNullOrEmpty(string? email)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        await _usersCacheService.InvalidateUserAsync(userId, email, _cancellationToken);

        // Assert
        _cacheServiceMock.Verify(
            x => x.RemoveAsync(It.Is<string>(key => key.Contains("email")), _cancellationToken),
            Times.Never);
    }

    [Fact]
    public async Task InvalidateUserAsync_ShouldRemoveEmailCache_WhenEmailIsWhitespace()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "   ";

        // Act
        await _usersCacheService.InvalidateUserAsync(userId, email, _cancellationToken);

        // Assert - espaços em branco não são considerados vazios por string.IsNullOrEmpty()
        _cacheServiceMock.Verify(
            x => x.RemoveAsync(UsersCacheKeys.UserByEmail(email), _cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task InvalidateUserAsync_ShouldHandleEmptyEmailGracefully()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert - não deve lançar exceção
        await _usersCacheService.InvalidateUserAsync(userId, "", _cancellationToken);
        await _usersCacheService.InvalidateUserAsync(userId, null, _cancellationToken);
        await _usersCacheService.InvalidateUserAsync(userId, "   ", _cancellationToken);

        // Verifica se a remoção básica do cache foi chamada para cada teste
        _cacheServiceMock.Verify(
            x => x.RemoveAsync(UsersCacheKeys.UserById(userId), _cancellationToken),
            Times.Exactly(3));
    }

    [Fact]
    public async Task GetOrCacheUserByIdAsync_ShouldUseCorrectCacheKey()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userData = new UserDto(
            Id: userId,
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User",
            KeycloakId: "keycloak123",
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null
        );
        Func<CancellationToken, ValueTask<UserDto?>> factory = ct => ValueTask.FromResult<UserDto?>(userData);

        // Setup GetAsync to return cache miss (not cached)
        _cacheServiceMock
            .Setup(x => x.GetAsync<UserDto?>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((default(UserDto), false));

        // Act
        await _usersCacheService.GetOrCacheUserByIdAsync(userId, factory, _cancellationToken);

        // Assert
        _cacheServiceMock.Verify(
            x => x.GetAsync<UserDto?>(
                UsersCacheKeys.UserById(userId),
                _cancellationToken),
            Times.Once);

        var expectedTags = new HashSet<string> { CacheTags.Users, CacheTags.UserById, CacheTags.UserTag(userId), CacheTags.UsersList };
        _cacheServiceMock.Verify(
            x => x.SetAsync(
                UsersCacheKeys.UserById(userId),
                userData,
                It.IsAny<TimeSpan?>(),
                It.IsAny<HybridCacheEntryOptions?>(),
                It.Is<IReadOnlyCollection<string>?>(tags => tags != null && new HashSet<string>(tags).SetEquals(expectedTags)),
                _cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetOrCacheSystemConfigAsync_ShouldUseCorrectConfigurationKey()
    {
        // Arrange
        var configData = new Dictionary<string, object> { { "MaxUsers", 1000 } };
        Func<CancellationToken, ValueTask<Dictionary<string, object>>> factory =
            ct => ValueTask.FromResult(configData);

        // Act
        await _usersCacheService.GetOrCacheSystemConfigAsync(factory, _cancellationToken);

        // Assert
        _cacheServiceMock.Verify(
            x => x.GetOrCreateAsync(
                UsersCacheKeys.UserSystemConfig,
                It.IsAny<Func<CancellationToken, ValueTask<Dictionary<string, object>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<HybridCacheEntryOptions?>(),
                It.IsAny<IReadOnlyCollection<string>?>(),
                _cancellationToken),
            Times.Once);
    }
}
