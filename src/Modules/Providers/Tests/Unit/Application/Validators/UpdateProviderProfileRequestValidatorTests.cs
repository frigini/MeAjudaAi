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
    public async Task Validate_WithNullBusinessProfile_ShouldHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with { BusinessProfile = null! };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BusinessProfile)
            .WithErrorMessage("BusinessProfile is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WithEmptyDescription_ShouldHaveValidationError(string description)
    {
        // Arrange
        var request = CreateValidRequest() with
        {
            BusinessProfile = CreateValidBusinessProfile() with { Description = description }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BusinessProfile!.Description)
            .WithErrorMessage("BusinessProfile.Description is required");
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
