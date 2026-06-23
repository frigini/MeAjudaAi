using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Validators;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Validators;

public class ActivateProviderCommandValidatorTests
{
    private readonly ActivateProviderCommandValidator _validator;

    public ActivateProviderCommandValidatorTests()
    {
        _validator = new ActivateProviderCommandValidator();
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new ActivateProviderCommand(Guid.NewGuid(), "admin@system.com");

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithEmptyProviderId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ActivateProviderCommand(Guid.Empty, "admin@system.com");

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProviderId)
            .WithErrorMessage("O ID do prestador é obrigatório.");
    }

    [Theory]
    [InlineData(null)]
    public async Task Validate_WithNullActivatedBy_ShouldNotHaveValidationError(string? activatedBy)
    {
        // Arrange
        var command = new ActivateProviderCommand(Guid.NewGuid(), activatedBy);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ActivatedBy);
    }
}
