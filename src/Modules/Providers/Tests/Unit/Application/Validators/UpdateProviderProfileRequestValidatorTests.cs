using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Application.Validators;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Validators;

public class UpdateProviderProfileRequestValidatorTests
{
    private readonly UpdateProviderProfileRequestValidator _validator;

    public UpdateProviderProfileRequestValidatorTests()
    {
        _validator = new UpdateProviderProfileRequestValidator();
    }

    [Fact]
    public async Task Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WithEmptyName_ShouldHaveValidationError(string name)
    {
        // Arrange
        var request = CreateValidRequest() with { Name = name };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Nome é obrigatório");
    }

    [Fact]
    public async Task Validate_WithNameLessThan2Characters_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with { Name = "A" };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Nome deve ter no mínimo 2 caracteres");
    }

    [Fact]
    public async Task Validate_WithNameExceeding100Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('A', 101);
        var request = CreateValidRequest() with { Name = longName };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Nome não pode exceder 100 caracteres");
    }

    [Fact]
    public async Task Validate_WithNullBusinessProfile_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with { BusinessProfile = null! };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BusinessProfile)
            .WithErrorMessage("Perfil de negócio é obrigatório");
    }

    [Fact]
    public async Task Validate_WithDescriptionExceeding500Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longDescription = new string('A', 501);
        var request = CreateValidRequest() with
        {
            BusinessProfile = CreateValidBusinessProfile() with { Description = longDescription }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BusinessProfile!.Description)
            .WithErrorMessage("Descrição não pode exceder 500 caracteres");
    }

    private static UpdateProviderProfileRequest CreateValidRequest()
    {
        return new UpdateProviderProfileRequest
        {
            Name = "Valid Provider Name",
            BusinessProfile = CreateValidBusinessProfile()
        };
    }

    private static BusinessProfileDto CreateValidBusinessProfile()
    {
        return new BusinessProfileDto(
            LegalName: "Valid Legal Name",
            FantasyName: "Valid Fantasy Name",
            Description: "Valid Description",
            ContactInfo: new ContactInfoDto(
                Email: "valid@example.com",
                PhoneNumber: "11987654321",
                Website: "https://example.com"
            ),
            PrimaryAddress: new AddressDto(
                Street: "Valid Street",
                Number: "123",
                Complement: "Apt 456",
                Neighborhood: "Valid Neighborhood",
                City: "São Paulo",
                State: "SP",
                ZipCode: "01234-567",
                Country: "Brazil"
            )
        );
    }
}
