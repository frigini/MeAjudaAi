using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Validators;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Validators;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class UpdateUserProfileRequestValidatorTests
{
    private readonly UpdateUserProfileRequestValidator _validator;

    public UpdateUserProfileRequestValidatorTests()
    {
        _validator = new UpdateUserProfileRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new UpdateUserProfileRequest
        {
            FirstName = "João",
            LastName = "Silva",
            Email = "joao.silva@example.com"
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
    public void Validate_EmptyFirstName_ShouldHaveValidationError(string? firstName)
    {
        // Arrange
        var request = new UpdateUserProfileRequest
        {
            FirstName = firstName ?? string.Empty,
            LastName = "Silva",
            Email = "test@example.com"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("Nome é obrigatório");
    }

    [Theory]
    [InlineData("A")] // Too short
    [InlineData("ThisIsAVeryLongFirstNameThatExceedsOneHundredCharactersAndShouldFailValidationBecauseItIsTooLongForTheSystem")] // Too long
    public void Validate_InvalidFirstNameLength_ShouldHaveValidationError(string firstName)
    {
        // Arrange
        var request = new UpdateUserProfileRequest
        {
            FirstName = firstName,
            LastName = "Silva",
            Email = "test@example.com"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("Nome deve ter entre 2 e 100 caracteres");
    }

    [Theory]
    [InlineData("João123")] // Numbers not allowed
    [InlineData("João@")] // Special characters not allowed
    [InlineData("João-")] // Hyphens not allowed
    [InlineData("João_")] // Underscores not allowed
    public void Validate_InvalidFirstNameFormat_ShouldHaveValidationError(string firstName)
    {
        // Arrange
        var request = new UpdateUserProfileRequest
        {
            FirstName = firstName,
            LastName = "Silva",
            Email = "test@example.com"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("Nome deve conter apenas letras e espaços");
    }

    [Theory]
    [InlineData("João")]
    [InlineData("Maria José")]
    [InlineData("José")]
    [InlineData("François")]
    [InlineData("Ana Beatriz")]
    [InlineData("José Carlos")]
    public void Validate_ValidFirstNames_ShouldNotHaveValidationError(string firstName)
    {
        // Arrange
        var request = new UpdateUserProfileRequest
        {
            FirstName = firstName,
            LastName = "Silva",
            Email = "test@example.com"
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
        var request = new UpdateUserProfileRequest
        {
            FirstName = "João",
            LastName = lastName ?? string.Empty,
            Email = "test@example.com"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
              .WithErrorMessage("Sobrenome é obrigatório");
    }

    [Theory]
    [InlineData("S")] // Too short
    [InlineData("ThisIsAVeryLongLastNameThatExceedsOneHundredCharactersAndShouldFailValidationBecauseItIsTooLongForTheSystem")] // Too long
    public void Validate_InvalidLastNameLength_ShouldHaveValidationError(string lastName)
    {
        // Arrange
        var request = new UpdateUserProfileRequest
        {
            FirstName = "João",
            LastName = lastName,
            Email = "test@example.com"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
              .WithErrorMessage("Sobrenome deve ter entre 2 e 100 caracteres");
    }

    [Theory]
    [InlineData("Silva123")] // Numbers not allowed
    [InlineData("Silva@")] // Special characters not allowed
    [InlineData("Silva-")] // Hyphens not allowed
    [InlineData("Silva_")] // Underscores not allowed
    public void Validate_InvalidLastNameFormat_ShouldHaveValidationError(string lastName)
    {
        // Arrange
        var request = new UpdateUserProfileRequest
        {
            FirstName = "João",
            LastName = lastName,
            Email = "test@example.com"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
              .WithErrorMessage("Sobrenome deve conter apenas letras e espaços");
    }

    [Theory]
    [InlineData("Silva")]
    [InlineData("Silva Santos")]
    [InlineData("Oliveira")]
    [InlineData("Costa")]
    [InlineData("de Oliveira")]
    [InlineData("Van Der Berg")]
    public void Validate_ValidLastNames_ShouldNotHaveValidationError(string lastName)
    {
        // Arrange
        var request = new UpdateUserProfileRequest
        {
            FirstName = "João",
            LastName = lastName,
            Email = "test@example.com"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Validate_EmptyEmail_ShouldHaveValidationError(string? email)
    {
        // Arrange
        var request = new UpdateUserProfileRequest
        {
            FirstName = "João",
            LastName = "Silva",
            Email = email ?? string.Empty
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email é obrigatório");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test.example.com")]
    public void Validate_InvalidEmailFormat_ShouldHaveValidationError(string email)
    {
        // Arrange
        var request = new UpdateUserProfileRequest
        {
            FirstName = "João",
            LastName = "Silva",
            Email = email
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email deve ter um formato válido");
    }

    [Fact]
    public void Validate_EmailTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longEmail = string.Concat(Enumerable.Repeat("a", 250)) + "@example.com"; // Over 255 characters
        var request = new UpdateUserProfileRequest
        {
            FirstName = "João",
            LastName = "Silva",
            Email = longEmail
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email não pode ter mais de 255 caracteres");
    }

    [Theory]
    [InlineData("joao@example.com")]
    [InlineData("joao.silva@example.com")]
    [InlineData("joao+test@example.com")]
    [InlineData("joao.silva+tag@domain.co.uk")]
    [InlineData("user@domain-with-hyphens.com")]
    public void Validate_ValidEmails_ShouldNotHaveValidationError(string email)
    {
        // Arrange
        var request = new UpdateUserProfileRequest
        {
            FirstName = "João",
            LastName = "Silva",
            Email = email
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_AllFieldsInvalid_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var request = new UpdateUserProfileRequest
        {
            FirstName = "",
            LastName = "S",
            Email = "invalid-email"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}