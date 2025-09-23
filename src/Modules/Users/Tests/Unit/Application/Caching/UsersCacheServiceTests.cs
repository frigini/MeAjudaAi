using MeAjudaAi.Modules.Users.Application.Caching;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Caching;
using Microsoft.Extensions.Caching.Hybrid;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Caching;

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

        _cacheServiceMock
            .Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<HybridCacheEntryOptions?>(),
                It.IsAny<IReadOnlyCollection<string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _usersCacheService.GetOrCacheUserByIdAsync(userId, factory, _cancellationToken);

        // Assert
        result.Should().Be(expectedUser);
        _cacheServiceMock.Verify(
            x => x.GetOrCreateAsync(
                UsersCacheKeys.UserById(userId),
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<HybridCacheEntryOptions?>(),
                It.IsAny<IReadOnlyCollection<string>?>(),
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

        // Assert - whitespace is not considered empty by string.IsNullOrEmpty()
        _cacheServiceMock.Verify(
            x => x.RemoveAsync(UsersCacheKeys.UserByEmail(email), _cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task InvalidateUserAsync_ShouldHandleEmptyEmailGracefully()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert - should not throw
        await _usersCacheService.InvalidateUserAsync(userId, "", _cancellationToken);
        await _usersCacheService.InvalidateUserAsync(userId, null, _cancellationToken);
        await _usersCacheService.InvalidateUserAsync(userId, "   ", _cancellationToken);

        // Verify basic cache removal was called for each test
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

        // Act
        await _usersCacheService.GetOrCacheUserByIdAsync(userId, factory, _cancellationToken);

        // Assert
        _cacheServiceMock.Verify(
            x => x.GetOrCreateAsync(
                UsersCacheKeys.UserById(userId),
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<HybridCacheEntryOptions?>(),
                It.IsAny<IReadOnlyCollection<string>?>(),
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