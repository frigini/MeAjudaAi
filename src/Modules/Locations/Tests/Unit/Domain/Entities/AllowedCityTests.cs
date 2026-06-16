using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Locations;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Domain.Entities;

public class AllowedCityTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateAllowedCity()
    {
        // Act
        var allowedCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();

        // Assert
        allowedCity.Id.Should().NotBeEmpty();
        allowedCity.CityName.Should().Be("Muriaé");
        allowedCity.StateSigla.Should().Be("MG");
        allowedCity.IbgeCode.Should().BeNull();
        allowedCity.IsActive.Should().BeTrue();
        allowedCity.CreatedBy.Should().Be("admin@test.com");
        allowedCity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        allowedCity.UpdatedAt.Should().BeNull();
        allowedCity.UpdatedBy.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullIbgeCode_ShouldCreateAllowedCity()
    {
        // Act
        var allowedCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();

        // Assert
        allowedCity.IbgeCode.Should().BeNull();
        allowedCity.CityName.Should().Be("Muriaé");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCityName_ShouldThrowArgumentException(string invalidCityName)
    {
        // Arrange & Act - Using new AllowedCity() directly to match actual constructor validation
        var act = () => new AllowedCity(invalidCityName!, "MG", "admin@test.com", null, 0, 0, 0);

        // Assert
        act.Should().Throw<InvalidLocationArgumentException>()
            .WithMessage("*Nome da cidade não pode ser vazio*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidStateSigla_ShouldThrowArgumentException(string invalidStateSigla)
    {
        // Arrange & Act - Using new AllowedCity() directly to match actual constructor validation
        var act = () => new AllowedCity("Muriaé", invalidStateSigla!, "admin@test.com", null, 0, 0, 0);

        // Assert
        act.Should().Throw<InvalidLocationArgumentException>()
            .WithMessage("*Sigla do estado não pode ser vazia*");
    }

    [Theory]
    [InlineData("M")]
    [InlineData("MGA")]
    public void Constructor_WithInvalidStateSiglaLength_ShouldThrowArgumentException(string invalidLength)
    {
        // Arrange & Act - Using new AllowedCity() directly to match actual constructor validation
        var act = () => new AllowedCity("Muriaé", invalidLength, "admin@test.com", null, 0, 0, 0);

        // Assert
        act.Should().Throw<InvalidLocationArgumentException>()
            .WithMessage("*Sigla do estado deve ter 2 caracteres*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCreatedBy_ShouldThrowArgumentException(string invalidCreatedBy)
    {
        // Arrange & Act - Using new AllowedCity() directly to match actual constructor validation
        var act = () => new AllowedCity("Muriaé", "MG", invalidCreatedBy, null, 0, 0, 0);

        // Assert
        act.Should().Throw<InvalidLocationArgumentException>()
            .WithMessage("*CreatedBy não pode ser vazio*");
    }

    [Fact]
    public void Constructor_ShouldNormalizeStateSiglaToUpperCase()
    {
        // Arrange & Act
        var allowedCity = AllowedCityBuilder.AsTestCity("Muriaé", "mg").Build();

        // Assert
        allowedCity.StateSigla.Should().Be("MG");
    }

    [Fact]
    public void Constructor_ShouldTrimCityNameAndStateSigla()
    {
        // Arrange & Act
        var allowedCity = AllowedCityBuilder.AsTestCity("  Muriaé  ", "  mg  ").Build();

        // Assert
        allowedCity.CityName.Should().Be("Muriaé");
        allowedCity.StateSigla.Should().Be("MG");
    }

    [Fact]
    public void Constructor_WithIsActiveFalse_ShouldCreateInactiveCity()
    {
        // Arrange & Act
        var allowedCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG").AsInactive().Build();

        // Assert
        allowedCity.IsActive.Should().BeFalse();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateAllowedCity()
    {
        // Arrange
        var allowedCity = AllowedCityBuilder.AsTestCity("Juiz de Fora", "MG")
            .WithCreatedBy("test@user.com")
            .WithIbgeCode(3136702)
            .Build();
        var newCityName = "Itaperuna";
        var newStateSigla = "RJ";
        var newIbgeCode = 3302270;
        var updatedBy = "admin2@test.com";

        // Act
        allowedCity.Update(newCityName, newStateSigla, newIbgeCode, 0, 0, 0, true, updatedBy);

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
        var allowedCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();

        // Act
        var act = () => allowedCity.Update(invalidCityName, "RJ", null, 0, 0, 0, true, "admin@test.com");

        // Assert
        act.Should().Throw<InvalidLocationArgumentException>()
            .WithMessage("*Nome da cidade não pode ser vazio*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidStateSigla_ShouldThrowArgumentException(string invalidStateSigla)
    {
        // Arrange
        var allowedCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();

        // Act
        var act = () => allowedCity.Update("Itaperuna", invalidStateSigla, null, 0, 0, 0, true, "admin@test.com");

        // Assert
        act.Should().Throw<InvalidLocationArgumentException>()
            .WithMessage("*Sigla do estado não pode ser vazia*");
    }

    [Theory]
    [InlineData("M")]
    [InlineData("RJX")]
    public void Update_WithInvalidStateSiglaLength_ShouldThrowArgumentException(string invalidLength)
    {
        // Arrange
        var allowedCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();

        // Act
        var act = () => allowedCity.Update("Itaperuna", invalidLength, null, 0, 0, 0, true, "admin@test.com");

        // Assert
        act.Should().Throw<InvalidLocationArgumentException>()
            .WithMessage("*Sigla do estado deve ter 2 caracteres*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidUpdatedBy_ShouldThrowArgumentException(string invalidUpdatedBy)
    {
        // Arrange
        var allowedCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();

        // Act
        var act = () => allowedCity.Update("Itaperuna", "RJ", null, 0, 0, 0, true, invalidUpdatedBy);

        // Assert
        act.Should().Throw<InvalidLocationArgumentException>()
            .WithMessage("*UpdatedBy não pode ser vazio*");
    }

    [Fact]
    public void Update_ShouldNormalizeStateSiglaToUpperCase()
    {
        // Arrange
        var allowedCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();

        // Act
        allowedCity.Update("Itaperuna", "rj", null, 0, 0, 0, true, "admin@test.com");

        // Assert
        allowedCity.StateSigla.Should().Be("RJ");
    }

    [Fact]
    public void Update_ShouldTrimCityNameAndStateSigla()
    {
        // Arrange
        var allowedCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();

        // Act
        allowedCity.Update("  Itaperuna  ", "  rj  ", null, 0, 0, 0, true, "admin@test.com");

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
        var allowedCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG").AsInactive().Build();

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
        var allowedCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG").AsInactive().Build();

        // Act
        var act = () => allowedCity.Activate(invalidUpdatedBy);

        // Assert
        act.Should().Throw<InvalidLocationArgumentException>()
            .WithMessage("*UpdatedBy não pode ser vazio*");
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var allowedCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();

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
        var allowedCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();

        // Act
        var act = () => allowedCity.Deactivate(invalidUpdatedBy);

        // Assert
        act.Should().Throw<InvalidLocationArgumentException>()
            .WithMessage("*UpdatedBy não pode ser vazio*");
    }

    #endregion
}
