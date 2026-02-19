using FluentAssertions;
using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Application.Validators;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Validators;

public class AddDocumentRequestValidatorTests
{
    private readonly AddDocumentRequestValidator _validator;

    public AddDocumentRequestValidatorTests()
    {
        _validator = new AddDocumentRequestValidator();
    }

    [Fact]
    public async Task Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new AddDocumentRequest("ABC123456", EDocumentType.CPF);

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Validate_WithEmptyNumber_ShouldHaveValidationError(string? number)
    {
        // Arrange
#pragma warning disable CS8604 // Possible null reference argument - intentional for test
        var request = new AddDocumentRequest(number, EDocumentType.CPF);
#pragma warning restore CS8604

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Number)
            .WithErrorMessage("Número do documento é obrigatório");
    }

    [Fact]
    public async Task Validate_WithNumberLessThan3Characters_ShouldHaveValidationError()
    {
        // Arrange
        var request = new AddDocumentRequest("AB", EDocumentType.CPF);

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Number)
            .WithErrorMessage("Número do documento deve ter pelo menos 3 caracteres");
    }

    [Fact]
    public async Task Validate_WithNumberExceeding50Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longNumber = new string('A', 51);
        var request = new AddDocumentRequest(longNumber, EDocumentType.CPF);

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Number)
            .WithErrorMessage("Número do documento não pode exceder 50 caracteres");
    }

    [Theory]
    [InlineData("ABC@123")]
    [InlineData("ABC#456")]
    [InlineData("ABC 123")]
    [InlineData("ABC/123")]
    public async Task Validate_WithInvalidCharactersInNumber_ShouldHaveValidationError(string number)
    {
        // Arrange
        var request = new AddDocumentRequest(number, EDocumentType.CPF);

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Number)
            .WithErrorMessage("Número do documento deve conter apenas letras, números, hífens e pontos");
    }

    [Theory]
    [InlineData("ABC123")]
    [InlineData("123-456")]
    [InlineData("AB.CD.123")]
    [InlineData("A1B2C3")]
    [InlineData("123.456.789-00")]
    public async Task Validate_WithValidNumberFormats_ShouldNotHaveValidationErrors(string number)
    {
        // Arrange
        var request = new AddDocumentRequest(number, EDocumentType.CPF);

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Number);
    }

    [Fact]
    public async Task Validate_WithInvalidDocumentType_ShouldHaveValidationError()
    {
        // Arrange
        var request = new AddDocumentRequest("ABC123456", (EDocumentType)999);

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DocumentType)
            .WithErrorMessage("DocumentType deve ser um tipo de documento válido. Valores válidos: None, CPF, CNPJ, RG, CNH, Passport, Other");
    }
}
