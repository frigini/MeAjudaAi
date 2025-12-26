using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.Validators;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.Validators;

public class DeleteAllowedCityCommandValidatorTests
{
    private readonly DeleteAllowedCityCommandValidator _validator = new();

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var command = new DeleteAllowedCityCommand
        {
            Id = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Validate_WithValidId_ShouldNotHaveError()
    {
        // Arrange
        var command = new DeleteAllowedCityCommand
        {
            Id = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
