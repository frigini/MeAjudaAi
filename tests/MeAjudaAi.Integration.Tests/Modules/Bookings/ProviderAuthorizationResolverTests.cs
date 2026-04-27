using System.Security.Claims;
using System.Diagnostics.Metrics;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Bookings.Application.Common;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace MeAjudaAi.Integration.Tests.Modules.Bookings;

public class MockCacheMetrics : ICacheMetrics
{
    public void RecordCacheHit(string key, string operation = "get") { }
    public void RecordCacheMiss(string key, string operation = "get") { }
    public void RecordOperationDuration(double durationSeconds, string operation, string result) { }
    public void RecordOperation(string key, string operation, bool isHit, double durationSeconds) { }
}

public class ProviderAuthorizationResolverTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Bookings | TestModule.Providers;

    private readonly Mock<IProvidersModuleApi> _providersApiMock = new();

    // We need to use a custom factory to enable cache
    private IServiceProvider GetServiceProviderWithCache()
    {
        var services = new ServiceCollection();
        
        // Setup configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cache:Enabled"] = "true"
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        // Setup HybridCache (using memory for L2 in tests)
        services.AddDistributedMemoryCache();
        #pragma warning disable EXTEXP0018
        services.AddHybridCache(options => 
        {
            options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromSeconds(1),
                Expiration = TimeSpan.FromSeconds(10)
            };
        });
        #pragma warning restore EXTEXP0018

        services.AddSingleton<IMeterFactory>(new Mock<IMeterFactory>().Object);
        services.AddSingleton<ICacheMetrics, MockCacheMetrics>();
        services.AddSingleton<ICacheService>(sp => ActivatorUtilities.CreateInstance<HybridCacheService>(sp));
        services.AddSingleton(_providersApiMock.Object);
        services.AddSingleton<ProviderAuthorizationResolver>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task ResolveAsync_Should_RehydrateFromL2_WhenL1IsExpired()
    {
        // Arrange
        var serviceProvider = GetServiceProviderWithCache();
        var sut = serviceProvider.GetRequiredService<ProviderAuthorizationResolver>();
        var userId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(AuthConstants.Claims.Subject, userId.ToString())
        }, "Test"));

        _providersApiMock.Setup(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(new ModuleProviderDto(
                providerId, 
                "Test Provider", 
                "test-provider", 
                "test@test.com", 
                "123456789", 
                "Professional", 
                "Verified", 
                DateTime.UtcNow, 
                DateTime.UtcNow, 
                true)));

        // Act 1: Primeira chamada (Cache Miss -> Popula L1 e L2)
        var result1 = await sut.ResolveAsync(user);
        
        // Assert 1
        result1.ProviderId.Should().Be(providerId);
        _providersApiMock.Verify(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);

        // Aguardar expiração do L1 (configurado para 1s no factory do teste)
        await Task.Delay(1500);

        // Act 2: Segunda chamada (L1 Miss -> Re-hidrata do L2)
        var result2 = await sut.ResolveAsync(user);

        // Assert 2
        result2.ProviderId.Should().Be(providerId);
        // Não deve ter chamado o API novamente, pois deve ter vindo do L2
        _providersApiMock.Verify(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResolveAsync_Should_RehydrateNotLinkedFromL2_WhenL1IsExpired()
    {
        // Arrange
        var serviceProvider = GetServiceProviderWithCache();
        var sut = serviceProvider.GetRequiredService<ProviderAuthorizationResolver>();
        var userId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(AuthConstants.Claims.Subject, userId.ToString())
        }, "Test"));

        _providersApiMock.Setup(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(null));

        // Act 1: Primeira chamada
        var result1 = await sut.ResolveAsync(user);
        
        // Assert 1
        result1.FailureKind.Should().Be(AuthorizationFailureKind.NotLinked);
        _providersApiMock.Verify(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);

        // Aguardar expiração do L1
        await Task.Delay(1500);

        // Act 2: Segunda chamada
        var result2 = await sut.ResolveAsync(user);

        // Assert 2
        result2.FailureKind.Should().Be(AuthorizationFailureKind.NotLinked);
        _providersApiMock.Verify(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
