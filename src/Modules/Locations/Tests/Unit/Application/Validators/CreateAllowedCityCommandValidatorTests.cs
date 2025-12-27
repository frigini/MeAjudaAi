using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.Validators;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.Validators;

public class CreateAllowedCityCommandValidatorTests
{
    private readonly CreateAllowedCityCommandValidator _validator = new();

    [Fact]
    public void Validate_WithEmptyCityName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateAllowedCityCommand(
            CityName: "",
            StateSigla: "SP",
            IbgeCode: null,
            IsActive: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CityName);
    }

    [Fact]
    public void Validate_WithNullCityName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateAllowedCityCommand(
            CityName: null!,
            StateSigla: "SP",
            IbgeCode: null,
            IsActive: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CityName);
    }

    [Fact]
    public void Validate_WithTooLongCityName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateAllowedCityCommand(
            CityName: new string('A', 101), // 101 characters
            StateSigla: "SP",
            IbgeCode: null,
            IsActive: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CityName);
    }

    [Fact]
    public void Validate_WithEmptyStateSigla_ShouldHaveError()
    {
        // Arrange
        var command = new CreateAllowedCityCommand(
            CityName: "S\u00e3o Paulo",
            StateSigla: "",
            IbgeCode: null,
            IsActive: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.StateSigla);
    }

    [Fact]
    public void Validate_WithNullStateSigla_ShouldHaveError()
    {
        // Arrange
        var command = new CreateAllowedCityCommand(
            CityName: "S\u00e3o Paulo",
            StateSigla: null!,
            IbgeCode: null,
            IsActive: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.StateSigla);
    }

    [Fact]
    public void Validate_WithTooShortStateSigla_ShouldHaveError()
    {
        // Arrange
        var command = new CreateAllowedCityCommand(
            CityName: "S\u00e3o Paulo",
            StateSigla: "S", // Only 1 character
            IbgeCode: null,
            IsActive: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.StateSigla);
    }

    [Fact]
    public void Validate_WithTooLongStateSigla_ShouldHaveError()
    {
        // Arrange
        var command = new CreateAllowedCityCommand(
            CityName: "S\u00e3o Paulo",
            StateSigla: "SPP", // 3 characters
            IbgeCode: null,
            IsActive: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.StateSigla);
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateAllowedCityCommand(
            CityName: "S\u00e3o Paulo",
            StateSigla: "SP",
            IbgeCode: 3550308,
            IsActive: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMinimumLengthCityName_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateAllowedCityCommand(
            CityName: "A", // 1 character (minimum)
            StateSigla: "SP",
            IbgeCode: null,
            IsActive: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.CityName);
    }

    [Fact]
    public void Validate_WithMaximumLengthCityName_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateAllowedCityCommand(
            CityName: new string('A', 100), // 100 characters (maximum)
            StateSigla: "SP",
            IbgeCode: null,
            IsActive: true
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.CityName);
    }
}
