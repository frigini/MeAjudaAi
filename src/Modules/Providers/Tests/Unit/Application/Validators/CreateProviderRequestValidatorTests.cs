using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Application.Validators;
using MeAjudaAi.Modules.Providers.Domain.Enums;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Validators;

public class CreateProviderRequestValidatorTests
{
    private readonly CreateProviderRequestValidator _validator;

    public CreateProviderRequestValidatorTests()
    {
        _validator = new CreateProviderRequestValidator();
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

    [Fact]
    public async Task Validate_WithEmptyGuidUserId_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with { UserId = Guid.Empty };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId é obrigatório");
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
    public async Task Validate_WithInvalidProviderType_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with { Type = (EProviderType)999 };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public async Task Validate_WithNullBusinessProfile_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with { BusinessProfile = null };

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

    [Fact]
    public async Task Validate_WithNullContactInfo_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with
        {
            BusinessProfile = CreateValidBusinessProfile() with { ContactInfo = null! }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BusinessProfile!.ContactInfo)
            .WithErrorMessage("Informações de contato são obrigatórias");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WithEmptyEmail_ShouldHaveValidationError(string email)
    {
        // Arrange
        var request = CreateValidRequest() with
        {
            BusinessProfile = CreateValidBusinessProfile() with
            {
                ContactInfo = new ContactInfoDto(email, "11987654321", "https://example.com")
            }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BusinessProfile!.ContactInfo!.Email)
            .WithErrorMessage("E-mail é obrigatório");
    }

    [Fact]
    public async Task Validate_WithInvalidEmailFormat_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with
        {
            BusinessProfile = CreateValidBusinessProfile() with
            {
                ContactInfo = new ContactInfoDto("invalid-email", "11987654321", "https://example.com")
            }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BusinessProfile!.ContactInfo!.Email)
            .WithErrorMessage("E-mail deve ser um endereço válido");
    }

    [Fact]
    public async Task Validate_WithNullPrimaryAddress_WhenShowAddressToClientIsTrue_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with
        {
            BusinessProfile = CreateValidBusinessProfile() with { ShowAddressToClient = true, PrimaryAddress = null! }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BusinessProfile!.PrimaryAddress)
            .WithErrorMessage("Endereço principal é obrigatório");
    }

    [Fact]
    public async Task Validate_WithNullPrimaryAddress_WhenShowAddressToClientIsFalse_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with
        {
            BusinessProfile = CreateValidBusinessProfile() with { ShowAddressToClient = false, PrimaryAddress = null! }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BusinessProfile!.PrimaryAddress);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WithEmptyStreet_ShouldHaveValidationError(string street)
    {
        // Arrange
        var request = CreateValidRequest() with
        {
            BusinessProfile = CreateValidBusinessProfile() with
            {
                PrimaryAddress = CreateValidAddress() with { Street = street }
            }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BusinessProfile!.PrimaryAddress!.Street)
            .WithErrorMessage("Rua é obrigatória");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WithEmptyCity_ShouldHaveValidationError(string city)
    {
        // Arrange
        var request = CreateValidRequest() with
        {
            BusinessProfile = CreateValidBusinessProfile() with
            {
                PrimaryAddress = CreateValidAddress() with { City = city }
            }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BusinessProfile!.PrimaryAddress!.City)
            .WithErrorMessage("Cidade é obrigatória");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WithEmptyState_ShouldHaveValidationError(string state)
    {
        // Arrange
        var request = CreateValidRequest() with
        {
            BusinessProfile = CreateValidBusinessProfile() with
            {
                PrimaryAddress = CreateValidAddress() with { State = state }
            }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BusinessProfile!.PrimaryAddress!.State)
            .WithErrorMessage("Estado é obrigatório");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WithEmptyZipCode_ShouldHaveValidationError(string zipCode)
    {
        // Arrange
        var request = CreateValidRequest() with
        {
            BusinessProfile = CreateValidBusinessProfile() with
            {
                PrimaryAddress = CreateValidAddress() with { ZipCode = zipCode }
            }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BusinessProfile!.PrimaryAddress!.ZipCode)
            .WithErrorMessage("CEP é obrigatório");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WithEmptyCountry_ShouldHaveValidationError(string country)
    {
        // Arrange
        var request = CreateValidRequest() with
        {
            BusinessProfile = CreateValidBusinessProfile() with
            {
                PrimaryAddress = CreateValidAddress() with { Country = country }
            }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BusinessProfile!.PrimaryAddress!.Country)
            .WithErrorMessage("País é obrigatório");
    }

    private static CreateProviderRequest CreateValidRequest()
    {
        return new CreateProviderRequest
        {
            UserId = Guid.NewGuid(),
            Name = "Valid Provider Name",
            Type = EProviderType.Individual,
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
            PrimaryAddress: CreateValidAddress()
        );
    }

    private static AddressDto CreateValidAddress()
    {
        return new AddressDto(
            Street: "Valid Street",
            Number: "123",
            Complement: "Apt 456",
            Neighborhood: "Valid Neighborhood",
            City: "São Paulo",
            State: "SP",
            ZipCode: "01234-567",
            Country: "Brazil"
        );
    }
}
