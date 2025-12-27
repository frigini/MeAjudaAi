using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.Validators;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.Validators;

public class UpdateAllowedCityCommandValidatorTests
{
    private readonly UpdateAllowedCityCommandValidator _validator = new();

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateAllowedCityCommand
        {
            Id = Guid.Empty,
            CityName = "S\u00e3o Paulo",
            StateSigla = "SP"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Validate_WithEmptyCityName_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateAllowedCityCommand
        {
            Id = Guid.NewGuid(),
            CityName = "",
            StateSigla = "SP"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CityName);
    }

    [Fact]
    public void Validate_WithNullCityName_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateAllowedCityCommand
        {
            Id = Guid.NewGuid(),
            CityName = null!,
            StateSigla = "SP"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CityName);
    }

    [Fact]
    public void Validate_WithTooLongCityName_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateAllowedCityCommand
        {
            Id = Guid.NewGuid(),
            CityName = new string('A', 101), // 101 characters
            StateSigla = "SP"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CityName);
    }

    [Fact]
    public void Validate_WithEmptyStateSigla_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateAllowedCityCommand
        {
            Id = Guid.NewGuid(),
            CityName = "S\u00e3o Paulo",
            StateSigla = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.StateSigla);
    }

    [Fact]
    public void Validate_WithNullStateSigla_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateAllowedCityCommand
        {
            Id = Guid.NewGuid(),
            CityName = "S\u00e3o Paulo",
            StateSigla = null!
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.StateSigla);
    }

    [Fact]
    public void Validate_WithTooShortStateSigla_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateAllowedCityCommand
        {
            Id = Guid.NewGuid(),
            CityName = "S\u00e3o Paulo",
            StateSigla = "S" // Only 1 character
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.StateSigla);
    }

    [Fact]
    public void Validate_WithTooLongStateSigla_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateAllowedCityCommand
        {
            Id = Guid.NewGuid(),
            CityName = "S\u00e3o Paulo",
            StateSigla = "SPP" // 3 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.StateSigla);
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotHaveError()
    {
        // Arrange
        var command = new UpdateAllowedCityCommand
        {
            Id = Guid.NewGuid(),
            CityName = "S\u00e3o Paulo",
            StateSigla = "SP"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMinimumLengthCityName_ShouldNotHaveError()
    {
        // Arrange
        var command = new UpdateAllowedCityCommand
        {
            Id = Guid.NewGuid(),
            CityName = "A", // 1 character (minimum)
            StateSigla = "SP"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.CityName);
    }

    [Fact]
    public void Validate_WithMaximumLengthCityName_ShouldNotHaveError()
    {
        // Arrange
        var command = new UpdateAllowedCityCommand
        {
            Id = Guid.NewGuid(),
            CityName = new string('A', 100), // 100 characters (maximum)
            StateSigla = "SP"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.CityName);
    }
}
