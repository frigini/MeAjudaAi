using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.ValueObjects;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Modules.Locations.Infrastructure.Services;
using MeAjudaAi.Shared.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Infrastructure.Services;

public sealed class CepLookupServiceTests
{
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<ILogger<CepLookupService>> _loggerMock;
    private readonly Mock<ILogger<ViaCepClient>> _viaLoggerMock;
    private readonly Mock<ILogger<BrasilApiCepClient>> _brasilLoggerMock;
    private readonly Mock<ILogger<OpenCepClient>> _openLoggerMock;

    public CepLookupServiceTests()
    {
        _cacheMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<CepLookupService>>();
        _viaLoggerMock = new Mock<ILogger<ViaCepClient>>();
        _brasilLoggerMock = new Mock<ILogger<BrasilApiCepClient>>();
        _openLoggerMock = new Mock<ILogger<OpenCepClient>>();
    }

    [Fact]
    public async Task LookupAsync_WhenCacheHit_ShouldReturnCachedAddress()
    {
        // Arrange
        var cep = Cep.Create("01310-100");
        var cachedAddress = Address.Create(
            cep: cep,
            street: "Avenida Paulista",
            neighborhood: "Bela Vista",
            city: "São Paulo",
            state: "SP",
            complement: null);

        _cacheMock.Setup(c => c.GetOrCreateAsync<Address?>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<Address?>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedAddress);

        var httpClient = new HttpClient();
        var viaClient = new ViaCepClient(httpClient, _viaLoggerMock.Object);
        var brasilClient = new BrasilApiCepClient(httpClient, _brasilLoggerMock.Object);
        var openClient = new OpenCepClient(httpClient, _openLoggerMock.Object);
        
        var service = new CepLookupService(
            viaClient,
            brasilClient,
            openClient,
            _cacheMock.Object,
            _loggerMock.Object);

        // Act
        var result = await service.LookupAsync(cep);

        // Assert
        result.Should().Be(cachedAddress);
    }

    [Fact]
    public async Task LookupAsync_ShouldUseCacheWithCorrectKey()
    {
        // Arrange
        var cep = Cep.Create("01310-100");
        var address = Address.Create(
            cep: cep,
            street: "Avenida Paulista",
            neighborhood: "Bela Vista",
            city: "São Paulo",
            state: "SP",
            complement: null);

        string? capturedKey = null;
        _cacheMock.Setup(c => c.GetOrCreateAsync<Address?>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<Address?>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Func<CancellationToken, ValueTask<Address?>>, TimeSpan?, HybridCacheEntryOptions?, IReadOnlyCollection<string>?, CancellationToken>(
                (key, _, _, _, _, _) => capturedKey = key)
            .ReturnsAsync(address);

        var httpClient = new HttpClient();
        var viaClient = new ViaCepClient(httpClient, _viaLoggerMock.Object);
        var brasilClient = new BrasilApiCepClient(httpClient, _brasilLoggerMock.Object);
        var openClient = new OpenCepClient(httpClient, _openLoggerMock.Object);
        
        var service = new CepLookupService(
            viaClient,
            brasilClient,
            openClient,
            _cacheMock.Object,
            _loggerMock.Object);

        // Act
        await service.LookupAsync(cep);

        // Assert
        capturedKey.Should().Be("cep:01310100");
    }

    [Fact]
    public async Task LookupAsync_ShouldUseCacheWith24HourExpiration()
    {
        // Arrange
        var cep = Cep.Create("01310-100");
        var address = Address.Create(
            cep: cep,
            street: "Avenida Paulista",
            neighborhood: "Bela Vista",
            city: "São Paulo",
            state: "SP",
            complement: null);

        TimeSpan? capturedExpiration = null;
        _cacheMock.Setup(c => c.GetOrCreateAsync<Address?>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<Address?>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Func<CancellationToken, ValueTask<Address?>>, TimeSpan?, HybridCacheEntryOptions?, IReadOnlyCollection<string>?, CancellationToken>(
                (_, _, expiration, _, _, _) => capturedExpiration = expiration)
            .ReturnsAsync(address);

        var httpClient = new HttpClient();
        var viaClient = new ViaCepClient(httpClient, _viaLoggerMock.Object);
        var brasilClient = new BrasilApiCepClient(httpClient, _brasilLoggerMock.Object);
        var openClient = new OpenCepClient(httpClient, _openLoggerMock.Object);
        
        var service = new CepLookupService(
            viaClient,
            brasilClient,
            openClient,
            _cacheMock.Object,
            _loggerMock.Object);

        // Act
        await service.LookupAsync(cep);

        // Assert
        capturedExpiration.Should().Be(TimeSpan.FromHours(24));
    }

    [Fact]
    public async Task LookupAsync_ShouldUseCacheWithCorrectTags()
    {
        // Arrange
        var cep = Cep.Create("01310-100");
        var address = Address.Create(
            cep: cep,
            street: "Avenida Paulista",
            neighborhood: "Bela Vista",
            city: "São Paulo",
            state: "SP",
            complement: null);

        IReadOnlyCollection<string>? capturedTags = null;
        _cacheMock.Setup(c => c.GetOrCreateAsync<Address?>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<Address?>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Func<CancellationToken, ValueTask<Address?>>, TimeSpan?, HybridCacheEntryOptions?, IReadOnlyCollection<string>?, CancellationToken>(
                (_, _, _, _, tags, _) => capturedTags = tags)
            .ReturnsAsync(address);

        var httpClient = new HttpClient();
        var viaClient = new ViaCepClient(httpClient, _viaLoggerMock.Object);
        var brasilClient = new BrasilApiCepClient(httpClient, _brasilLoggerMock.Object);
        var openClient = new OpenCepClient(httpClient, _openLoggerMock.Object);
        
        var service = new CepLookupService(
            viaClient,
            brasilClient,
            openClient,
            _cacheMock.Object,
            _loggerMock.Object);

        // Act
        await service.LookupAsync(cep);

        // Assert
        capturedTags.Should().NotBeNull();
        capturedTags.Should().Contain("cep");
        capturedTags.Should().Contain("cep:01310100");
    }
}
