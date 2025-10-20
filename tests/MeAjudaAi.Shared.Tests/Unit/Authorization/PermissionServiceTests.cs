using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Authorization.Metrics;
using MeAjudaAi.Shared.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization;

/// <summary>
/// Testes unitários para o PermissionService.
/// </summary>
public class PermissionServiceTests
{
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<PermissionService>> _mockLogger;
    private readonly Mock<IPermissionMetricsService> _mockMetrics;
    private readonly PermissionService _permissionService;

    public PermissionServiceTests()
    {
        _mockCacheService = new Mock<ICacheService>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<PermissionService>>();
        _mockMetrics = new Mock<IPermissionMetricsService>();

        _permissionService = new PermissionService(
            _mockCacheService.Object,
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockMetrics.Object);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WithValidUserId_ShouldReturnPermissions()
    {
        // Arrange
        var userId = "test-user-123";
        var expectedPermissions = new List<EPermission> { EPermission.UsersRead, EPermission.UsersProfile };

        var mockProvider = new Mock<IPermissionProvider>();
        mockProvider.Setup(x => x.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPermissions);
        mockProvider.Setup(x => x.ModuleName).Returns("Users");

        _mockServiceProvider.Setup(x => x.GetService(typeof(IEnumerable<IPermissionProvider>)))
            .Returns(new[] { mockProvider.Object });

        _mockCacheService.Setup(x => x.GetOrCreateAsync<IReadOnlyList<EPermission>>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>>, TimeSpan, HybridCacheEntryOptions, IReadOnlyCollection<string>, CancellationToken>(
                async (key, factory, expiration, options, tags, ct) => await factory(ct));

        // Act
        var result = await _permissionService.GetUserPermissionsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPermissions.Count, result.Count);
        Assert.Contains(EPermission.UsersRead, result);
        Assert.Contains(EPermission.UsersProfile, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task GetUserPermissionsAsync_WithInvalidUserId_ShouldReturnEmpty(string invalidUserId)
    {
        // Act
        var result = await _permissionService.GetUserPermissionsAsync(invalidUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task HasPermissionAsync_WithUserHavingPermission_ShouldReturnTrue()
    {
        // Arrange
        var userId = "test-user-123";
        var permission = EPermission.UsersRead;
        var userPermissions = new List<EPermission> { EPermission.UsersRead, EPermission.UsersProfile };

        _mockCacheService.Setup(x => x.GetOrCreateAsync<IReadOnlyList<EPermission>>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPermissions);

        // Act
        var result = await _permissionService.HasPermissionAsync(userId, permission);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasPermissionAsync_WithUserNotHavingPermission_ShouldReturnFalse()
    {
        // Arrange
        var userId = "test-user-123";
        var permission = EPermission.AdminSystem;
        var userPermissions = new List<EPermission> { EPermission.UsersRead, EPermission.UsersProfile };

        _mockCacheService.Setup(x => x.GetOrCreateAsync<IReadOnlyList<EPermission>>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPermissions);

        // Act
        var result = await _permissionService.HasPermissionAsync(userId, permission);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionsAsync_WithRequireAllTrue_ShouldReturnTrueWhenUserHasAllPermissions()
    {
        // Arrange
        var userId = "test-user-123";
        var requiredPermissions = new[] { EPermission.UsersRead, EPermission.UsersProfile };
        var userPermissions = new List<EPermission> { EPermission.UsersRead, EPermission.UsersProfile, EPermission.UsersList };

        _mockCacheService.Setup(x => x.GetOrCreateAsync<IReadOnlyList<EPermission>>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPermissions);

        // Act
        var result = await _permissionService.HasPermissionsAsync(userId, requiredPermissions, requireAll: true);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasPermissionsAsync_WithRequireAllTrue_ShouldReturnFalseWhenUserMissingPermission()
    {
        // Arrange
        var userId = "test-user-123";
        var requiredPermissions = new[] { EPermission.UsersRead, EPermission.AdminSystem };
        var userPermissions = new List<EPermission> { EPermission.UsersRead, EPermission.UsersProfile };

        _mockCacheService.Setup(x => x.GetOrCreateAsync<IReadOnlyList<EPermission>>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPermissions);

        // Act
        var result = await _permissionService.HasPermissionsAsync(userId, requiredPermissions, requireAll: true);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionsAsync_WithRequireAllFalse_ShouldReturnTrueWhenUserHasAnyPermission()
    {
        // Arrange
        var userId = "test-user-123";
        var requiredPermissions = new[] { EPermission.UsersRead, EPermission.AdminSystem };
        var userPermissions = new List<EPermission> { EPermission.UsersRead, EPermission.UsersProfile };

        _mockCacheService.Setup(x => x.GetOrCreateAsync<IReadOnlyList<EPermission>>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPermissions);

        // Act
        var result = await _permissionService.HasPermissionsAsync(userId, requiredPermissions, requireAll: false);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetUserPermissionsByModuleAsync_ShouldReturnOnlyModulePermissions()
    {
        // Arrange
        var userId = "test-user-123";
        var module = "Users";
        var modulePermissions = new List<EPermission>
        {
            EPermission.UsersRead,
            EPermission.UsersProfile
        };

        var mockProvider = new Mock<IPermissionProvider>();
        mockProvider.Setup(x => x.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(modulePermissions);
        mockProvider.Setup(x => x.ModuleName).Returns(module);

        _mockServiceProvider.Setup(x => x.GetService(typeof(IEnumerable<IPermissionProvider>)))
            .Returns(new[] { mockProvider.Object });

        _mockCacheService.Setup(x => x.GetOrCreateAsync<IReadOnlyList<EPermission>>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>>, TimeSpan, HybridCacheEntryOptions, IReadOnlyCollection<string>, CancellationToken>(
                async (key, factory, expiration, options, tags, ct) => await factory(ct));

        // Act
        var result = await _permissionService.GetUserPermissionsByModuleAsync(userId, module);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, permission => Assert.Equal(module.ToLower(), permission.GetModule()));
    }

    [Fact]
    public async Task InvalidateUserPermissionsCacheAsync_ShouldCallCacheRemoval()
    {
        // Arrange
        var userId = "test-user-123";

        // Act
        await _permissionService.InvalidateUserPermissionsCacheAsync(userId);

        // Assert
        _mockCacheService.Verify(x => x.RemoveByTagAsync($"user:{userId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task InvalidateUserPermissionsCacheAsync_WithInvalidUserId_ShouldNotCallCache(string invalidUserId)
    {
        // Act
        await _permissionService.InvalidateUserPermissionsCacheAsync(invalidUserId);

        // Assert
        _mockCacheService.Verify(x => x.RemoveByPatternAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HasPermissionsAsync_WithEmptyPermissionsList_ShouldReturnTrue()
    {
        // Arrange
        var userId = "test-user-123";
        var emptyPermissions = Array.Empty<EPermission>();

        // Act
        var result = await _permissionService.HasPermissionsAsync(userId, emptyPermissions);

        // Assert
        Assert.True(result);
    }
}
