using MeAjudaAi.Shared.Models;

namespace MeAjudaAi.Shared.Tests.Unit.Models;

[Trait("Category", "Unit")]
[Trait("Component", "Shared")]
[Trait("Layer", "Models")]
public class ErrorResponseTests
{
    [Fact]
    public void ValidationErrorResponse_DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var response = new ValidationErrorResponse();

        // Assert
        response.StatusCode.Should().Be(400);
        response.Title.Should().Be("Validation Error");
        response.Detail.Should().NotBeNullOrEmpty();
        response.ValidationErrors.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ValidationErrorResponse_WithValidationErrors_ShouldStoreErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            ["Email"] = new[] { "Email is required", "Email format is invalid" },
            ["Name"] = new[] { "Name must be at least 3 characters" }
        };

        // Act
        var response = new ValidationErrorResponse(errors);

        // Assert
        response.ValidationErrors.Should().HaveCount(2);
        response.ValidationErrors["Email"].Should().HaveCount(2);
        response.ValidationErrors["Name"].Should().HaveCount(1);
    }

    [Fact]
    public void NotFoundErrorResponse_DefaultConstructor_ShouldHave404StatusCode()
    {
        // Act
        var response = new NotFoundErrorResponse();

        // Assert
        response.StatusCode.Should().Be(404);
        response.Title.Should().Be("Not Found");
        response.Detail.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NotFoundErrorResponse_WithResourceInfo_ShouldIncludeInDetail()
    {
        // Act
        var response = new NotFoundErrorResponse("User", "abc-123");

        // Assert
        response.StatusCode.Should().Be(404);
        response.Detail.Should().Contain("User");
        response.Detail.Should().Contain("abc-123");
    }

    [Fact]
    public void AuthenticationErrorResponse_ShouldHave401StatusCode()
    {
        // Act
        var response = new AuthenticationErrorResponse();

        // Assert
        response.StatusCode.Should().Be(401);
        response.Title.Should().Be("Unauthorized");
        response.Detail.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AuthorizationErrorResponse_ShouldHave403StatusCode()
    {
        // Act
        var response = new AuthorizationErrorResponse();

        // Assert
        response.StatusCode.Should().Be(403);
        response.Title.Should().Be("Forbidden");
        response.Detail.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void InternalServerErrorResponse_ShouldHave500StatusCode()
    {
        // Act
        var response = new InternalServerErrorResponse();

        // Assert
        response.StatusCode.Should().Be(500);
        response.Title.Should().Be("Internal Server Error");
        response.Detail.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RateLimitErrorResponse_ShouldHave429StatusCode()
    {
        // Act
        var response = new RateLimitErrorResponse();

        // Assert
        response.StatusCode.Should().Be(429);
        response.Title.Should().Be("Too Many Requests");
        response.Detail.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ValidationErrorResponse_WithEmptyErrors_ShouldHaveEmptyDictionary()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>();

        // Act
        var response = new ValidationErrorResponse(errors);

        // Assert
        response.ValidationErrors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Provider")]
    [InlineData("Document")]
    [InlineData("Service")]
    [InlineData("Category")]
    public void NotFoundErrorResponse_WithDifferentResourceTypes_ShouldIncludeResourceTypeInDetail(string resourceType)
    {
        // Act
        var response = new NotFoundErrorResponse(resourceType, "123");

        // Assert
        response.Detail.Should().Contain(resourceType);
    }
}

