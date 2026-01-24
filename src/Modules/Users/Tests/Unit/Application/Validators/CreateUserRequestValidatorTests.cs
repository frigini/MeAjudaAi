using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Validators;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Contracts.Utilities.Constants;

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
              .WithErrorMessage(ValidationMessages.Required.Username);
    }

    [Theory]
    [InlineData("ab", ValidationMessages.Length.UsernameTooShort)] // Muito curto
    [InlineData("a", ValidationMessages.Length.UsernameTooShort)] // Muito curto
    [InlineData("this_is_a_very_long_username_that_exceeds_fifty_chars", ValidationMessages.Length.UsernameTooLong)] // Muito longo
    public void Validate_InvalidUsernameLength_ShouldHaveValidationError(string username, string expectedMessage)
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
              .WithErrorMessage(expectedMessage);
    }

    [Theory]
    [InlineData("user@name")] // Caractere inválido
    [InlineData("user name")] // Espaço não permitido
    [InlineData("user#name")] // Caractere inválido
    [InlineData("user%name")] // Caractere inválido
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
              .WithErrorMessage(ValidationMessages.InvalidFormat.Username);
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
              .WithErrorMessage(ValidationMessages.Required.Email);
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
              .WithErrorMessage(ValidationMessages.InvalidFormat.Email);
    }

    [Fact]
    public void Validate_EmailTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longEmail = string.Concat(Enumerable.Repeat("a", 250)) + "@example.com"; // Mais de 255 caracteres
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
              .WithErrorMessage(ValidationMessages.Length.EmailTooLong);
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
              .WithErrorMessage(ValidationMessages.Required.Password);
    }

    [Theory]
    [InlineData("1234567")] // Muito curta
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
              .WithErrorMessage(ValidationMessages.Length.PasswordTooShort);
    }

    [Theory]
    [InlineData("password123")] // Sem maiúscula
    [InlineData("PASSWORD123")] // Sem minúscula
    [InlineData("PasswordABC")] // Sem número
    [InlineData("12345678")] // Sem letras
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
              .WithErrorMessage(ValidationMessages.InvalidFormat.Password);
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
              .WithErrorMessage(ValidationMessages.Required.FirstName);
    }

    [Theory]
    [InlineData("A", ValidationMessages.Length.FirstNameTooShort)] // Muito curto
    [InlineData("ThisIsAVeryLongFirstNameThatExceedsOneHundredCharactersAndShouldFailValidationBecauseItIsTooLongForTheSystem", ValidationMessages.Length.FirstNameTooLong)] // Muito longo
    public void Validate_InvalidFirstNameLength_ShouldHaveValidationError(string firstName, string expectedMessage)
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
              .WithErrorMessage(expectedMessage);
    }

    [Theory]
    [InlineData("John123")] // Números não permitidos
    [InlineData("John@")]
    [InlineData("John-")]
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
              .WithErrorMessage(ValidationMessages.InvalidFormat.FirstName);
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
              .WithErrorMessage(ValidationMessages.Required.LastName);
    }
}
