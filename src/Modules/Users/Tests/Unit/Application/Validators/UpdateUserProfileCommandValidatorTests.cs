using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.Validators;
using MeAjudaAi.Contracts.Utilities.Constants;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Validators;

public class UpdateUserProfileCommandValidatorTests
{
    private readonly UpdateUserProfileCommandValidator _validator = new();

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.Empty,
            FirstName: "Test",
            LastName: "User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UserId);
    }

    [Fact]
    public void Validate_WithValidUserId_ShouldNotHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "Test",
            LastName: "User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.UserId);
    }

    [Fact]
    public void Validate_WithEmptyFirstName_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "",
            LastName: "User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.FirstName);
    }

    [Fact]
    public void Validate_WithNullFirstName_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: null!,
            LastName: "User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.FirstName);
    }

    [Fact]
    public void Validate_WithValidFirstName_ShouldNotHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "Test",
            LastName: "User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.FirstName);
    }

    [Fact]
    public void Validate_WithTooShortFirstName_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "A",
            LastName: "User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.FirstName)
            .WithErrorMessage(ValidationMessages.Length.FirstNameTooShort);
    }

    [Fact]
    public void Validate_WithTooLongFirstName_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: new string('A', 101),
            LastName: "User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.FirstName)
            .WithErrorMessage(ValidationMessages.Length.FirstNameTooLong);
    }

    [Fact]
    public void Validate_WithEmptyLastName_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "Test",
            LastName: ""
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }

    [Fact]
    public void Validate_WithNullLastName_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "Test",
            LastName: null!
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }

    [Fact]
    public void Validate_WithValidLastName_ShouldNotHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "Test",
            LastName: "User"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.LastName);
    }

    [Fact]
    public void Validate_WithTooShortLastName_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "Test",
            LastName: "U"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.LastName)
            .WithErrorMessage(ValidationMessages.Length.LastNameTooShort);
    }

    [Fact]
    public void Validate_WithTooLongLastName_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "Test",
            LastName: new string('U', 101)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.LastName)
            .WithErrorMessage(ValidationMessages.Length.LastNameTooLong);
    }

    [Fact]
    public void Validate_WithInvalidEmailFormat_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "Test",
            LastName: "User",
            Email: "not-an-email"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Validate_WithEmptyEmailWhenNotNull_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "Test",
            LastName: "User",
            Email: ""
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Validate_WithNullEmail_ShouldNotHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "Test",
            LastName: "User",
            Email: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Validate_WithValidEmail_ShouldNotHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "Test",
            LastName: "User",
            Email: "test@example.com"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Validate_WithInvalidPhoneNumberFormat_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "Test",
            LastName: "User",
            PhoneNumber: "invalid-phone"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.PhoneNumber);
    }

    [Fact]
    public void Validate_WithValidPhoneNumber_ShouldNotHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "Test",
            LastName: "User",
            PhoneNumber: "+5511999999999"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.PhoneNumber);
    }

    [Fact]
    public void Validate_WithNullPhoneNumber_ShouldNotHaveError()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "Test",
            LastName: "User",
            PhoneNumber: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.PhoneNumber);
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            UserId: Guid.NewGuid(),
            FirstName: "Test",
            LastName: "User",
            Email: "test@example.com",
            PhoneNumber: "+5511999999999"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
