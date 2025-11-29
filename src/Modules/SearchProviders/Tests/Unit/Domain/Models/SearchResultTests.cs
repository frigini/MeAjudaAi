using Bogus;
using FluentAssertions;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Domain.Models;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Shared.Geolocation;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Domain.Models;

public class SearchResultTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void SearchResult_WithProviders_ShouldStoreData()
    {
        // Arrange
        var providers = CreateProviders(3);
        var distances = new List<double> { 1.5, 2.3, 3.7 };
        var totalCount = 10;

        // Act
        var result = new SearchResult
        {
            Providers = providers,
            DistancesInKm = distances,
            TotalCount = totalCount
        };

        // Assert
        result.Providers.Should().HaveCount(3);
        result.DistancesInKm.Should().HaveCount(3);
        result.TotalCount.Should().Be(totalCount);
    }

    [Fact]
    public void HasMore_WhenProvidersCountLessThanTotal_ShouldReturnTrue()
    {
        // Arrange
        var providers = CreateProviders(5);
        var distances = Enumerable.Range(0, 5).Select(i => (double)i).ToList();

        var result = new SearchResult
        {
            Providers = providers,
            DistancesInKm = distances,
            TotalCount = 10
        };

        // Act
        var hasMore = result.HasMore;

        // Assert
        hasMore.Should().BeTrue();
    }

    [Fact]
    public void HasMore_WhenProvidersCountEqualsTotal_ShouldReturnFalse()
    {
        // Arrange
        var providers = CreateProviders(5);
        var distances = Enumerable.Range(0, 5).Select(i => (double)i).ToList();

        var result = new SearchResult
        {
            Providers = providers,
            DistancesInKm = distances,
            TotalCount = 5
        };

        // Act
        var hasMore = result.HasMore;

        // Assert
        hasMore.Should().BeFalse();
    }

    [Fact]
    public void HasMore_WhenNoProviders_ShouldReturnFalse()
    {
        // Arrange
        var result = new SearchResult
        {
            Providers = new List<SearchableProvider>(),
            DistancesInKm = new List<double>(),
            TotalCount = 0
        };

        // Act
        var hasMore = result.HasMore;

        // Assert
        hasMore.Should().BeFalse();
    }

    [Fact]
    public void SearchResult_WithEmptyProviders_ShouldAllowEmptyLists()
    {
        // Act
        var result = new SearchResult
        {
            Providers = new List<SearchableProvider>(),
            DistancesInKm = new List<double>(),
            TotalCount = 0
        };

        // Assert
        result.Providers.Should().BeEmpty();
        result.DistancesInKm.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public void SearchResult_Distances_ShouldCorrespondToProviders()
    {
        // Arrange
        var providers = CreateProviders(3);
        var distances = new List<double> { 1.5, 2.3, 3.7 };

        // Act
        var result = new SearchResult
        {
            Providers = providers,
            DistancesInKm = distances,
            TotalCount = 3
        };

        // Assert
        result.Providers.Should().HaveCount(result.DistancesInKm.Count);
    }

    [Fact]
    public void SearchResult_RecordEquality_WithSameData_ShouldBeEqual()
    {
        // Arrange
        var providers = CreateProviders(2);
        var distances = new List<double> { 1.0, 2.0 };

        var result1 = new SearchResult
        {
            Providers = providers,
            DistancesInKm = distances,
            TotalCount = 5
        };

        var result2 = new SearchResult
        {
            Providers = providers,
            DistancesInKm = distances,
            TotalCount = 5
        };

        // Act & Assert
        result1.Should().Be(result2);
    }

    [Fact]
    public void SearchResult_ShouldBeReadOnly()
    {
        // Arrange
        var providers = CreateProviders(2);
        var distances = new List<double> { 1.0, 2.0 };

        var result = new SearchResult
        {
            Providers = providers,
            DistancesInKm = distances,
            TotalCount = 5
        };

        // Assert
        result.Providers.Should().BeAssignableTo<IReadOnlyList<SearchableProvider>>();
        result.DistancesInKm.Should().BeAssignableTo<IReadOnlyList<double>>();
    }

    [Fact]
    public void HasMore_WithPagination_ShouldIndicateMorePages()
    {
        // Arrange - simulating page 1 of 3 (10 items per page, 25 total)
        var providers = CreateProviders(10);
        var distances = Enumerable.Range(0, 10).Select(i => (double)i).ToList();

        var result = new SearchResult
        {
            Providers = providers,
            DistancesInKm = distances,
            TotalCount = 25
        };

        // Act
        var hasMore = result.HasMore;

        // Assert
        hasMore.Should().BeTrue();
        result.Providers.Count.Should().BeLessThan(result.TotalCount);
    }

    private IReadOnlyList<SearchableProvider> CreateProviders(int count)
    {
        var providers = new List<SearchableProvider>();
        
        for (int i = 0; i < count; i++)
        {
            var providerId = Guid.NewGuid();
            var location = new GeoPoint(-23.5505 + i * 0.1, -46.6333 + i * 0.1);
            
            var provider = SearchableProvider.Create(
                providerId,
                _faker.Person.FullName,
                location,
                _faker.Random.Enum<ESubscriptionTier>(),
                _faker.Lorem.Sentence(),
                _faker.Address.City(),
                _faker.Address.StateAbbr()
            );
            
            providers.Add(provider);
        }

        return providers;
    }
}
