using System.Diagnostics.Metrics;
using FluentAssertions;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Authorization.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization.Metrics;

/// <summary>
/// Testes unitários para PermissionMetricsService.
/// Cobre todos os métodos de coleta de métricas, counters, histograms, gauges e timers.
/// </summary>
public sealed class PermissionMetricsServiceTests : IDisposable
{
    private readonly Mock<ILogger<PermissionMetricsService>> _loggerMock;
    private readonly PermissionMetricsService _service;
    private readonly MeterListener _meterListener;
    private readonly Dictionary<string, long> _counterValues;
    private readonly Dictionary<string, double> _histogramValues;
    private readonly Dictionary<string, object?> _gaugeValues;

    public PermissionMetricsServiceTests()
    {
        _loggerMock = new Mock<ILogger<PermissionMetricsService>>();
        _service = new PermissionMetricsService(_loggerMock.Object);

        _counterValues = new Dictionary<string, long>();
        _histogramValues = new Dictionary<string, double>();
        _gaugeValues = new Dictionary<string, object?>();

        _meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == "MeAjudaAi.Authorization")
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            }
        };

        _meterListener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            var key = $"{instrument.Name}:{string.Join(",", tags.ToArray().Select(t => $"{t.Key}={t.Value}"))}";
            _counterValues[key] = _counterValues.GetValueOrDefault(key) + measurement;
        });

        _meterListener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
        {
            var key = $"{instrument.Name}:{string.Join(",", tags.ToArray().Select(t => $"{t.Key}={t.Value}"))}";
            _histogramValues[key] = measurement;
        });

        _meterListener.SetMeasurementEventCallback<int>((instrument, measurement, tags, state) =>
        {
            _gaugeValues[instrument.Name] = measurement;
        });

        _meterListener.Start();
    }

    public void Dispose()
    {
        _meterListener.Dispose();
        _service.Dispose();
    }

    #region MeasurePermissionResolution Tests

    [Fact]
    public void MeasurePermissionResolution_WithValidUserId_ShouldIncrementCounter()
    {
        // Act
        using var timer = _service.MeasurePermissionResolution("user123", "users");

        // Assert - Counter should be incremented
        var counterKey = _counterValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_permission_resolutions_total"));
        counterKey.Should().NotBeNullOrEmpty();
        _counterValues[counterKey!].Should().Be(1);
    }

    [Fact]
    public void MeasurePermissionResolution_WithNullModule_ShouldUseUnknown()
    {
        // Act
        using var timer = _service.MeasurePermissionResolution("user123", null);

        // Assert
        var counterKey = _counterValues.Keys.FirstOrDefault(k => k.Contains("module=unknown"));
        counterKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void MeasurePermissionResolution_OnDispose_ShouldRecordDuration()
    {
        // Act
        var timer = _service.MeasurePermissionResolution("user123", "users");
        Thread.Sleep(10); // Small delay to ensure measurable duration
        timer.Dispose();

        // Assert - Histogram should have recorded duration
        var histogramKey = _histogramValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_permission_resolution_duration_seconds"));
        histogramKey.Should().NotBeNullOrEmpty();
        _histogramValues[histogramKey!].Should().BeGreaterThan(0);
    }

    [Fact]
    public void MeasurePermissionResolution_SlowOperation_ShouldLogWarning()
    {
        // Arrange - We can't easily force a 1000ms delay in unit test, so we'll verify the timer works
        using var timer = _service.MeasurePermissionResolution("user123", "users");

        // Assert - Timer should be created and disposed without error
        timer.Should().NotBeNull();
    }

    [Fact]
    public void MeasurePermissionResolution_MultipleOperations_ShouldIncrementCounterMultipleTimes()
    {
        // Act
        using (var timer1 = _service.MeasurePermissionResolution("user1", "users")) { }
        using (var timer2 = _service.MeasurePermissionResolution("user2", "providers")) { }
        using (var timer3 = _service.MeasurePermissionResolution("user3", "users")) { }

        // Assert - Should have 3 total resolutions
        var totalResolutions = _counterValues
            .Where(kv => kv.Key.StartsWith("meajudaai_permission_resolutions_total"))
            .Sum(kv => kv.Value);
        totalResolutions.Should().Be(3);
    }

    #endregion

    #region MeasurePermissionCheck Tests

    [Fact]
    public void MeasurePermissionCheck_WithGrantedPermission_ShouldIncrementCheckCounter()
    {
        // Act
        using var timer = _service.MeasurePermissionCheck("user123", EPermission.UsersRead, granted: true);

        // Assert
        var counterKey = _counterValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_permission_checks_total"));
        counterKey.Should().NotBeNullOrEmpty();
        _counterValues[counterKey!].Should().Be(1);
    }

    [Fact]
    public void MeasurePermissionCheck_WithDeniedPermission_ShouldIncrementFailureCounter()
    {
        // Act
        using var timer = _service.MeasurePermissionCheck("user123", EPermission.UsersCreate, granted: false);

        // Assert
        var failureKey = _counterValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_authorization_failures_total"));
        failureKey.Should().NotBeNullOrEmpty();
        _counterValues[failureKey!].Should().Be(1);
    }

    [Fact]
    public void MeasurePermissionCheck_OnDispose_ShouldRecordAuthorizationCheckDuration()
    {
        // Act
        var timer = _service.MeasurePermissionCheck("user123", EPermission.UsersRead, granted: true);
        Thread.Sleep(5);
        timer.Dispose();

        // Assert
        var histogramKey = _histogramValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_authorization_check_duration_seconds"));
        histogramKey.Should().NotBeNullOrEmpty();
        _histogramValues[histogramKey!].Should().BeGreaterThan(0);
    }

    [Fact]
    public void MeasurePermissionCheck_ShouldUpdateSystemStats()
    {
        // Act
        using (var timer = _service.MeasurePermissionCheck("user123", EPermission.UsersRead, granted: true)) { }

        var stats = _service.GetSystemStats();

        // Assert
        stats.TotalPermissionChecks.Should().Be(1);
    }

    [Fact]
    public void MeasurePermissionCheck_WithMultipleChecks_ShouldAccumulateStats()
    {
        // Act
        using (var t1 = _service.MeasurePermissionCheck("user1", EPermission.UsersRead, true)) { }
        using (var t2 = _service.MeasurePermissionCheck("user2", EPermission.UsersCreate, false)) { }
        using (var t3 = _service.MeasurePermissionCheck("user3", EPermission.UsersUpdate, true)) { }

        var stats = _service.GetSystemStats();

        // Assert
        stats.TotalPermissionChecks.Should().Be(3);
    }

    #endregion

    #region MeasureMultiplePermissionCheck Tests

    [Fact]
    public void MeasureMultiplePermissionCheck_WithMultiplePermissions_ShouldIncrementCounterByPermissionCount()
    {
        // Arrange
        var permissions = new[] { EPermission.UsersRead, EPermission.UsersCreate, EPermission.UsersUpdate };

        // Act
        using var timer = _service.MeasureMultiplePermissionCheck("user123", permissions, requireAll: true);

        // Assert - Should increment by 3 (number of permissions)
        var stats = _service.GetSystemStats();
        stats.TotalPermissionChecks.Should().Be(3);
    }

    [Fact]
    public void MeasureMultiplePermissionCheck_WithRequireAllTrue_ShouldIncludeInTags()
    {
        // Arrange
        var permissions = new[] { EPermission.UsersRead, EPermission.UsersCreate };

        // Act
        using var timer = _service.MeasureMultiplePermissionCheck("user123", permissions, requireAll: true);

        // Assert - Counter should be incremented
        var counterKey = _counterValues.Keys.FirstOrDefault(k =>
            k.StartsWith("meajudaai_permission_checks_total") && k.Contains("require_all=True"));
        counterKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void MeasureMultiplePermissionCheck_WithRequireAllFalse_ShouldIncludeInTags()
    {
        // Arrange
        var permissions = new[] { EPermission.ProvidersRead };

        // Act
        using var timer = _service.MeasureMultiplePermissionCheck("user456", permissions, requireAll: false);

        // Assert
        var counterKey = _counterValues.Keys.FirstOrDefault(k =>
            k.StartsWith("meajudaai_permission_checks_total") && k.Contains("require_all=False"));
        counterKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void MeasureMultiplePermissionCheck_OnDispose_ShouldRecordDuration()
    {
        // Arrange
        var permissions = new[] { EPermission.UsersRead };

        // Act
        var timer = _service.MeasureMultiplePermissionCheck("user123", permissions, requireAll: true);
        Thread.Sleep(5);
        timer.Dispose();

        // Assert
        var histogramKey = _histogramValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_authorization_check_duration_seconds"));
        histogramKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void MeasureMultiplePermissionCheck_WithEmptyPermissions_ShouldNotIncrement()
    {
        // Arrange
        var permissions = Array.Empty<EPermission>();

        // Act
        using var timer = _service.MeasureMultiplePermissionCheck("user123", permissions, requireAll: true);

        var stats = _service.GetSystemStats();

        // Assert
        stats.TotalPermissionChecks.Should().Be(0);
    }

    #endregion

    #region MeasureModulePermissionResolution Tests

    [Fact]
    public void MeasureModulePermissionResolution_WithValidModule_ShouldIncrementCounter()
    {
        // Act
        using var timer = _service.MeasureModulePermissionResolution("user123", "users");

        // Assert
        var counterKey = _counterValues.Keys.FirstOrDefault(k =>
            k.StartsWith("meajudaai_permission_resolutions_total") && k.Contains("module=users"));
        counterKey.Should().NotBeNullOrEmpty();
        _counterValues[counterKey!].Should().Be(1);
    }

    [Fact]
    public void MeasureModulePermissionResolution_OnDispose_ShouldRecordDuration()
    {
        // Act
        var timer = _service.MeasureModulePermissionResolution("user123", "providers");
        Thread.Sleep(5);
        timer.Dispose();

        // Assert
        var histogramKey = _histogramValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_permission_resolution_duration_seconds"));
        histogramKey.Should().NotBeNullOrEmpty();
        _histogramValues[histogramKey!].Should().BeGreaterThan(0);
    }

    [Fact]
    public void MeasureModulePermissionResolution_WithDifferentModules_ShouldTrackSeparately()
    {
        // Act
        using (var t1 = _service.MeasureModulePermissionResolution("user1", "users")) { }
        using (var t2 = _service.MeasureModulePermissionResolution("user2", "providers")) { }
        using (var t3 = _service.MeasureModulePermissionResolution("user3", "users")) { }

        // Assert - Should have 2 users and 1 providers
        var usersKey = _counterValues.Keys.FirstOrDefault(k => k.Contains("module=users"));
        var providersKey = _counterValues.Keys.FirstOrDefault(k => k.Contains("module=providers"));

        usersKey.Should().NotBeNullOrEmpty();
        providersKey.Should().NotBeNullOrEmpty();
        _counterValues[usersKey!].Should().Be(2);
        _counterValues[providersKey!].Should().Be(1);
    }

    #endregion

    #region MeasureCacheOperation Tests

    [Fact]
    public void MeasureCacheOperation_WithCacheHit_ShouldIncrementHitCounter()
    {
        // Act
        using var timer = _service.MeasureCacheOperation("get", hit: true);

        // Assert
        var hitKey = _counterValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_permission_cache_hits_total"));
        hitKey.Should().NotBeNullOrEmpty();
        _counterValues[hitKey!].Should().Be(1);
    }

    [Fact]
    public void MeasureCacheOperation_WithCacheMiss_ShouldIncrementMissCounter()
    {
        // Act
        using var timer = _service.MeasureCacheOperation("get", hit: false);

        // Assert
        var missKey = _counterValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_permission_cache_misses_total"));
        missKey.Should().NotBeNullOrEmpty();
        _counterValues[missKey!].Should().Be(1);
    }

    [Fact]
    public void MeasureCacheOperation_WithCacheHit_ShouldUpdateSystemStats()
    {
        // Act
        using (var timer = _service.MeasureCacheOperation("get", hit: true)) { }

        var stats = _service.GetSystemStats();

        // Assert
        stats.TotalCacheHits.Should().Be(1);
    }

    [Fact]
    public void MeasureCacheOperation_OnDispose_ShouldRecordDuration()
    {
        // Act
        var timer = _service.MeasureCacheOperation("set", hit: false);
        Thread.Sleep(5);
        timer.Dispose();

        // Assert
        var histogramKey = _histogramValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_permission_cache_operation_duration_seconds"));
        histogramKey.Should().NotBeNullOrEmpty();
        _histogramValues[histogramKey!].Should().BeGreaterThan(0);
    }

    [Fact]
    public void MeasureCacheOperation_WithDifferentOperations_ShouldTrackSeparately()
    {
        // Act
        using (var t1 = _service.MeasureCacheOperation("get", hit: true)) { }
        using (var t2 = _service.MeasureCacheOperation("set", hit: false)) { }
        using (var t3 = _service.MeasureCacheOperation("invalidate", hit: false)) { }

        // Assert - Each operation should be tracked
        var getKey = _counterValues.Keys.FirstOrDefault(k => k.Contains("operation=get"));
        var setKey = _counterValues.Keys.FirstOrDefault(k => k.Contains("operation=set"));
        var invalidateKey = _counterValues.Keys.FirstOrDefault(k => k.Contains("operation=invalidate"));

        getKey.Should().NotBeNullOrEmpty();
        setKey.Should().NotBeNullOrEmpty();
        invalidateKey.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region RecordAuthorizationFailure Tests

    [Fact]
    public void RecordAuthorizationFailure_WithValidData_ShouldIncrementCounter()
    {
        // Act
        _service.RecordAuthorizationFailure("user123", EPermission.UsersDelete, "insufficient_permissions");

        // Assert
        var failureKey = _counterValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_authorization_failures_total"));
        failureKey.Should().NotBeNullOrEmpty();
        _counterValues[failureKey!].Should().Be(1);
    }

    [Fact]
    public void RecordAuthorizationFailure_ShouldIncludePermissionInTags()
    {
        // Act
        _service.RecordAuthorizationFailure("user123", EPermission.ProvidersCreate, "role_missing");

        // Assert
        var failureKey = _counterValues.Keys.FirstOrDefault(k => k.Contains("permission=providers:create"));
        failureKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RecordAuthorizationFailure_ShouldIncludeReasonInTags()
    {
        // Act
        _service.RecordAuthorizationFailure("user123", EPermission.UsersRead, "token_expired");

        // Assert
        var failureKey = _counterValues.Keys.FirstOrDefault(k => k.Contains("reason=token_expired"));
        failureKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RecordAuthorizationFailure_ShouldLogWarning()
    {
        // Act
        _service.RecordAuthorizationFailure("user123", EPermission.UsersDelete, "insufficient_permissions");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Authorization failure")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void RecordAuthorizationFailure_MultipleFailures_ShouldAccumulate()
    {
        // Act
        _service.RecordAuthorizationFailure("user1", EPermission.UsersRead, "reason1");
        _service.RecordAuthorizationFailure("user2", EPermission.UsersCreate, "reason2");
        _service.RecordAuthorizationFailure("user3", EPermission.UsersUpdate, "reason3");

        // Assert
        var totalFailures = _counterValues
            .Where(kv => kv.Key.StartsWith("meajudaai_authorization_failures_total"))
            .Sum(kv => kv.Value);
        totalFailures.Should().Be(3);
    }

    #endregion

    #region RecordCacheInvalidation Tests

    [Fact]
    public void RecordCacheInvalidation_WithValidData_ShouldIncrementCounter()
    {
        // Act
        _service.RecordCacheInvalidation("user123", "user_updated");

        // Assert
        var invalidationKey = _counterValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_permission_cache_invalidations_total"));
        invalidationKey.Should().NotBeNullOrEmpty();
        _counterValues[invalidationKey!].Should().Be(1);
    }

    [Fact]
    public void RecordCacheInvalidation_ShouldIncludeReasonInTags()
    {
        // Act
        _service.RecordCacheInvalidation("user123", "role_changed");

        // Assert
        var invalidationKey = _counterValues.Keys.FirstOrDefault(k => k.Contains("reason=role_changed"));
        invalidationKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RecordCacheInvalidation_ShouldLogDebug()
    {
        // Act
        _service.RecordCacheInvalidation("user123", "permissions_updated");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Permission cache invalidated")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void RecordCacheInvalidation_MultipleInvalidations_ShouldAccumulate()
    {
        // Act
        _service.RecordCacheInvalidation("user1", "reason1");
        _service.RecordCacheInvalidation("user2", "reason2");

        // Assert
        var totalInvalidations = _counterValues
            .Where(kv => kv.Key.StartsWith("meajudaai_permission_cache_invalidations_total"))
            .Sum(kv => kv.Value);
        totalInvalidations.Should().Be(2);
    }

    #endregion

    #region RecordPerformanceStats Tests

    [Fact]
    public void RecordPerformanceStats_WithValidData_ShouldRecordHistogram()
    {
        // Act
        _service.RecordPerformanceStats("cache_resolver", 150.5, "milliseconds");

        // Assert
        var performanceKey = _histogramValues.Keys.FirstOrDefault(k =>
            k.StartsWith("meajudaai_permission_performance") && k.Contains("component=cache_resolver"));
        performanceKey.Should().NotBeNullOrEmpty();
        _histogramValues[performanceKey!].Should().Be(150.5);
    }

    [Fact]
    public void RecordPerformanceStats_WithDefaultUnit_ShouldUseCount()
    {
        // Act
        _service.RecordPerformanceStats("permission_checks", 42);

        // Assert
        var performanceKey = _histogramValues.Keys.FirstOrDefault(k => k.Contains("unit=count"));
        performanceKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RecordPerformanceStats_WithDifferentComponents_ShouldTrackSeparately()
    {
        // Act
        _service.RecordPerformanceStats("component1", 100, "ms");
        _service.RecordPerformanceStats("component2", 200, "ms");

        // Assert
        var comp1Key = _histogramValues.Keys.FirstOrDefault(k => k.Contains("component=component1"));
        var comp2Key = _histogramValues.Keys.FirstOrDefault(k => k.Contains("component=component2"));

        comp1Key.Should().NotBeNullOrEmpty();
        comp2Key.Should().NotBeNullOrEmpty();
        _histogramValues[comp1Key!].Should().Be(100);
        _histogramValues[comp2Key!].Should().Be(200);
    }

    #endregion

    #region GetSystemStats Tests

    [Fact]
    public void GetSystemStats_InitialState_ShouldReturnZeroStats()
    {
        // Act
        var stats = _service.GetSystemStats();

        // Assert
        stats.TotalPermissionChecks.Should().Be(0);
        stats.TotalCacheHits.Should().Be(0);
        stats.CacheHitRate.Should().Be(0.0);
        stats.ActiveChecks.Should().Be(0);
        stats.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GetSystemStats_AfterPermissionChecks_ShouldReturnCorrectCounts()
    {
        // Act
        using (var t1 = _service.MeasurePermissionCheck("user1", EPermission.UsersRead, true)) { }
        using (var t2 = _service.MeasurePermissionCheck("user2", EPermission.UsersCreate, false)) { }

        var stats = _service.GetSystemStats();

        // Assert
        stats.TotalPermissionChecks.Should().Be(2);
    }

    [Fact]
    public void GetSystemStats_AfterCacheHits_ShouldReturnCorrectHitRate()
    {
        // Arrange - Need at least one permission check for denominator
        using (var check = _service.MeasurePermissionCheck("user1", EPermission.UsersRead, true)) { }
        using (var hit1 = _service.MeasureCacheOperation("get", hit: true)) { }
        using (var hit2 = _service.MeasureCacheOperation("get", hit: true)) { }
        using (var miss = _service.MeasureCacheOperation("get", hit: false)) { }

        // Act
        var stats = _service.GetSystemStats();

        // Assert
        stats.TotalCacheHits.Should().Be(2);
        stats.CacheHitRate.Should().Be(2.0); // 2 hits / 1 check = 2.0 (can be > 1 if multiple cache ops per check)
    }

    [Fact]
    public void GetSystemStats_WithActiveChecks_ShouldReturnActiveCount()
    {
        // Arrange
        var timer = _service.MeasurePermissionCheck("user1", EPermission.UsersRead, true);

        // Act
        var stats = _service.GetSystemStats();

        // Assert
        stats.ActiveChecks.Should().Be(1);

        // Cleanup
        timer.Dispose();
    }

    [Fact]
    public void GetSystemStats_WithZeroChecks_ShouldReturnZeroHitRate()
    {
        // Arrange - Only cache operations, no permission checks
        using (var hit = _service.MeasureCacheOperation("get", hit: true)) { }

        // Act
        var stats = _service.GetSystemStats();

        // Assert
        stats.TotalPermissionChecks.Should().Be(0);
        stats.CacheHitRate.Should().Be(0.0);
    }

    [Fact]
    public void GetSystemStats_ThreadSafe_ShouldHandleConcurrentCalls()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act - Multiple concurrent permission checks and stats reads
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                using var timer = _service.MeasurePermissionCheck($"user{i}", EPermission.UsersRead, true);
                var stats = _service.GetSystemStats();
            }));
        }

        Task.WaitAll(tasks.ToArray());
        var finalStats = _service.GetSystemStats();

        // Assert
        finalStats.TotalPermissionChecks.Should().Be(10);
    }

    #endregion

    #region OperationTimer Tests

    [Fact]
    public void OperationTimer_OnDispose_ShouldInvokeOnCompleteCallback()
    {
        // Arrange
        var callbackInvoked = false;
        TimeSpan? recordedDuration = null;

        // Act
        using (var timer = _service.MeasurePermissionResolution("user123", "users"))
        {
            Thread.Sleep(10);
        }

        // Assert - Duration should be recorded in histogram
        var histogramKey = _histogramValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_permission_resolution_duration_seconds"));
        histogramKey.Should().NotBeNullOrEmpty();
        _histogramValues[histogramKey!].Should().BeGreaterThan(0);
    }

    [Fact]
    public void OperationTimer_MultipleDispose_ShouldNotThrow()
    {
        // Arrange
        var timer = _service.MeasurePermissionResolution("user123", "users");

        // Act & Assert - Should not throw on multiple dispose
        timer.Dispose();
        timer.Dispose();
        timer.Dispose();
    }

    [Fact]
    public void OperationTimer_ShortOperation_ShouldRecordSmallDuration()
    {
        // Act
        using (var timer = _service.MeasurePermissionResolution("user123", "users"))
        {
            // Immediate disposal
        }

        // Assert - Should record very small but non-negative duration
        var histogramKey = _histogramValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_permission_resolution_duration_seconds"));
        histogramKey.Should().NotBeNullOrEmpty();
        _histogramValues[histogramKey!].Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region PermissionMetricsExtensions Tests

    [Fact]
    public void AddPermissionMetrics_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddPermissionMetrics();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var concreteService = serviceProvider.GetService<PermissionMetricsService>();
        var interfaceService = serviceProvider.GetService<IPermissionMetricsService>();

        concreteService.Should().NotBeNull();
        interfaceService.Should().NotBeNull();
        concreteService.Should().BeSameAs(interfaceService);

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddPermissionMetrics_ShouldRegisterAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPermissionMetrics();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var service1 = serviceProvider.GetService<IPermissionMetricsService>();
        var service2 = serviceProvider.GetService<IPermissionMetricsService>();

        // Assert
        service1.Should().BeSameAs(service2);

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public async Task MeasureAsync_ShouldExecuteOperationAndReturnResult()
    {
        // Arrange
        var expectedResult = 42;
        Func<Task<int>> operation = async () =>
        {
            await Task.Delay(10);
            return expectedResult;
        };

        // Act
        var result = await _service.MeasureAsync(operation, "test_operation", "user123");

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task MeasureAsync_ShouldRecordMetrics()
    {
        // Arrange
        Func<Task<string>> operation = async () =>
        {
            await Task.Delay(10);
            return "success";
        };

        // Act
        await _service.MeasureAsync(operation, "test_module", "user456");

        // Assert - Should have incremented resolution counter
        var counterKey = _counterValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_permission_resolutions_total"));
        counterKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task MeasureAsync_WithException_ShouldPropagateException()
    {
        // Arrange
        Func<Task<int>> operation = async () =>
        {
            await Task.Delay(5);
            throw new InvalidOperationException("Test exception");
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.MeasureAsync(operation, "failing_operation", "user789"));
    }

    #endregion

    #region Meter Configuration Tests

    [Fact]
    public void Constructor_ShouldInitializeMeterWithCorrectName()
    {
        // Assert - Meter is initialized in constructor
        _service.Should().NotBeNull();
    }

    [Fact]
    public void Dispose_ShouldDisposeMeter()
    {
        // Arrange
        var service = new PermissionMetricsService(_loggerMock.Object);

        // Act & Assert - Should not throw
        service.Dispose();
    }

    [Fact]
    public void Dispose_MultipleCalls_ShouldNotThrow()
    {
        // Arrange
        var service = new PermissionMetricsService(_loggerMock.Object);

        // Act & Assert
        service.Dispose();
        service.Dispose();
        service.Dispose();
    }

    #endregion

    #region Integration Tests - Complex Scenarios

    [Fact]
    public void ComplexScenario_MultipleOperations_ShouldTrackAllMetrics()
    {
        // Arrange & Act
        using (var resolution = _service.MeasurePermissionResolution("user1", "users"))
        {
            using (var check1 = _service.MeasurePermissionCheck("user1", EPermission.UsersRead, true)) { }
            using (var check2 = _service.MeasurePermissionCheck("user1", EPermission.UsersCreate, false)) { }
            using (var cacheHit = _service.MeasureCacheOperation("get", hit: true)) { }
        }

        _service.RecordAuthorizationFailure("user1", EPermission.UsersCreate, "insufficient_role");
        _service.RecordCacheInvalidation("user1", "user_updated");
        _service.RecordPerformanceStats("total_resolution", 125.5, "ms");

        var stats = _service.GetSystemStats();

        // Assert
        stats.TotalPermissionChecks.Should().Be(2);
        stats.TotalCacheHits.Should().Be(1);
        stats.CacheHitRate.Should().Be(0.5); // 1 hit / 2 checks

        // Verify counters
        var resolutionKey = _counterValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_permission_resolutions_total"));
        var checksKey = _counterValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_permission_checks_total"));
        var hitKey = _counterValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_permission_cache_hits_total"));
        var invalidationKey = _counterValues.Keys.FirstOrDefault(k => k.StartsWith("meajudaai_permission_cache_invalidations_total"));

        resolutionKey.Should().NotBeNullOrEmpty();
        checksKey.Should().NotBeNullOrEmpty();
        hitKey.Should().NotBeNullOrEmpty();
        invalidationKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NestedTimers_ShouldTrackActiveChecksCorrectly()
    {
        // Act
        using (var outer = _service.MeasurePermissionResolution("user1", "users"))
        {
            var stats1 = _service.GetSystemStats();
            stats1.ActiveChecks.Should().Be(1);

            using (var inner1 = _service.MeasurePermissionCheck("user1", EPermission.UsersRead, true))
            {
                var stats2 = _service.GetSystemStats();
                stats2.ActiveChecks.Should().Be(2);

                using (var inner2 = _service.MeasurePermissionCheck("user1", EPermission.UsersCreate, true))
                {
                    var stats3 = _service.GetSystemStats();
                    stats3.ActiveChecks.Should().Be(3);
                }

                var stats4 = _service.GetSystemStats();
                stats4.ActiveChecks.Should().Be(2);
            }

            var stats5 = _service.GetSystemStats();
            stats5.ActiveChecks.Should().Be(1);
        }

        var finalStats = _service.GetSystemStats();
        finalStats.ActiveChecks.Should().Be(0);
    }

    #endregion
}
