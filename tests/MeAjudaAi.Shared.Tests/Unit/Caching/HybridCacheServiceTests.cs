using System.Diagnostics.Metrics;
using MeAjudaAi.Shared.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.Unit.Caching;

[Trait("Category", "Unit")]
public class HybridCacheServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly HybridCache _hybridCache;
    private readonly CacheMetrics _cacheMetrics;
    private readonly HybridCacheService _cacheService;

    public HybridCacheServiceTests()
    {
        // Cria HybridCache real com configuração in-memory para testes
        var services = new ServiceCollection();
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(2)
            };
        });
        services.AddMemoryCache();
        services.AddMetrics();
        services.AddLogging();
        
        // Add configuration with Cache:Enabled = true for tests
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cache:Enabled"] = "true"
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        _serviceProvider = services.BuildServiceProvider();
        _hybridCache = _serviceProvider.GetRequiredService<HybridCache>();

        var meterFactory = _serviceProvider.GetRequiredService<IMeterFactory>();
        _cacheMetrics = new CacheMetrics(meterFactory);

        _cacheService = new HybridCacheService(
            _hybridCache,
            Mock.Of<ILogger<HybridCacheService>>(),
            _cacheMetrics,
            configuration);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public async Task GetAsync_WithNonExistentKey_ShouldReturnDefault()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        var (result, isCached) = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ThenGetAsync_ShouldReturnCachedValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        var expiration = TimeSpan.FromMinutes(10);

        // Act
        await _cacheService.SetAsync(key, value, expiration);
        var (result, isCached) = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public async Task SetAsync_WithCustomOptions_ShouldStoreValue()
    {
        // Arrange
        var key = "custom-options-key";
        var value = "custom-value";
        var customOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromHours(1),
            LocalCacheExpiration = TimeSpan.FromMinutes(10)
        };

        // Act
        await _cacheService.SetAsync(key, value, null, customOptions);
        var (result, isCached) = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public async Task SetAsync_WithTags_ShouldStoreValueWithTags()
    {
        // Arrange
        var key = "tagged-key";
        var value = "tagged-value";
        var tags = new[] { "tag1", "tag2" };

        // Act
        await _cacheService.SetAsync(key, value, tags: tags);
        var (result, isCached) = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveValueFromCache()
    {
        // Arrange
        var key = "remove-test-key";
        var value = "remove-test-value";

        // Primeiro armazena o valor
        await _cacheService.SetAsync(key, value);
        var (beforeRemove, _) = await _cacheService.GetAsync<string>(key);
        beforeRemove.Should().Be(value);

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        var (afterRemove, isCached) = await _cacheService.GetAsync<string>(key);
        afterRemove.Should().BeNull();
    }

    [Fact]
    public async Task RemoveByPatternAsync_ShouldRemoveTaggedValues()
    {
        // Arrange
        var tag = "pattern-test";
        var key1 = "pattern-key-1";
        var key2 = "pattern-key-2";
        var value1 = "pattern-value-1";
        var value2 = "pattern-value-2";

        // Armazena valores com a mesma tag
        await _cacheService.SetAsync(key1, value1, tags: [tag]);
        await _cacheService.SetAsync(key2, value2, tags: [tag]);

        // Verifica se os valores est�o em cache
        var (beforeRemove1, _) = await _cacheService.GetAsync<string>(key1);
        var (beforeRemove2, __) = await _cacheService.GetAsync<string>(key2);
        beforeRemove1.Should().Be(value1);
        beforeRemove2.Should().Be(value2);

        // Act
        await _cacheService.RemoveByPatternAsync(tag);

        // Assert
        var (afterRemove1, isCached1) = await _cacheService.GetAsync<string>(key1);
        var (afterRemove2, isCached2) = await _cacheService.GetAsync<string>(key2);
        afterRemove1.Should().BeNull();
        afterRemove2.Should().BeNull();
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenCacheMiss_ShouldCallFactoryAndCacheResult()
    {
        // Arrange
        var key = "factory-test-key";
        var factoryValue = "factory-value";
        var factoryCalled = false;

        ValueTask<string> factory(CancellationToken ct)
        {
            factoryCalled = true;
            return ValueTask.FromResult(factoryValue);
        }

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, factory);

        // Assert
        result.Should().Be(factoryValue);
        factoryCalled.Should().BeTrue();

        // Verifica se o valor foi armazenado em cache
        var (cachedResult, isCached) = await _cacheService.GetAsync<string>(key);
        cachedResult.Should().Be(factoryValue);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenCacheHit_ShouldNotCallFactory()
    {
        // Arrange
        var key = "cache-hit-test-key";
        var cachedValue = "cached-value";
        var factoryValue = "factory-value";

        // Primeiro armazena um valor
        await _cacheService.SetAsync(key, cachedValue);

        var factoryCalled = false;
        ValueTask<string> factory(CancellationToken ct)
        {
            factoryCalled = true;
            return ValueTask.FromResult(factoryValue);
        }

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, factory);

        // Assert
        result.Should().Be(cachedValue);
        factoryCalled.Should().BeFalse(); // Factory n�o deve ser chamado em cache hit
    }

    [Fact]
    public async Task GetOrCreateAsync_WithOptions_ShouldUseProviededOptions()
    {
        // Arrange
        var key = "options-factory-key";
        var factoryValue = "options-factory-value";
        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(30)
        };
        var tags = new[] { "option-tag" };

        ValueTask<string> factory(CancellationToken ct) =>
            ValueTask.FromResult(factoryValue);

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, factory, null, options, tags);

        // Assert
        result.Should().Be(factoryValue);

        // Verifica se o valor foi armazenado em cache
        var (cachedResult, isCached) = await _cacheService.GetAsync<string>(key);
        cachedResult.Should().Be(factoryValue);
    }

    [Fact]
    public async Task GetAsync_WithComplexType_ShouldWork()
    {
        // Arrange
        var key = "complex-type-key";
        var complexValue = new TestModel { Id = 123, Name = "Test" };

        // Act
        await _cacheService.SetAsync(key, complexValue);
        var (result, isCached) = await _cacheService.GetAsync<TestModel>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(complexValue.Id);
        result.Name.Should().Be(complexValue.Name);
    }

    [Fact]
    public async Task SetAsync_WithNullValue_ShouldWork()
    {
        // Arrange
        var key = "null-value-key";
        string? nullValue = null;

        // Act & Assert
        await _cacheService.SetAsync(key, nullValue);
        var (result, isCached) = await _cacheService.GetAsync<string>(key);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrCreateAsync_WithAsyncFactory_ShouldWork()
    {
        // Arrange
        var key = "async-factory-key";
        var factoryValue = "async-factory-value";

        async ValueTask<string> asyncFactory(CancellationToken ct)
        {
            await Task.Delay(10, ct); // Simula trabalho ass�ncrono
            return factoryValue;
        }

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, asyncFactory);

        // Assert
        result.Should().Be(factoryValue);
    }

    [Fact]
    public async Task SetAsync_WithZeroExpiration_ShouldNotThrow()
    {
        // Arrange
        var key = "zero-expiration-key";
        var value = "zero-expiration-value";

        // Act & Assert
        await _cacheService.SetAsync(key, value, TimeSpan.Zero);
    }

    // Modelo de teste para tipos complexos
    private class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

