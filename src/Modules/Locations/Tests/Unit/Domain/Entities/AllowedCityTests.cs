using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Domain.Entities;

public class AllowedCityTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateAllowedCity()
    {
        // Arrange
        var cityName = "Muriaé";
        var stateSigla = "MG";
        var ibgeCode = "3143906";
        var createdBy = "admin@test.com";

        // Act
        var allowedCity = new AllowedCity(cityName, stateSigla, ibgeCode, createdBy);

        // Assert
        allowedCity.Id.Should().NotBeEmpty();
        allowedCity.CityName.Should().Be(cityName);
        allowedCity.StateSigla.Should().Be(stateSigla);
        allowedCity.IbgeCode.Should().Be(ibgeCode);
        allowedCity.IsActive.Should().BeTrue();
        allowedCity.CreatedBy.Should().Be(createdBy);
        allowedCity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        allowedCity.UpdatedAt.Should().BeNull();
        allowedCity.UpdatedBy.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullIbgeCode_ShouldCreateAllowedCity()
    {
        // Arrange
        var cityName = "Muriaé";
        var stateSigla = "MG";
        var createdBy = "admin@test.com";

        // Act
        var allowedCity = new AllowedCity(cityName, stateSigla, null, createdBy);

        // Assert
        allowedCity.IbgeCode.Should().BeNull();
        allowedCity.CityName.Should().Be(cityName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCityName_ShouldThrowArgumentException(string? invalidCityName)
    {
        // Arrange
        var stateSigla = "MG";
        var createdBy = "admin@test.com";

        // Act
        var act = () => new AllowedCity(invalidCityName!, stateSigla, null, createdBy);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cityName*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidStateSigla_ShouldThrowArgumentException(string? invalidStateSigla)
    {
        // Arrange
        var cityName = "Muriaé";
        var createdBy = "admin@test.com";

        // Act
        var act = () => new AllowedCity(cityName, invalidStateSigla!, null, createdBy);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*stateSigla*");
    }

    [Theory]
    [InlineData("M")]
    [InlineData("MGS")]
    [InlineData("MINAS")]
    public void Constructor_WithStateSiglaNotTwoCharacters_ShouldThrowArgumentException(string invalidStateSigla)
    {
        // Arrange
        var cityName = "Muriaé";
        var createdBy = "admin@test.com";

        // Act
        var act = () => new AllowedCity(cityName, invalidStateSigla, null, createdBy);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*2 caracteres*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCreatedBy_ShouldThrowArgumentException(string? invalidCreatedBy)
    {
        // Arrange
        var cityName = "Muriaé";
        var stateSigla = "MG";

        // Act
        var act = () => new AllowedCity(cityName, stateSigla, null, invalidCreatedBy!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*createdBy*");
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateAllowedCity()
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", "3143906", "admin@test.com");
        var originalCreatedAt = allowedCity.CreatedAt;
        var newCityName = "Itaperuna";
        var newStateSigla = "RJ";
        var newIbgeCode = "3302270";
        var updatedBy = "admin2@test.com";

        // Act
        allowedCity.Update(newCityName, newStateSigla, newIbgeCode, updatedBy);

        // Assert
        allowedCity.CityName.Should().Be(newCityName);
        allowedCity.StateSigla.Should().Be(newStateSigla);
        allowedCity.IbgeCode.Should().Be(newIbgeCode);
        allowedCity.UpdatedBy.Should().Be(updatedBy);
        allowedCity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        allowedCity.CreatedAt.Should().Be(originalCreatedAt);
        allowedCity.CreatedBy.Should().Be("admin@test.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidCityName_ShouldThrowArgumentException(string? invalidCityName)
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", null, "admin@test.com");
        var updatedBy = "admin2@test.com";

        // Act
        var act = () => allowedCity.Update(invalidCityName!, "RJ", null, updatedBy);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cityName*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidStateSigla_ShouldThrowArgumentException(string? invalidStateSigla)
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", null, "admin@test.com");
        var updatedBy = "admin2@test.com";

        // Act
        var act = () => allowedCity.Update("Itaperuna", invalidStateSigla!, null, updatedBy);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*stateSigla*");
    }

    [Theory]
    [InlineData("M")]
    [InlineData("RJS")]
    public void Update_WithStateSiglaNotTwoCharacters_ShouldThrowArgumentException(string invalidStateSigla)
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", null, "admin@test.com");
        var updatedBy = "admin2@test.com";

        // Act
        var act = () => allowedCity.Update("Itaperuna", invalidStateSigla, null, updatedBy);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*2 caracteres*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidUpdatedBy_ShouldThrowArgumentException(string? invalidUpdatedBy)
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", null, "admin@test.com");

        // Act
        var act = () => allowedCity.Update("Itaperuna", "RJ", null, invalidUpdatedBy!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*updatedBy*");
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateCity()
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", null, "admin@test.com");
        allowedCity.Deactivate("admin@test.com");
        var updatedBy = "admin2@test.com";

        // Act
        allowedCity.Activate(updatedBy);

        // Assert
        allowedCity.IsActive.Should().BeTrue();
        allowedCity.UpdatedBy.Should().Be(updatedBy);
        allowedCity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Activate_WithInvalidUpdatedBy_ShouldThrowArgumentException(string? invalidUpdatedBy)
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", null, "admin@test.com");
        allowedCity.Deactivate("admin@test.com");

        // Act
        var act = () => allowedCity.Activate(invalidUpdatedBy!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*updatedBy*");
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateCity()
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", null, "admin@test.com");
        var updatedBy = "admin2@test.com";

        // Act
        allowedCity.Deactivate(updatedBy);

        // Assert
        allowedCity.IsActive.Should().BeFalse();
        allowedCity.UpdatedBy.Should().Be(updatedBy);
        allowedCity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deactivate_WithInvalidUpdatedBy_ShouldThrowArgumentException(string? invalidUpdatedBy)
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", null, "admin@test.com");

        // Act
        var act = () => allowedCity.Deactivate(invalidUpdatedBy!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*updatedBy*");
    }
}
