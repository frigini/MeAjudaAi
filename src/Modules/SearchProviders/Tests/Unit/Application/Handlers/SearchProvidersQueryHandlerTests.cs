using FluentAssertions;
using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using MeAjudaAi.Modules.SearchProviders.Application.Handlers;
using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Domain.Models;
using MeAjudaAi.Modules.SearchProviders.Domain.Repositories;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Application.Handlers;

/// <summary>
/// Testes unitários para SearchProvidersQueryHandler
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "SearchProviders")]
[Trait("Component", "QueryHandler")]
public class SearchProvidersQueryHandlerTests
{
    private readonly Mock<ISearchableProviderRepository> _repositoryMock;
    private readonly Mock<ILogger<SearchProvidersQueryHandler>> _loggerMock;
    private readonly SearchProvidersQueryHandler _sut;

    public SearchProvidersQueryHandlerTests()
    {
        _repositoryMock = new Mock<ISearchableProviderRepository>();
        _loggerMock = new Mock<ILogger<SearchProvidersQueryHandler>>();
        _sut = new SearchProvidersQueryHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidQuery_ShouldReturnSuccessWithResults()
    {
        // Arrange
        var query = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: 10,
            Page: 1,
            PageSize: 20);

        var providers = CreateTestProviders(3);
        var searchPoint = new GeoPoint(query.Latitude, query.Longitude);
        var distances = providers.Select(p => p.CalculateDistanceToInKm(searchPoint)).ToList();
        _repositoryMock
            .Setup(x => x.SearchAsync(
                It.IsAny<GeoPoint>(),
                query.RadiusInKm,
                query.Term,
                query.ServiceIds,
                query.MinRating,
                query.SubscriptionTiers,
                0,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult(
                Providers: providers,
                DistancesInKm: distances,
                TotalCount: 3));

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(3);
        result.Value.TotalItems.Should().Be(3);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
        result.Value.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCoordinates_ShouldReturnFailure()
    {
        // Arrange - latitude out of range
        var query = new SearchProvidersQuery(
            Latitude: 91,
            Longitude: -46.6333,
            RadiusInKm: 10);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithServiceIdsFilter_ShouldFilterByServices()
    {
        // Arrange
        var serviceId1 = Guid.NewGuid();
        var serviceId2 = Guid.NewGuid();
        var query = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: 10,
            ServiceIds: new[] { serviceId1, serviceId2 });

        var providers = CreateTestProviders(2);
        var searchPoint = new GeoPoint(query.Latitude, query.Longitude);
        var distances = providers.Select(p => p.CalculateDistanceToInKm(searchPoint)).ToList();
        _repositoryMock
            .Setup(x => x.SearchAsync(
                It.IsAny<GeoPoint>(),
                It.IsAny<double>(),
                It.IsAny<string?>(),
                It.Is<Guid[]?>(ids => ids != null && ids.Contains(serviceId1) && ids.Contains(serviceId2)),
                It.IsAny<decimal?>(),
                It.IsAny<ESubscriptionTier[]?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult(
                Providers: providers,
                DistancesInKm: distances,
                TotalCount: 2));

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(x => x.SearchAsync(
            It.IsAny<GeoPoint>(),
            It.IsAny<double>(),
            It.IsAny<string?>(),
            It.Is<Guid[]?>(ids => ids != null && ids.Contains(serviceId1) && ids.Contains(serviceId2)),
            It.IsAny<decimal?>(),
            It.IsAny<ESubscriptionTier[]?>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithMinRatingFilter_ShouldFilterByRating()
    {
        // Arrange
        var query = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: 10,
            MinRating: 4.0m);

        var providers = CreateTestProviders(1);
        var searchPoint = new GeoPoint(query.Latitude, query.Longitude);
        var distances = providers.Select(p => p.CalculateDistanceToInKm(searchPoint)).ToList();
        _repositoryMock
            .Setup(x => x.SearchAsync(
                It.IsAny<GeoPoint>(),
                It.IsAny<double>(),
                It.IsAny<string?>(),
                It.IsAny<Guid[]?>(),
                4.0m,
                It.IsAny<ESubscriptionTier[]?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult(
                Providers: providers,
                DistancesInKm: distances,
                TotalCount: 1));

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(x => x.SearchAsync(
            It.IsAny<GeoPoint>(),
            It.IsAny<double>(),
            It.IsAny<string?>(),
            It.IsAny<Guid[]?>(),
            4.0m,
            It.IsAny<ESubscriptionTier[]?>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithSubscriptionTiersFilter_ShouldFilterByTiers()
    {
        // Arrange
        var query = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: 10,
            SubscriptionTiers: new[] { ESubscriptionTier.Gold, ESubscriptionTier.Platinum });

        var providers = CreateTestProviders(2);
        var searchPoint = new GeoPoint(query.Latitude, query.Longitude);
        var distances = providers.Select(p => p.CalculateDistanceToInKm(searchPoint)).ToList();
        _repositoryMock
            .Setup(x => x.SearchAsync(
                It.IsAny<GeoPoint>(),
                It.IsAny<double>(),
                It.IsAny<string?>(),
                It.IsAny<Guid[]?>(),
                It.IsAny<decimal?>(),
                It.Is<ESubscriptionTier[]?>(t => t != null && t.Contains(ESubscriptionTier.Gold)),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult(
                Providers: providers,
                DistancesInKm: distances,
                TotalCount: 2));

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithPagination_ShouldCalculateCorrectSkipAndTake()
    {
        // Arrange - Page 2, 10 items per page
        var query = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: 10,
            Page: 2,
            PageSize: 10);

        var providers = CreateTestProviders(10);
        var searchPoint = new GeoPoint(query.Latitude, query.Longitude);
        var distances = providers.Select(p => p.CalculateDistanceToInKm(searchPoint)).ToList();
        _repositoryMock
            .Setup(x => x.SearchAsync(
                It.IsAny<GeoPoint>(),
                It.IsAny<double>(),
                It.IsAny<string?>(),
                It.IsAny<Guid[]?>(),
                It.IsAny<decimal?>(),
                It.IsAny<ESubscriptionTier[]?>(),
                10, // skip = (2-1) * 10 = 10
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult(
                Providers: providers,
                DistancesInKm: distances,
                TotalCount: 25)); // Total 25 items

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.PageNumber.Should().Be(2);
        result.Value.TotalPages.Should().Be(3); // 25 / 10 = 3
        result.Value.HasPreviousPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithNoResults_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: 1);

        _repositoryMock
            .Setup(x => x.SearchAsync(
                It.IsAny<GeoPoint>(),
                It.IsAny<double>(),
                It.IsAny<string?>(),
                It.IsAny<Guid[]?>(),
                It.IsAny<decimal?>(),
                It.IsAny<ESubscriptionTier[]?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult(
                Providers: new List<SearchableProvider>(),
                DistancesInKm: new List<double>(),
                TotalCount: 0));

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_ShouldMapDistanceCorrectly()
    {
        // Arrange
        var searchLocation = new GeoPoint(-23.5505, -46.6333); // São Paulo
        var query = new SearchProvidersQuery(
            Latitude: searchLocation.Latitude,
            Longitude: searchLocation.Longitude,
            RadiusInKm: 500);

        var provider = SearchableProvider.Create(
            Guid.NewGuid(),
            "Test Provider",
            new GeoPoint(-22.9068, -43.1729), // Rio de Janeiro
            ESubscriptionTier.Free);

        var distance = provider.CalculateDistanceToInKm(searchLocation);
        _repositoryMock
            .Setup(x => x.SearchAsync(
                It.IsAny<GeoPoint>(),
                It.IsAny<double>(),
                It.IsAny<string?>(),
                It.IsAny<Guid[]?>(),
                It.IsAny<decimal?>(),
                It.IsAny<ESubscriptionTier[]?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult(
                Providers: new List<SearchableProvider> { provider },
                DistancesInKm: new List<double> { distance },
                TotalCount: 1));

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value!.Items.First();
        dto.DistanceInKm.Should().NotBeNull();
        dto.DistanceInKm!.Value.Should().BeApproximately(357, 10); // Distance SP-RJ
    }

    private static List<SearchableProvider> CreateTestProviders(int count)
    {
        var providers = new List<SearchableProvider>();
        for (int i = 0; i < count; i++)
        {
            providers.Add(SearchableProvider.Create(
                Guid.NewGuid(),
                $"Provider {i + 1}",
                new GeoPoint(-23.5505 + i * 0.01, -46.6333 + i * 0.01),
                (ESubscriptionTier)(i % 4),
                $"Description {i + 1}",
                "São Paulo",
                "SP"));
        }
        return providers;
    }
}
