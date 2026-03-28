using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Metrics;
using MeAjudaAi.Shared.Authorization.Services;
using MeAjudaAi.Shared.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization;

[Trait("Category", "Unit")]
public class PermissionServiceTests
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<PermissionService>> _loggerMock;
    private readonly Mock<IPermissionMetricsService> _metricsMock;
    private readonly PermissionService _service;

    public PermissionServiceTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<PermissionService>>();
        _metricsMock = new Mock<IPermissionMetricsService>();

        // Setup metrics mock to return a disposal that doesn't crash
        _metricsMock.Setup(m => m.MeasurePermissionResolution(It.IsAny<string>())).Returns(new Mock<IDisposable>().Object);
        _metricsMock.Setup(m => m.MeasureCacheOperation(It.IsAny<string>(), It.IsAny<bool>())).Returns(new Mock<IDisposable>().Object);
        _metricsMock.Setup(m => m.MeasurePermissionCheck(It.IsAny<string>(), It.IsAny<EPermission>(), It.IsAny<bool>())).Returns(new Mock<IDisposable>().Object);

        _service = new PermissionService(
            _cacheServiceMock.Object,
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _metricsMock.Object);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WhenUserIdIsEmpty_ShouldReturnEmpty()
    {
        // Act
        var result = await _service.GetUserPermissionsAsync("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WhenCacheExists_ShouldReturnCachedValues()
    {
        // Arrange
        var userId = "user-123";
        var cachedPermissions = new[] { EPermission.UsersRead };
        
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<HybridCacheEntryOptions>(),
            It.IsAny<IReadOnlyCollection<string>?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedPermissions);

        // Act
        var result = await _service.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(cachedPermissions);
        _serviceProviderMock.Verify(s => s.GetService(typeof(IEnumerable<IPermissionProvider>)), Times.Never);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WhenCacheMiss_ShouldResolveFromProviders()
    {
        // Arrange
        var userId = "user-123";
        var permissions = new[] { EPermission.UsersRead, EPermission.SystemRead };
        
        var providerMock = new Mock<IPermissionProvider>();
        providerMock.Setup(p => p.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        _serviceProviderMock.Setup(s => s.GetService(typeof(IEnumerable<IPermissionProvider>)))
            .Returns(new[] { providerMock.Object });

        // Capture the factory passed to GetOrCreateAsync
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<HybridCacheEntryOptions>(),
            It.IsAny<IReadOnlyCollection<string>?>(),
            It.IsAny<CancellationToken>()))
            .Returns(async (string key, Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>> factory, TimeSpan? exp, HybridCacheEntryOptions opt, IReadOnlyCollection<string>? tags, CancellationToken ct) => 
                await factory(ct));

        // Act
        var result = await _service.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(permissions);
        providerMock.Verify(p => p.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HasPermissionAsync_WhenUserHasPermission_ShouldReturnTrue()
    {
        // Arrange
        var userId = "user-123";
        var permission = EPermission.UsersCreate;
        
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<HybridCacheEntryOptions>(),
            It.IsAny<IReadOnlyCollection<string>?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { EPermission.UsersCreate, EPermission.UsersRead });

        // Act
        var result = await _service.HasPermissionAsync(userId, permission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionsAsync_WhenRequireAllIsTrueAndAllPresent_ShouldReturnTrue()
    {
        // Arrange
        var userId = "user-123";
        var permissionsToCheck = new[] { EPermission.UsersRead, EPermission.UsersCreate };
        
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<HybridCacheEntryOptions>(),
            It.IsAny<IReadOnlyCollection<string>?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { EPermission.UsersRead, EPermission.UsersCreate, EPermission.SystemRead });

        // Act
        var result = await _service.HasPermissionsAsync(userId, permissionsToCheck, requireAll: true);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionsAsync_WhenRequireAllIsTrueAndSomeMissing_ShouldReturnFalse()
    {
        // Arrange
        var userId = "user-123";
        var permissionsToCheck = new[] { EPermission.UsersRead, EPermission.UsersDelete };
        
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<HybridCacheEntryOptions>(),
            It.IsAny<IReadOnlyCollection<string>?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { EPermission.UsersRead, EPermission.UsersCreate });

        // Act
        var result = await _service.HasPermissionsAsync(userId, permissionsToCheck, requireAll: true);

        // Assert
        result.Should().BeFalse();
        _metricsMock.Verify(m => m.RecordAuthorizationFailure(userId, It.IsAny<EPermission>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task HasPermissionsAsync_WhenRequireAllIsFalseAndAtLeastOnePresent_ShouldReturnTrue()
    {
        // Arrange
        var userId = "user-123";
        var permissionsToCheck = new[] { EPermission.UsersDelete, EPermission.UsersCreate };
        
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<HybridCacheEntryOptions>(),
            It.IsAny<IReadOnlyCollection<string>?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { EPermission.UsersRead, EPermission.UsersCreate });

        // Act
        var result = await _service.HasPermissionsAsync(userId, permissionsToCheck, requireAll: false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserPermissionsByModuleAsync_WhenModuleIsValid_ShouldReturnPermissions()
    {
        // Arrange
        var userId = "user-123";
        var module = "Documents";
        var expected = new[] { EPermission.DocumentsRead, EPermission.DocumentsCreate };
        
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<IReadOnlyList<EPermission>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<HybridCacheEntryOptions>(),
            It.IsAny<IReadOnlyCollection<string>?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _service.GetUserPermissionsByModuleAsync(userId, module);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task InvalidateUserPermissionsCacheAsync_ShouldCallCacheServiceWithCorrectTag()
    {
        // Arrange
        var userId = "user-123";

        // Act
        await _service.InvalidateUserPermissionsCacheAsync(userId);

        // Assert
        _cacheServiceMock.Verify(c => c.RemoveByTagAsync($"user:{userId}", It.IsAny<CancellationToken>()), Times.Once);
    }
}
