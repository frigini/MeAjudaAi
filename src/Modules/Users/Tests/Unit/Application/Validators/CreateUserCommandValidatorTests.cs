using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.Validators;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Validators;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveError()
    {
        // Arrange
        var command = new CreateUserCommand(
            Username: "testuser",
            Email: "not-an-email", // Invalid format
            FirstName: "Test",
            LastName: "User",
            Password: "ValidPass123",
            Roles: Array.Empty<string>(),
            PhoneNumber: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Validate_WithValidEmail_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateUserCommand(
            Username: "testuser",
            Email: "test@example.com", // Valid format
            FirstName: "Test",
            LastName: "User",
            Password: "ValidPass123",
            Roles: Array.Empty<string>(),
            PhoneNumber: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Email);
    }
}
