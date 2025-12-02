using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Application.Validators;
using MeAjudaAi.Modules.Providers.Domain.Enums;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Validators;

public class UpdateVerificationStatusRequestValidatorTests
{
    private readonly UpdateVerificationStatusRequestValidator _validator;

    public UpdateVerificationStatusRequestValidatorTests()
    {
        _validator = new UpdateVerificationStatusRequestValidator();
    }

    [Fact]
    public async Task Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new UpdateVerificationStatusRequest
        {
            Status = EVerificationStatus.Verified,
            Notes = "All documents have been verified successfully"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithValidRequestWithoutNotes_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new UpdateVerificationStatusRequest
        {
            Status = EVerificationStatus.Verified,
            Notes = null
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithInvalidStatus_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateVerificationStatusRequest
        {
            Status = (EVerificationStatus)999,
            Notes = "Some notes"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public async Task Validate_WithNotesExceeding1000Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longNotes = new string('A', 1001);
        var request = new UpdateVerificationStatusRequest
        {
            Status = EVerificationStatus.Verified,
            Notes = longNotes
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage("Notes cannot exceed 1000 characters");
    }

    [Fact]
    public async Task Validate_WithNotesExactly1000Characters_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var notes = new string('A', 1000);
        var request = new UpdateVerificationStatusRequest
        {
            Status = EVerificationStatus.Verified,
            Notes = notes
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }
}
