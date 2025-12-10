using FluentAssertions;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Modules.Locations.Infrastructure.Services;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Infrastructure.Services;

public sealed class GeocodingServiceTests
{
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<ILogger<GeocodingService>> _loggerMock;
    private readonly Mock<ILogger<NominatimClient>> _nominatimLoggerMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;

    public GeocodingServiceTests()
    {
        _cacheMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<GeocodingService>>();
        _nominatimLoggerMock = new Mock<ILogger<NominatimClient>>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(d => d.CurrentDate()).Returns(DateTime.UtcNow);
    }

    [Fact]
    public async Task GetCoordinatesAsync_WhenAddressIsNull_ShouldReturnNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var nominatimClient = new NominatimClient(httpClient, _nominatimLoggerMock.Object, _dateTimeProviderMock.Object);
        var service = new GeocodingService(nominatimClient, _cacheMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetCoordinatesAsync(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCoordinatesAsync_WhenAddressIsWhitespace_ShouldReturnNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var nominatimClient = new NominatimClient(httpClient, _nominatimLoggerMock.Object, _dateTimeProviderMock.Object);
        var service = new GeocodingService(nominatimClient, _cacheMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetCoordinatesAsync("   ");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCoordinatesAsync_WhenCacheHit_ShouldReturnCachedCoordinates()
    {
        // Arrange
        var address = "Avenida Paulista, 1578, São Paulo";
        var cachedPoint = new GeoPoint(-23.5615, -46.6560);

        _cacheMock.Setup(c => c.GetOrCreateAsync<GeoPoint?>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<GeoPoint?>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedPoint);

        var httpClient = new HttpClient();
        var nominatimClient = new NominatimClient(httpClient, _nominatimLoggerMock.Object, _dateTimeProviderMock.Object);
        var service = new GeocodingService(nominatimClient, _cacheMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetCoordinatesAsync(address);

        // Assert
        result.Should().Be(cachedPoint);
    }

    [Fact]
    public async Task GetCoordinatesAsync_ShouldNormalizeCacheKey()
    {
        // Arrange
        var address = "  Avenida Paulista  ";
        var geoPoint = new GeoPoint(-23.5615, -46.6560);

        string? capturedKey = null;
        _cacheMock.Setup(c => c.GetOrCreateAsync<GeoPoint?>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<GeoPoint?>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Func<CancellationToken, ValueTask<GeoPoint?>>, TimeSpan?, HybridCacheEntryOptions?, IReadOnlyCollection<string>?, CancellationToken>(
                (key, _, _, _, _, _) => capturedKey = key)
            .ReturnsAsync(geoPoint);

        var httpClient = new HttpClient();
        var nominatimClient = new NominatimClient(httpClient, _nominatimLoggerMock.Object, _dateTimeProviderMock.Object);
        var service = new GeocodingService(nominatimClient, _cacheMock.Object, _loggerMock.Object);

        // Act
        await service.GetCoordinatesAsync(address);

        // Assert
        capturedKey.Should().Be("geocoding:avenida paulista");
    }

    [Fact]
    public async Task GetCoordinatesAsync_ShouldUseSevenDayExpiration()
    {
        // Arrange
        var address = "Avenida Paulista, São Paulo";
        var geoPoint = new GeoPoint(-23.5615, -46.6560);

        TimeSpan? capturedExpiration = null;
        _cacheMock.Setup(c => c.GetOrCreateAsync<GeoPoint?>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<GeoPoint?>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Func<CancellationToken, ValueTask<GeoPoint?>>, TimeSpan?, HybridCacheEntryOptions?, IReadOnlyCollection<string>?, CancellationToken>(
                (_, _, expiration, _, _, _) => capturedExpiration = expiration)
            .ReturnsAsync(geoPoint);

        var httpClient = new HttpClient();
        var nominatimClient = new NominatimClient(httpClient, _nominatimLoggerMock.Object, _dateTimeProviderMock.Object);
        var service = new GeocodingService(nominatimClient, _cacheMock.Object, _loggerMock.Object);

        // Act
        await service.GetCoordinatesAsync(address);

        // Assert
        capturedExpiration.Should().Be(TimeSpan.FromDays(7));
    }

    [Fact]
    public async Task GetCoordinatesAsync_ShouldUseGeocodingTag()
    {
        // Arrange
        var address = "Avenida Paulista, São Paulo";
        var geoPoint = new GeoPoint(-23.5615, -46.6560);

        IReadOnlyCollection<string>? capturedTags = null;
        _cacheMock.Setup(c => c.GetOrCreateAsync<GeoPoint?>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<GeoPoint?>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Func<CancellationToken, ValueTask<GeoPoint?>>, TimeSpan?, HybridCacheEntryOptions?, IReadOnlyCollection<string>?, CancellationToken>(
                (_, _, _, _, tags, _) => capturedTags = tags)
            .ReturnsAsync(geoPoint);

        var httpClient = new HttpClient();
        var nominatimClient = new NominatimClient(httpClient, _nominatimLoggerMock.Object, _dateTimeProviderMock.Object);
        var service = new GeocodingService(nominatimClient, _cacheMock.Object, _loggerMock.Object);

        // Act
        await service.GetCoordinatesAsync(address);

        // Assert
        capturedTags.Should().NotBeNull();
        capturedTags.Should().Contain("geocoding");
    }
}
