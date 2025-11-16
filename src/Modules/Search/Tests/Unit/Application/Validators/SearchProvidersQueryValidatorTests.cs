using FluentAssertions;
using MeAjudaAi.Modules.Search.Application.Queries;
using MeAjudaAi.Modules.Search.Application.Validators;
using MeAjudaAi.Modules.Search.Domain.Enums;

namespace MeAjudaAi.Modules.Search.Tests.Unit.Application.Validators;

/// <summary>
/// Testes unitários para SearchProvidersQueryValidator
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Search")]
[Trait("Component", "Validator")]
public class SearchProvidersQueryValidatorTests
{
    private readonly SearchProvidersQueryValidator _validator;

    public SearchProvidersQueryValidatorTests()
    {
        _validator = new SearchProvidersQueryValidator();
    }

    [Fact]
    public void Validate_WithValidQuery_ShouldPass()
    {
        // Arrange
        var query = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: 10,
            PageNumber: 1,
            PageSize: 20);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    [InlineData(-100)]
    [InlineData(100)]
    public void Validate_WithInvalidLatitude_ShouldFail(double invalidLatitude)
    {
        // Arrange
        var query = new SearchProvidersQuery(
            Latitude: invalidLatitude,
            Longitude: -46.6333,
            RadiusInKm: 10);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        var error = result.Errors.Single(e => e.PropertyName == nameof(SearchProvidersQuery.Latitude));
        error.ErrorMessage.Should().Contain("between -90 and 90");
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    [InlineData(-200)]
    [InlineData(200)]
    public void Validate_WithInvalidLongitude_ShouldFail(double invalidLongitude)
    {
        // Arrange
        var query = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: invalidLongitude,
            RadiusInKm: 10);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        var error = result.Errors.Single(e => e.PropertyName == nameof(SearchProvidersQuery.Longitude));
        error.ErrorMessage.Should().Contain("between -180 and 180");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_WithZeroOrNegativeRadius_ShouldFail(double invalidRadius)
    {
        // Arrange
        var query = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: invalidRadius);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        var error = result.Errors.Single(e => e.PropertyName == nameof(SearchProvidersQuery.RadiusInKm));
        error.ErrorMessage.Should().Contain("greater than 0");
    }

    [Fact]
    public void Validate_WithRadiusExceeding500Km_ShouldFail()
    {
        // Arrange
        var query = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: 501);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        var error = result.Errors.Single(e => e.PropertyName == nameof(SearchProvidersQuery.RadiusInKm));
        error.ErrorMessage.Should().Contain("cannot exceed 500");
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(5.1)]
    [InlineData(10)]
    public void Validate_WithInvalidMinRating_ShouldFail(decimal invalidRating)
    {
        // Arrange
        var query = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: 10,
            MinRating: invalidRating);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        var error = result.Errors.Single(e => e.PropertyName == nameof(SearchProvidersQuery.MinRating));
        error.ErrorMessage.Should().Contain("between 0 and 5");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidPageNumber_ShouldFail(int invalidPage)
    {
        // Arrange
        var query = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: 10,
            PageNumber: invalidPage);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        var error = result.Errors.Single(e => e.PropertyName == nameof(SearchProvidersQuery.PageNumber));
        error.ErrorMessage.Should().Contain("greater than 0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidPageSize_ShouldFail(int invalidSize)
    {
        // Arrange
        var query = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: 10,
            PageSize: invalidSize);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        var error = result.Errors.Single(e => e.PropertyName == nameof(SearchProvidersQuery.PageSize));
        error.ErrorMessage.Should().Contain("greater than 0");
    }

    [Fact]
    public void Validate_WithPageSizeExceeding100_ShouldFail()
    {
        // Arrange
        var query = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: 10,
            PageSize: 101);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        var error = result.Errors.Single(e => e.PropertyName == nameof(SearchProvidersQuery.PageSize));
        error.ErrorMessage.Should().Contain("cannot exceed 100");
    }

    [Fact]
    public void Validate_WithAllFilters_ShouldPass()
    {
        // Arrange
        var query = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: 50,
            ServiceIds: new[] { Guid.NewGuid(), Guid.NewGuid() },
            MinRating: 4.5m,
            SubscriptionTiers: new[] { ESubscriptionTier.Gold, ESubscriptionTier.Platinum },
            PageNumber: 2,
            PageSize: 50);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
