using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Validators;
using MeAjudaAi.Shared.Utilities.Constants;

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
              .WithErrorMessage(ValidationMessages.Required.FirstName);
    }

    [Theory]
    [InlineData("A")] // Muito curto
    [InlineData("ThisIsAVeryLongFirstNameThatExceedsOneHundredCharactersAndShouldFailValidationBecauseItIsTooLongForTheSystem")] // Muito longo
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
              .WithErrorMessage(ValidationMessages.Length.FirstNameTooLong);
    }

    [Theory]
    [InlineData("João123")] // Números não permitidos
    [InlineData("João@")] // Caracteres especiais não permitidos
    [InlineData("João-")] // Hífens não permitidos
    [InlineData("João_")] // Underline não permitidos
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
              .WithErrorMessage(ValidationMessages.InvalidFormat.FirstName);
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
              .WithErrorMessage(ValidationMessages.Required.LastName);
    }

    [Theory]
    [InlineData("S")] // Muito curto
    [InlineData("ThisIsAVeryLongLastNameThatExceedsOneHundredCharactersAndShouldFailValidationBecauseItIsTooLongForTheSystem")] // Muito longo
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
              .WithErrorMessage(ValidationMessages.Length.LastNameTooLong);
    }

    [Theory]
    [InlineData("Silva123")] // Números não permitidos
    [InlineData("Silva@")] // Caracteres especiais não permitidos
    [InlineData("Silva-")] // Hífens não permitidos
    [InlineData("Silva_")] // Underline não permitidos
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
              .WithErrorMessage(ValidationMessages.InvalidFormat.LastName);
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
              .WithErrorMessage(ValidationMessages.InvalidFormat.Email);
    }

    [Fact]
    public void Validate_EmailTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longEmail = string.Concat(Enumerable.Repeat("a", 250)) + "@example.com"; // Mais de 255 caracteres
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
              .WithErrorMessage(ValidationMessages.Length.EmailTooLong);
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
