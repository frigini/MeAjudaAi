using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Application.Validators;
using MeAjudaAi.Modules.Providers.Domain.Enums;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Validators;

public class GetProvidersRequestValidatorTests
{
    private readonly GetProvidersRequestValidator _validator;

    public GetProvidersRequestValidatorTests()
    {
        _validator = new GetProvidersRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new GetProvidersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Name = "João Silva",
            Type = (int)EProviderType.Individual,
            VerificationStatus = (int)EVerificationStatus.Verified
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidRequestWithoutOptionalFields_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new GetProvidersRequest
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_InvalidPageNumber_ShouldHaveValidationError(int pageNumber)
    {
        // Arrange
        var request = new GetProvidersRequest
        {
            PageNumber = pageNumber,
            PageSize = 10
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(200)]
    public void Validate_InvalidPageSize_ShouldHaveValidationError(int pageSize)
    {
        // Arrange
        var request = new GetProvidersRequest
        {
            PageNumber = 1,
            PageSize = pageSize
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData("a")]
    public void Validate_NameTooShort_ShouldHaveValidationError(string name)
    {
        // Arrange
        var request = new GetProvidersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Name = name
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var name = new string('a', 101); // Max is 100
        var request = new GetProvidersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Name = name
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("João")]
    [InlineData("Dr. João Silva")]
    public void Validate_ValidNames_ShouldNotHaveValidationError(string name)
    {
        // Arrange
        var request = new GetProvidersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Name = name
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData((int)EProviderType.None)]
    [InlineData((int)EProviderType.Individual)]
    [InlineData((int)EProviderType.Company)]
    [InlineData((int)EProviderType.Cooperative)]
    [InlineData((int)EProviderType.Freelancer)]
    public void Validate_ValidProviderTypes_ShouldNotHaveValidationError(int type)
    {
        // Arrange
        var request = new GetProvidersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Type = type
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(99)]
    [InlineData(999)]
    public void Validate_InvalidProviderTypes_ShouldHaveValidationError(int type)
    {
        // Arrange
        var request = new GetProvidersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Type = type
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Type.Value");
    }

    [Theory]
    [InlineData((int)EVerificationStatus.None)]
    [InlineData((int)EVerificationStatus.Pending)]
    [InlineData((int)EVerificationStatus.InProgress)]
    [InlineData((int)EVerificationStatus.Verified)]
    [InlineData((int)EVerificationStatus.Rejected)]
    [InlineData((int)EVerificationStatus.Suspended)]
    public void Validate_ValidVerificationStatuses_ShouldNotHaveValidationError(int status)
    {
        // Arrange
        var request = new GetProvidersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            VerificationStatus = status
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.VerificationStatus);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(99)]
    [InlineData(999)]
    public void Validate_InvalidVerificationStatuses_ShouldHaveValidationError(int status)
    {
        // Arrange
        var request = new GetProvidersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            VerificationStatus = status
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("VerificationStatus.Value");
    }

    [Fact]
    public void Validate_NullOptionalValues_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new GetProvidersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Name = null,
            Type = null,
            VerificationStatus = null
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new GetProvidersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Name = ""
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WhitespaceName_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new GetProvidersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Name = "   "
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}
