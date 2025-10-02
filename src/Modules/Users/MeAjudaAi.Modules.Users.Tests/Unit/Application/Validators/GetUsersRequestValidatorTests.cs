using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Validators;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Validators;

public class GetUsersRequestValidatorTests
{
    private readonly GetUsersRequestValidator _validator;

    public GetUsersRequestValidatorTests()
    {
        _validator = new GetUsersRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new GetUsersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = "john"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidRequestWithoutSearchTerm_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new GetUsersRequest
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
        var request = new GetUsersRequest
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
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void Validate_ValidPageNumbers_ShouldNotHaveValidationError(int pageNumber)
    {
        // Arrange
        var request = new GetUsersRequest
        {
            PageNumber = pageNumber,
            PageSize = 10
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_InvalidPageSize_ShouldHaveValidationError(int pageSize)
    {
        // Arrange
        var request = new GetUsersRequest
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
    [InlineData(101)]
    [InlineData(200)]
    [InlineData(1000)]
    public void Validate_PageSizeTooLarge_ShouldHaveValidationError(int pageSize)
    {
        // Arrange
        var request = new GetUsersRequest
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
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_ValidPageSizes_ShouldNotHaveValidationError(int pageSize)
    {
        // Arrange
        var request = new GetUsersRequest
        {
            PageNumber = 1,
            PageSize = pageSize
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyOrWhitespaceSearchTerm_ShouldNotHaveValidationError(string searchTerm)
    {
        // Arrange
        var request = new GetUsersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = searchTerm
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SearchTerm);
    }

    [Theory]
    [InlineData("a")]
    public void Validate_SearchTermTooShort_ShouldHaveValidationError(string searchTerm)
    {
        // Arrange
        var request = new GetUsersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = searchTerm
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SearchTerm);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("abc")]
    [InlineData("john")]
    [InlineData("user123")]
    [InlineData("search term")]
    public void Validate_ValidSearchTerms_ShouldNotHaveValidationError(string searchTerm)
    {
        // Arrange
        var request = new GetUsersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = searchTerm
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SearchTerm);
    }

    [Fact]
    public void Validate_SearchTermExactlyMaxLength_ShouldNotHaveValidationError()
    {
        // Arrange
        var searchTerm = new string('a', 50); // Tamanho máximo é 50
        var request = new GetUsersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = searchTerm
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SearchTerm);
    }

    [Fact]
    public void Validate_SearchTermTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var searchTerm = new string('a', 51); // Tamanho máximo é 50
        var request = new GetUsersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = searchTerm
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SearchTerm);
    }
}