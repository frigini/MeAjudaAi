using FluentAssertions;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Shared.Geolocation;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Domain.Entities;

/// <summary>
/// Testes unitários para SearchableProvider entity
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "SearchProviders")]
[Trait("Component", "Domain")]
public class SearchableProviderTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateSearchableProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var name = "Test Provider";
        var location = new GeoPoint(-23.5505, -46.6333); // São Paulo
        var subscriptionTier = ESubscriptionTier.Gold;

        // Act
        var provider = SearchableProvider.Create(
            providerId,
            name,
            location,
            subscriptionTier,
            "Test description",
            "São Paulo",
            "SP");

        // Assert
        provider.Should().NotBeNull();
        provider.ProviderId.Should().Be(providerId);
        provider.Name.Should().Be(name);
        provider.Location.Should().Be(location);
        provider.SubscriptionTier.Should().Be(subscriptionTier);
        provider.Description.Should().Be("Test description");
        provider.City.Should().Be("São Paulo");
        provider.State.Should().Be("SP");
        provider.IsActive.Should().BeTrue();
        provider.AverageRating.Should().Be(0);
        provider.TotalReviews.Should().Be(0);
        provider.ServiceIds.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var location = new GeoPoint(-23.5505, -46.6333);

        // Act
        var act = () => SearchableProvider.Create(providerId, "", location);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Provider name cannot be empty*");
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrowArgumentException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var location = new GeoPoint(-23.5505, -46.6333);

        // Act
        var act = () => SearchableProvider.Create(providerId, "   ", location);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateBasicInfo_WithValidData_ShouldUpdateFields()
    {
        // Arrange
        var provider = CreateValidProvider();
        var newName = "Updated Provider";
        var newDescription = "Updated description";
        var newCity = "Rio de Janeiro";
        var newState = "RJ";

        // Act
        provider.UpdateBasicInfo(newName, newDescription, newCity, newState);

        // Assert
        provider.Name.Should().Be(newName);
        provider.Description.Should().Be(newDescription);
        provider.City.Should().Be(newCity);
        provider.State.Should().Be(newState);
        provider.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateBasicInfo_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        var act = () => provider.UpdateBasicInfo("", "desc", "city", "ST");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateLocation_WithValidLocation_ShouldUpdateLocation()
    {
        // Arrange
        var provider = CreateValidProvider();
        var newLocation = new GeoPoint(-22.9068, -43.1729); // Rio de Janeiro

        // Act
        provider.UpdateLocation(newLocation);

        // Assert
        provider.Location.Should().Be(newLocation);
        provider.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateLocation_WithNullLocation_ShouldThrowArgumentNullException()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        var act = () => provider.UpdateLocation(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdateRating_WithValidRating_ShouldUpdateRating()
    {
        // Arrange
        var provider = CreateValidProvider();
        var averageRating = 4.5m;
        var totalReviews = 10;

        // Act
        provider.UpdateRating(averageRating, totalReviews);

        // Assert
        provider.AverageRating.Should().Be(averageRating);
        provider.TotalReviews.Should().Be(totalReviews);
        provider.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(5.1)]
    [InlineData(10)]
    public void UpdateRating_WithInvalidRating_ShouldThrowArgumentOutOfRangeException(decimal invalidRating)
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        var act = () => provider.UpdateRating(invalidRating, 10);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Rating must be between 0 and 5*");
    }

    [Fact]
    public void UpdateRating_WithNegativeTotalReviews_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        var act = () => provider.UpdateRating(4.5m, -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Total reviews cannot be negative*");
    }

    [Theory]
    [InlineData(ESubscriptionTier.Free)]
    [InlineData(ESubscriptionTier.Standard)]
    [InlineData(ESubscriptionTier.Gold)]
    [InlineData(ESubscriptionTier.Platinum)]
    public void UpdateSubscriptionTier_WithValidTier_ShouldUpdateTier(ESubscriptionTier tier)
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        provider.UpdateSubscriptionTier(tier);

        // Assert
        provider.SubscriptionTier.Should().Be(tier);
        provider.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateServices_WithServiceIds_ShouldUpdateServices()
    {
        // Arrange
        var provider = CreateValidProvider();
        var serviceIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act
        provider.UpdateServices(serviceIds);

        // Assert
        provider.ServiceIds.Should().BeEquivalentTo(serviceIds);
        provider.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateServices_WithNull_ShouldSetEmptyArray()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        provider.UpdateServices(null!);

        // Assert
        provider.ServiceIds.Should().BeEmpty();
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateProvider()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.Deactivate();

        // Act
        provider.Activate();

        // Assert
        provider.IsActive.Should().BeTrue();
        provider.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldNotChangeState()
    {
        // Arrange
        var provider = CreateValidProvider();
        var originalUpdatedAt = provider.UpdatedAt;

        // Act
        provider.Activate();

        // Assert
        provider.IsActive.Should().BeTrue();
        provider.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateProvider()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        provider.Deactivate();

        // Assert
        provider.IsActive.Should().BeFalse();
        provider.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void CalculateDistanceToInKm_ShouldReturnCorrectDistance()
    {
        // Arrange
        var provider = CreateValidProvider(); // São Paulo
        var targetLocation = new GeoPoint(-22.9068, -43.1729); // Rio de Janeiro

        // Act
        var distance = provider.CalculateDistanceToInKm(targetLocation);

        // Assert
        // Distância entre São Paulo e Rio de Janeiro é aproximadamente 357 km
        distance.Should().BeApproximately(357, 10); // tolerância de ±10km
    }

    [Fact]
    public void CalculateDistanceToInKm_SameLocation_ShouldReturnZero()
    {
        // Arrange
        var location = new GeoPoint(-23.5505, -46.6333);
        var provider = SearchableProvider.Create(Guid.NewGuid(), "Test", location);

        // Act
        var distance = provider.CalculateDistanceToInKm(location);

        // Assert
        distance.Should().BeApproximately(0, 0.1); // Muito próximo de zero
    }

    private static SearchableProvider CreateValidProvider()
    {
        return SearchableProvider.Create(
            Guid.NewGuid(),
            "Test Provider",
            new GeoPoint(-23.5505, -46.6333), // São Paulo
            ESubscriptionTier.Free,
            "Test description",
            "São Paulo",
            "SP");
    }
}
