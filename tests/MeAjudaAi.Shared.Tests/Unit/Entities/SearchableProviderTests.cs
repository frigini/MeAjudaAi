using FluentAssertions;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Shared.Geolocation;

namespace MeAjudaAi.Shared.Tests.Unit.Entities;

/// <summary>
/// Testes unitários completos para SearchableProvider entity
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "SearchProviders")]
[Trait("Component", "Domain")]
public class SearchableProviderTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidMinimalData_ShouldCreateProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var name = "Test Provider";
        var location = new GeoPoint(-23.5505, -46.6333); // São Paulo

        // Act
        var provider = SearchableProvider.Create(providerId, name, location);

        // Assert
        provider.Should().NotBeNull();
        provider.ProviderId.Should().Be(providerId);
        provider.Name.Should().Be(name);
        provider.Location.Should().Be(location);
        provider.SubscriptionTier.Should().Be(ESubscriptionTier.Free);
        provider.IsActive.Should().BeTrue();
        provider.AverageRating.Should().Be(0);
        provider.TotalReviews.Should().Be(0);
        provider.ServiceIds.Should().BeEmpty();
        provider.Description.Should().BeNull();
        provider.City.Should().BeNull();
        provider.State.Should().BeNull();
    }

    [Fact]
    public void Create_WithAllData_ShouldCreateProviderWithAllFields()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var name = "Full Provider";
        var location = new GeoPoint(-22.9068, -43.1729); // Rio
        var tier = ESubscriptionTier.Gold;
        var description = "Full service provider";
        var city = "Rio de Janeiro";
        var state = "RJ";

        // Act
        var provider = SearchableProvider.Create(
            providerId, name, location, tier, description, city, state);

        // Assert
        provider.ProviderId.Should().Be(providerId);
        provider.Name.Should().Be(name);
        provider.Location.Should().Be(location);
        provider.SubscriptionTier.Should().Be(tier);
        provider.Description.Should().Be(description);
        provider.City.Should().Be(city);
        provider.State.Should().Be(state);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var location = new GeoPoint(-23.5505, -46.6333);

        // Act
        var act = () => SearchableProvider.Create(providerId, invalidName, location);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Provider name cannot be empty*");
    }

    [Fact]
    public void Create_WithNullLocation_ShouldThrowArgumentNullException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var name = "Test Provider";

        // Act
        var act = () => SearchableProvider.Create(providerId, name, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithWhitespaceInFields_ShouldTrimValues()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var location = new GeoPoint(-23.5505, -46.6333);

        // Act
        var provider = SearchableProvider.Create(
            providerId,
            "  Provider Name  ",
            location,
            ESubscriptionTier.Free,
            "  Description  ",
            "  São Paulo  ",
            "  SP  ");

        // Assert
        provider.Name.Should().Be("Provider Name");
        provider.Description.Should().Be("Description");
        provider.City.Should().Be("São Paulo");
        provider.State.Should().Be("SP");
    }

    [Theory]
    [InlineData(ESubscriptionTier.Free)]
    [InlineData(ESubscriptionTier.Standard)]
    [InlineData(ESubscriptionTier.Gold)]
    [InlineData(ESubscriptionTier.Platinum)]
    public void Create_WithDifferentSubscriptionTiers_ShouldSetCorrectTier(ESubscriptionTier tier)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var location = new GeoPoint(-23.5505, -46.6333);

        // Act
        var provider = SearchableProvider.Create(
            providerId, "Provider", location, tier);

        // Assert
        provider.SubscriptionTier.Should().Be(tier);
    }

    #endregion

    #region UpdateBasicInfo Tests

    [Fact]
    public void UpdateBasicInfo_WithValidData_ShouldUpdateAllFields()
    {
        // Arrange
        var provider = CreateValidProvider();
        var newName = "Updated Provider";
        var newDescription = "New description";
        var newCity = "Brasília";
        var newState = "DF";

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
    public void UpdateBasicInfo_WithNullOptionalFields_ShouldSetToNull()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        provider.UpdateBasicInfo("New Name", null, null, null);

        // Assert
        provider.Name.Should().Be("New Name");
        provider.Description.Should().BeNull();
        provider.City.Should().BeNull();
        provider.State.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateBasicInfo_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        var act = () => provider.UpdateBasicInfo(invalidName, "Desc", "City", "ST");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Provider name cannot be empty*");
    }

    [Fact]
    public void UpdateBasicInfo_WithWhitespace_ShouldTrimValues()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        provider.UpdateBasicInfo(
            "  New Name  ",
            "  New Description  ",
            "  New City  ",
            "  NC  ");

        // Assert
        provider.Name.Should().Be("New Name");
        provider.Description.Should().Be("New Description");
        provider.City.Should().Be("New City");
        provider.State.Should().Be("NC");
    }

    #endregion

    #region UpdateLocation Tests

    [Fact]
    public void UpdateLocation_WithValidLocation_ShouldUpdateLocation()
    {
        // Arrange
        var provider = CreateValidProvider();
        var newLocation = new GeoPoint(-15.7942, -47.8822); // Brasília

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

    [Theory]
    [InlineData(-90, -180)]  // Min valid coordinates
    [InlineData(90, 180)]    // Max valid coordinates
    [InlineData(0, 0)]       // Null Island
    [InlineData(-23.5505, -46.6333)] // São Paulo
    public void UpdateLocation_WithValidCoordinates_ShouldAcceptLocation(double lat, double lng)
    {
        // Arrange
        var provider = CreateValidProvider();
        var location = new GeoPoint(lat, lng);

        // Act
        provider.UpdateLocation(location);

        // Assert
        provider.Location.Latitude.Should().Be(lat);
        provider.Location.Longitude.Should().Be(lng);
    }

    #endregion

    #region UpdateRating Tests

    [Theory]
    [InlineData(0, 0)]
    [InlineData(2.5, 10)]
    [InlineData(4.8, 150)]
    [InlineData(5.0, 1000)]
    public void UpdateRating_WithValidValues_ShouldUpdateRating(decimal rating, int totalReviews)
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        provider.UpdateRating(rating, totalReviews);

        // Assert
        provider.AverageRating.Should().Be(rating);
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

    [Fact]
    public void UpdateRating_FromZeroToPositive_ShouldUpdateCorrectly()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.AverageRating.Should().Be(0);
        provider.TotalReviews.Should().Be(0);

        // Act
        provider.UpdateRating(4.5m, 50);

        // Assert
        provider.AverageRating.Should().Be(4.5m);
        provider.TotalReviews.Should().Be(50);
    }

    #endregion

    #region UpdateSubscriptionTier Tests

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
    public void UpdateSubscriptionTier_FromFreeToGold_ShouldUpgrade()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.SubscriptionTier.Should().Be(ESubscriptionTier.Free);

        // Act
        provider.UpdateSubscriptionTier(ESubscriptionTier.Gold);

        // Assert
        provider.SubscriptionTier.Should().Be(ESubscriptionTier.Gold);
    }

    [Fact]
    public void UpdateSubscriptionTier_FromGoldToFree_ShouldDowngrade()
    {
        // Arrange
        var provider = SearchableProvider.Create(
            Guid.NewGuid(),
            "Provider",
            new GeoPoint(-23.5505, -46.6333),
            ESubscriptionTier.Gold);

        // Act
        provider.UpdateSubscriptionTier(ESubscriptionTier.Free);

        // Assert
        provider.SubscriptionTier.Should().Be(ESubscriptionTier.Free);
    }

    #endregion

    #region UpdateServices Tests

    [Fact]
    public void UpdateServices_WithServiceIds_ShouldSetServices()
    {
        // Arrange
        var provider = CreateValidProvider();
        var serviceIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act
        provider.UpdateServices(serviceIds);

        // Assert
        provider.ServiceIds.Should().HaveCount(3);
        provider.ServiceIds.Should().BeEquivalentTo(serviceIds);
        provider.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateServices_WithEmptyArray_ShouldClearServices()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.UpdateServices(new[] { Guid.NewGuid() });

        // Act
        provider.UpdateServices(Array.Empty<Guid>());

        // Assert
        provider.ServiceIds.Should().BeEmpty();
    }

    [Fact]
    public void UpdateServices_WithNull_ShouldSetEmptyArray()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        provider.UpdateServices(null!);

        // Assert
        provider.ServiceIds.Should().NotBeNull();
        provider.ServiceIds.Should().BeEmpty();
    }

    [Fact]
    public void UpdateServices_ShouldCreateNewArray()
    {
        // Arrange
        var provider = CreateValidProvider();
        var originalIds = new[] { Guid.NewGuid() };

        // Act
        provider.UpdateServices(originalIds);
        originalIds[0] = Guid.NewGuid(); // Modify original

        // Assert - provider's array should not change
        provider.ServiceIds[0].Should().NotBe(originalIds[0]);
    }

    #endregion

    #region Activate/Deactivate Tests

    [Fact]
    public void Activate_WhenInactive_ShouldSetActiveToTrue()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.Deactivate();
        provider.IsActive.Should().BeFalse();

        // Act
        provider.Activate();

        // Assert
        provider.IsActive.Should().BeTrue();
        provider.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldNotChangeUpdatedAt()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.IsActive.Should().BeTrue();
        var originalUpdatedAt = provider.UpdatedAt;

        // Act
        provider.Activate();

        // Assert
        provider.IsActive.Should().BeTrue();
        provider.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetActiveToFalse()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.IsActive.Should().BeTrue();

        // Act
        provider.Deactivate();

        // Assert
        provider.IsActive.Should().BeFalse();
        provider.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldNotChangeUpdatedAt()
    {
        // Arrange
        var provider = CreateValidProvider();
        provider.Deactivate();
        var originalUpdatedAt = provider.UpdatedAt;

        // Act
        provider.Deactivate();

        // Assert
        provider.IsActive.Should().BeFalse();
        provider.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void Activate_Then_Deactivate_ShouldToggleCorrectly()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act & Assert - multiple toggles
        provider.IsActive.Should().BeTrue();

        provider.Deactivate();
        provider.IsActive.Should().BeFalse();

        provider.Activate();
        provider.IsActive.Should().BeTrue();

        provider.Deactivate();
        provider.IsActive.Should().BeFalse();
    }

    #endregion

    #region CalculateDistanceToInKm Tests

    [Fact]
    public void CalculateDistanceToInKm_WithSameLocation_ShouldReturnZero()
    {
        // Arrange
        var location = new GeoPoint(-23.5505, -46.6333);
        var provider = SearchableProvider.Create(Guid.NewGuid(), "Provider", location);

        // Act
        var distance = provider.CalculateDistanceToInKm(location);

        // Assert
        distance.Should().Be(0);
    }

    [Fact]
    public void CalculateDistanceToInKm_WithDifferentLocation_ShouldCalculateDistance()
    {
        // Arrange - São Paulo
        var spLocation = new GeoPoint(-23.5505, -46.6333);
        var provider = SearchableProvider.Create(Guid.NewGuid(), "Provider", spLocation);

        // Rio de Janeiro
        var rjLocation = new GeoPoint(-22.9068, -43.1729);

        // Act
        var distance = provider.CalculateDistanceToInKm(rjLocation);

        // Assert - Distance SP to RJ is approximately 357 km
        distance.Should().BeGreaterThan(350);
        distance.Should().BeLessThan(370);
    }

    [Fact]
    public void CalculateDistanceToInKm_WithNullLocation_ShouldThrowArgumentNullException()
    {
        // Arrange
        var provider = CreateValidProvider();

        // Act
        var act = () => provider.CalculateDistanceToInKm(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CalculateDistanceToInKm_ShouldBeSymmetric()
    {
        // Arrange
        var location1 = new GeoPoint(-23.5505, -46.6333); // São Paulo
        var location2 = new GeoPoint(-22.9068, -43.1729); // Rio

        var provider1 = SearchableProvider.Create(Guid.NewGuid(), "P1", location1);
        var provider2 = SearchableProvider.Create(Guid.NewGuid(), "P2", location2);

        // Act
        var distance1to2 = provider1.CalculateDistanceToInKm(location2);
        var distance2to1 = provider2.CalculateDistanceToInKm(location1);

        // Assert
        distance1to2.Should().BeApproximately(distance2to1, 0.1);
    }

    #endregion

    #region Helper Methods

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

    #endregion
}
