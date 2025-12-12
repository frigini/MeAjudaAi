using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Domain.Entities;

public class AllowedCityTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateAllowedCity()
    {
        // Arrange
        var cityName = "Muriaé";
        var stateSigla = "MG";
        var ibgeCode = 3143906;
        var createdBy = "admin@test.com";

        // Act
        var allowedCity = new AllowedCity(cityName, stateSigla, createdBy, ibgeCode);

        // Assert
        allowedCity.Id.Should().NotBeEmpty();
        allowedCity.CityName.Should().Be(cityName);
        allowedCity.StateSigla.Should().Be("MG");
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
        var allowedCity = new AllowedCity(cityName, stateSigla, createdBy);

        // Assert
        allowedCity.IbgeCode.Should().BeNull();
        allowedCity.CityName.Should().Be(cityName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCityName_ShouldThrowArgumentException(string invalidCityName)
    {
        // Arrange & Act
        var act = () => new AllowedCity(invalidCityName, "MG", "admin@test.com");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Nome da cidade não pode ser vazio*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidStateSigla_ShouldThrowArgumentException(string invalidStateSigla)
    {
        // Arrange & Act
        var act = () => new AllowedCity("Muriaé", invalidStateSigla, "admin@test.com");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Sigla do estado não pode ser vazia*");
    }

    [Theory]
    [InlineData("M")]
    [InlineData("MGA")]
    public void Constructor_WithInvalidStateSiglaLength_ShouldThrowArgumentException(string invalidLength)
    {
        // Arrange & Act
        var act = () => new AllowedCity("Muriaé", invalidLength, "admin@test.com");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Sigla do estado deve ter 2 caracteres*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCreatedBy_ShouldThrowArgumentException(string invalidCreatedBy)
    {
        // Arrange & Act
        var act = () => new AllowedCity("Muriaé", "MG", invalidCreatedBy);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*CreatedBy não pode ser vazio*");
    }

    [Fact]
    public void Constructor_ShouldNormalizeStateSiglaToUpperCase()
    {
        // Arrange & Act
        var allowedCity = new AllowedCity("Muriaé", "mg", "admin@test.com");

        // Assert
        allowedCity.StateSigla.Should().Be("MG");
    }

    [Fact]
    public void Constructor_ShouldTrimCityNameAndStateSigla()
    {
        // Arrange & Act
        var allowedCity = new AllowedCity("  Muriaé  ", "  mg  ", "admin@test.com");

        // Assert
        allowedCity.CityName.Should().Be("Muriaé");
        allowedCity.StateSigla.Should().Be("MG");
    }

    [Fact]
    public void Constructor_WithIsActiveFalse_ShouldCreateInactiveCity()
    {
        // Arrange & Act
        var allowedCity = new AllowedCity("Muriaé", "MG", "admin@test.com", isActive: false);

        // Assert
        allowedCity.IsActive.Should().BeFalse();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateAllowedCity()
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906);
        var newCityName = "Itaperuna";
        var newStateSigla = "RJ";
        var newIbgeCode = 3302270;
        var updatedBy = "admin2@test.com";

        // Act
        allowedCity.Update(newCityName, newStateSigla, newIbgeCode, true, updatedBy);

        // Assert
        allowedCity.CityName.Should().Be(newCityName);
        allowedCity.StateSigla.Should().Be("RJ");
        allowedCity.IbgeCode.Should().Be(newIbgeCode);
        allowedCity.UpdatedBy.Should().Be(updatedBy);
        allowedCity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidCityName_ShouldThrowArgumentException(string invalidCityName)
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", "admin@test.com");

        // Act
        var act = () => allowedCity.Update(invalidCityName, "RJ", null, true, "admin@test.com");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Nome da cidade não pode ser vazio*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidStateSigla_ShouldThrowArgumentException(string invalidStateSigla)
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", "admin@test.com");

        // Act
        var act = () => allowedCity.Update("Itaperuna", invalidStateSigla, null, true, "admin@test.com");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Sigla do estado não pode ser vazia*");
    }

    [Theory]
    [InlineData("M")]
    [InlineData("RJX")]
    public void Update_WithInvalidStateSiglaLength_ShouldThrowArgumentException(string invalidLength)
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", "admin@test.com");

        // Act
        var act = () => allowedCity.Update("Itaperuna", invalidLength, null, true, "admin@test.com");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Sigla do estado deve ter 2 caracteres*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidUpdatedBy_ShouldThrowArgumentException(string invalidUpdatedBy)
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", "admin@test.com");

        // Act
        var act = () => allowedCity.Update("Itaperuna", "RJ", null, true, invalidUpdatedBy);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*UpdatedBy não pode ser vazio*");
    }

    [Fact]
    public void Update_ShouldNormalizeStateSiglaToUpperCase()
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", "admin@test.com");

        // Act
        allowedCity.Update("Itaperuna", "rj", null, true, "admin@test.com");

        // Assert
        allowedCity.StateSigla.Should().Be("RJ");
    }

    [Fact]
    public void Update_ShouldTrimCityNameAndStateSigla()
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", "admin@test.com");

        // Act
        allowedCity.Update("  Itaperuna  ", "  rj  ", null, true, "admin@test.com");

        // Assert
        allowedCity.CityName.Should().Be("Itaperuna");
        allowedCity.StateSigla.Should().Be("RJ");
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", "admin@test.com", isActive: false);

        // Act
        allowedCity.Activate("admin@test.com");

        // Assert
        allowedCity.IsActive.Should().BeTrue();
        allowedCity.UpdatedBy.Should().Be("admin@test.com");
        allowedCity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Activate_WithInvalidUpdatedBy_ShouldThrowArgumentException(string invalidUpdatedBy)
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", "admin@test.com", isActive: false);

        // Act
        var act = () => allowedCity.Activate(invalidUpdatedBy);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*UpdatedBy não pode ser vazio*");
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", "admin@test.com");

        // Act
        allowedCity.Deactivate("admin@test.com");

        // Assert
        allowedCity.IsActive.Should().BeFalse();
        allowedCity.UpdatedBy.Should().Be("admin@test.com");
        allowedCity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deactivate_WithInvalidUpdatedBy_ShouldThrowArgumentException(string invalidUpdatedBy)
    {
        // Arrange
        var allowedCity = new AllowedCity("Muriaé", "MG", "admin@test.com");

        // Act
        var act = () => allowedCity.Deactivate(invalidUpdatedBy);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*UpdatedBy não pode ser vazio*");
    }

    #endregion
}
