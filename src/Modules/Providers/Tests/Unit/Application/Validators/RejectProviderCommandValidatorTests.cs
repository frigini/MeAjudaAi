using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Validators;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Validators;

public class RejectProviderCommandValidatorTests
{
    private readonly RejectProviderCommandValidator _validator;

    public RejectProviderCommandValidatorTests()
    {
        _validator = new RejectProviderCommandValidator();
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new RejectProviderCommand(
            ProviderId: Guid.NewGuid(),
            Reason: "This provider submitted invalid documentation and does not meet requirements",
            RejectedBy: "admin@example.com"
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithEmptyProviderId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RejectProviderCommand(
            ProviderId: Guid.Empty,
            Reason: "This provider submitted invalid documentation",
            RejectedBy: "admin@example.com"
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProviderId)
            .WithErrorMessage("Provider ID is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Validate_WithEmptyRejectedBy_ShouldHaveValidationError(string rejectedBy)
    {
        // Arrange
        var command = new RejectProviderCommand(
            ProviderId: Guid.NewGuid(),
            Reason: "This provider submitted invalid documentation",
            RejectedBy: rejectedBy
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RejectedBy)
            .WithErrorMessage("RejectedBy is required for audit purposes");
    }

    [Fact]
    public async Task Validate_WithRejectedByExceeding255Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longRejectedBy = new string('A', 256);
        var command = new RejectProviderCommand(
            ProviderId: Guid.NewGuid(),
            Reason: "This provider submitted invalid documentation",
            RejectedBy: longRejectedBy
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RejectedBy)
            .WithErrorMessage("RejectedBy cannot exceed 255 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Validate_WithEmptyReason_ShouldHaveValidationError(string reason)
    {
        // Arrange
        var command = new RejectProviderCommand(
            ProviderId: Guid.NewGuid(),
            Reason: reason,
            RejectedBy: "admin@example.com"
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason is required for audit purposes");
    }

    [Fact]
    public async Task Validate_WithReasonLessThan10Characters_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RejectProviderCommand(
            ProviderId: Guid.NewGuid(),
            Reason: "Too short",
            RejectedBy: "admin@example.com"
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason must be at least 10 characters");
    }

    [Fact]
    public async Task Validate_WithReasonExceeding1000Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longReason = new string('A', 1001);
        var command = new RejectProviderCommand(
            ProviderId: Guid.NewGuid(),
            Reason: longReason,
            RejectedBy: "admin@example.com"
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason cannot exceed 1000 characters");
    }
}
