using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Validators;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Validators;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _validator;

    public CreateUserRequestValidatorTests()
    {
        _validator = new CreateUserRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Test",
            LastName = "User",
            Roles = ["Customer"]
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Validate_EmptyUsername_ShouldHaveValidationError(string? username)
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = username ?? string.Empty,
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username is required");
    }

    [Theory]
    [InlineData("ab")] // Too short
    [InlineData("a")] // Too short
    [InlineData("this_is_a_very_long_username_that_exceeds_fifty_chars")] // Too long
    public void Validate_InvalidUsernameLength_ShouldHaveValidationError(string username)
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = username,
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username must be between 3 and 50 characters");
    }

    [Theory]
    [InlineData("user@name")] // Invalid character
    [InlineData("user name")] // Space not allowed
    [InlineData("user#name")] // Invalid character
    [InlineData("user%name")] // Invalid character
    public void Validate_InvalidUsernameFormat_ShouldHaveValidationError(string username)
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = username,
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username must contain only letters, numbers, dots, hyphens or underscores");
    }

    [Theory]
    [InlineData("test.user")]
    [InlineData("test-user")]
    [InlineData("test_user")]
    [InlineData("testuser123")]
    [InlineData("123test")]
    public void Validate_ValidUsernameFormats_ShouldNotHaveValidationError(string username)
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = username,
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Validate_EmptyEmail_ShouldHaveValidationError(string? email)
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = email ?? string.Empty,
            Password = "Password123",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email is required");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test.example.com")]
    public void Validate_InvalidEmailFormat_ShouldHaveValidationError(string email)
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = email,
            Password = "Password123",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email must have a valid format");
    }

    [Fact]
    public void Validate_EmailTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longEmail = string.Concat(Enumerable.Repeat("a", 250)) + "@example.com"; // Over 255 characters
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = longEmail,
            Password = "Password123",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email cannot exceed 255 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Validate_EmptyPassword_ShouldHaveValidationError(string? password)
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = password ?? string.Empty,
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password is required");
    }

    [Theory]
    [InlineData("1234567")] // Too short
    [InlineData("short")]
    public void Validate_PasswordTooShort_ShouldHaveValidationError(string password)
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = password,
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must be at least 8 characters long");
    }

    [Theory]
    [InlineData("password123")] // No uppercase
    [InlineData("PASSWORD123")] // No lowercase
    [InlineData("PasswordABC")] // No number
    [InlineData("12345678")] // No letters
    public void Validate_PasswordMissingRequiredCharacters_ShouldHaveValidationError(string password)
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = password,
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must contain at least one lowercase letter, one uppercase letter and one number");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Validate_EmptyFirstName_ShouldHaveValidationError(string? firstName)
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            FirstName = firstName ?? string.Empty,
            LastName = "User"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("First name is required");
    }

    [Theory]
    [InlineData("A")] // Too short
    [InlineData("ThisIsAVeryLongFirstNameThatExceedsOneHundredCharactersAndShouldFailValidationBecauseItIsTooLongForTheSystem")] // Too long
    public void Validate_InvalidFirstNameLength_ShouldHaveValidationError(string firstName)
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            FirstName = firstName,
            LastName = "User"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("First name must be between 2 and 100 characters");
    }

    [Theory]
    [InlineData("John123")] // Numbers not allowed
    [InlineData("John@")] // Special characters not allowed
    [InlineData("John-")] // Hyphens not allowed
    public void Validate_InvalidFirstNameFormat_ShouldHaveValidationError(string firstName)
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            FirstName = firstName,
            LastName = "User"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("First name must contain only letters and spaces");
    }

    [Theory]
    [InlineData("John")]
    [InlineData("Mary Jane")]
    [InlineData("José")]
    [InlineData("François")]
    public void Validate_ValidFirstNames_ShouldNotHaveValidationError(string firstName)
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            FirstName = firstName,
            LastName = "User"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Validate_EmptyLastName_ShouldHaveValidationError(string? lastName)
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Test",
            LastName = lastName ?? string.Empty
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
              .WithErrorMessage("Last name is required");
    }
}