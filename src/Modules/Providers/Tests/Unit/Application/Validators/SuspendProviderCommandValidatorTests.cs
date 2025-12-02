using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Validators;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Validators;

public class SuspendProviderCommandValidatorTests
{
    private readonly SuspendProviderCommandValidator _validator;

    public SuspendProviderCommandValidatorTests()
    {
        _validator = new SuspendProviderCommandValidator();
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new SuspendProviderCommand(
            ProviderId: Guid.NewGuid(),
            Reason: "This provider violated terms of service repeatedly and must be temporarily suspended",
            SuspendedBy: "admin@example.com"
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
        var command = new SuspendProviderCommand(
            ProviderId: Guid.Empty,
            Reason: "This provider violated terms of service",
            SuspendedBy: "admin@example.com"
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
    public async Task Validate_WithEmptySuspendedBy_ShouldHaveValidationError(string suspendedBy)
    {
        // Arrange
        var command = new SuspendProviderCommand(
            ProviderId: Guid.NewGuid(),
            Reason: "This provider violated terms of service",
            SuspendedBy: suspendedBy
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SuspendedBy)
            .WithErrorMessage("SuspendedBy is required for audit purposes");
    }

    [Fact]
    public async Task Validate_WithSuspendedByExceeding255Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longSuspendedBy = new string('A', 256);
        var command = new SuspendProviderCommand(
            ProviderId: Guid.NewGuid(),
            Reason: "This provider violated terms of service",
            SuspendedBy: longSuspendedBy
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SuspendedBy)
            .WithErrorMessage("SuspendedBy cannot exceed 255 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Validate_WithEmptyReason_ShouldHaveValidationError(string reason)
    {
        // Arrange
        var command = new SuspendProviderCommand(
            ProviderId: Guid.NewGuid(),
            Reason: reason,
            SuspendedBy: "admin@example.com"
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
        var command = new SuspendProviderCommand(
            ProviderId: Guid.NewGuid(),
            Reason: "Too short",
            SuspendedBy: "admin@example.com"
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
        var command = new SuspendProviderCommand(
            ProviderId: Guid.NewGuid(),
            Reason: longReason,
            SuspendedBy: "admin@example.com"
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason cannot exceed 1000 characters");
    }
}
