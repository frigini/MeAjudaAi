using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Validators;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Validators;

public class AddQualificationCommandValidatorTests
{
    private readonly AddQualificationCommandValidator _validator;

    public AddQualificationCommandValidatorTests()
    {
        _validator = new AddQualificationCommandValidator();
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new AddQualificationCommand(
            ProviderId: Guid.NewGuid(),
            Name: "AWS Solutions Architect",
            Description: "AWS Solutions Architect Associate certification",
            IssuingOrganization: "Amazon Web Services",
            IssueDate: DateTime.UtcNow.AddYears(-1),
            ExpirationDate: DateTime.UtcNow.AddYears(2),
            DocumentNumber: "AWS-SAA-123456"
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
        var command = new AddQualificationCommand(
            ProviderId: Guid.Empty,
            Name: "AWS Solutions Architect",
            Description: null,
            IssuingOrganization: null,
            IssueDate: null,
            ExpirationDate: null,
            DocumentNumber: null
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProviderId)
            .WithErrorMessage("O ID do prestador é obrigatório.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validate_WithEmptyName_ShouldHaveValidationError(string? name)
    {
        // Arrange
        var command = new AddQualificationCommand(
            ProviderId: Guid.NewGuid(),
            Name: name!,
            Description: null,
            IssuingOrganization: null,
            IssueDate: null,
            ExpirationDate: null,
            DocumentNumber: null
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("O nome da qualificação é obrigatório.");
    }

    [Fact]
    public async Task Validate_WithNameLessThan2Characters_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddQualificationCommand(
            ProviderId: Guid.NewGuid(),
            Name: "A",
            Description: null,
            IssuingOrganization: null,
            IssueDate: null,
            ExpirationDate: null,
            DocumentNumber: null
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("O nome da qualificação deve ter pelo menos 2 caracteres.");
    }

    [Fact]
    public async Task Validate_WithNameExceeding200Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('A', 201);
        var command = new AddQualificationCommand(
            ProviderId: Guid.NewGuid(),
            Name: longName,
            Description: null,
            IssuingOrganization: null,
            IssueDate: null,
            ExpirationDate: null,
            DocumentNumber: null
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("O nome da qualificação não pode exceder 200 caracteres.");
    }

    [Fact]
    public async Task Validate_WithDescriptionExceeding1000Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longDescription = new string('A', 1001);
        var command = new AddQualificationCommand(
            ProviderId: Guid.NewGuid(),
            Name: "AWS Certification",
            Description: longDescription,
            IssuingOrganization: null,
            IssueDate: null,
            ExpirationDate: null,
            DocumentNumber: null
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("A descrição não pode exceder 1000 caracteres.");
    }

    [Fact]
    public async Task Validate_WithFutureIssueDate_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddQualificationCommand(
            ProviderId: Guid.NewGuid(),
            Name: "AWS Certification",
            Description: null,
            IssuingOrganization: null,
            IssueDate: DateTime.UtcNow.AddDays(1),
            ExpirationDate: null,
            DocumentNumber: null
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IssueDate)
            .WithErrorMessage("A data de emissão não pode ser no futuro.");
    }

    [Fact]
    public async Task Validate_WithExpirationDateBeforeIssueDate_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddQualificationCommand(
            ProviderId: Guid.NewGuid(),
            Name: "AWS Certification",
            Description: null,
            IssuingOrganization: null,
            IssueDate: DateTime.UtcNow,
            ExpirationDate: DateTime.UtcNow.AddYears(-1),
            DocumentNumber: null
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpirationDate)
            .WithErrorMessage("A data de expiração deve ser posterior à data de emissão.");
    }

    [Fact]
    public async Task Validate_WithValidIssueDateAndNoExpiration_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new AddQualificationCommand(
            ProviderId: Guid.NewGuid(),
            Name: "AWS Certification",
            Description: null,
            IssuingOrganization: null,
            IssueDate: DateTime.UtcNow.AddYears(-1),
            ExpirationDate: null,
            DocumentNumber: null
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IssueDate);
        result.ShouldNotHaveValidationErrorFor(x => x.ExpirationDate);
    }

    [Fact]
    public async Task Validate_WithValidDates_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new AddQualificationCommand(
            ProviderId: Guid.NewGuid(),
            Name: "AWS Certification",
            Description: null,
            IssuingOrganization: null,
            IssueDate: new DateTime(2023, 1, 1),
            ExpirationDate: new DateTime(2025, 1, 1),
            DocumentNumber: null
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithIssuingOrganizationExceeding200Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longOrganization = new string('A', 201);
        var command = new AddQualificationCommand(
            ProviderId: Guid.NewGuid(),
            Name: "AWS Certification",
            Description: null,
            IssuingOrganization: longOrganization,
            IssueDate: null,
            ExpirationDate: null,
            DocumentNumber: null
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IssuingOrganization)
            .WithErrorMessage("A organização emissora não pode exceder 200 caracteres.");
    }

    [Fact]
    public async Task Validate_WithIssuingOrganizationAt200Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var organization = new string('A', 200);
        var command = new AddQualificationCommand(
            ProviderId: Guid.NewGuid(),
            Name: "AWS Certification",
            Description: null,
            IssuingOrganization: organization,
            IssueDate: null,
            ExpirationDate: null,
            DocumentNumber: null
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IssuingOrganization);
    }

    [Fact]
    public async Task Validate_WithNullIssuingOrganization_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new AddQualificationCommand(
            ProviderId: Guid.NewGuid(),
            Name: "AWS Certification",
            Description: null,
            IssuingOrganization: null,
            IssueDate: null,
            ExpirationDate: null,
            DocumentNumber: null
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IssuingOrganization);
    }

    [Fact]
    public async Task Validate_WithDocumentNumberExceeding100Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longDocumentNumber = new string('1', 101);
        var command = new AddQualificationCommand(
            ProviderId: Guid.NewGuid(),
            Name: "AWS Certification",
            Description: null,
            IssuingOrganization: null,
            IssueDate: null,
            ExpirationDate: null,
            DocumentNumber: longDocumentNumber
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DocumentNumber)
            .WithErrorMessage("O número do documento não pode exceder 100 caracteres.");
    }

    [Fact]
    public async Task Validate_WithDocumentNumberAt100Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var documentNumber = new string('1', 100);
        var command = new AddQualificationCommand(
            ProviderId: Guid.NewGuid(),
            Name: "AWS Certification",
            Description: null,
            IssuingOrganization: null,
            IssueDate: null,
            ExpirationDate: null,
            DocumentNumber: documentNumber
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DocumentNumber);
    }

    [Fact]
    public async Task Validate_WithNullDocumentNumber_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new AddQualificationCommand(
            ProviderId: Guid.NewGuid(),
            Name: "AWS Certification",
            Description: null,
            IssuingOrganization: null,
            IssueDate: null,
            ExpirationDate: null,
            DocumentNumber: null
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DocumentNumber);
    }
}
