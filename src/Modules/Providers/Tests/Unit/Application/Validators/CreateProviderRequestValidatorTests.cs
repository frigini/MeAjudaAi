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
            .WithErrorMessage("UserId is required");
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
            .WithErrorMessage("Name is required");
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
            .WithErrorMessage("Name must be at least 2 characters long");
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
            .WithErrorMessage("Name cannot exceed 100 characters");
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
            .WithErrorMessage("BusinessProfile is required");
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
            .WithErrorMessage("BusinessProfile.Description cannot exceed 500 characters");
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
            .WithErrorMessage("BusinessProfile.ContactInfo is required");
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
            .WithErrorMessage("Email is required");
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
            .WithErrorMessage("Email must be a valid email address");
    }

    [Fact]
    public async Task Validate_WithNullPrimaryAddress_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with
        {
            BusinessProfile = CreateValidBusinessProfile() with { PrimaryAddress = null! }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BusinessProfile!.PrimaryAddress)
            .WithErrorMessage("BusinessProfile.PrimaryAddress is required");
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
            .WithErrorMessage("PrimaryAddress.Street is required");
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
            .WithErrorMessage("PrimaryAddress.City is required");
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
            .WithErrorMessage("PrimaryAddress.State is required");
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
            .WithErrorMessage("PrimaryAddress.ZipCode is required");
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
            .WithErrorMessage("PrimaryAddress.Country is required");
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
