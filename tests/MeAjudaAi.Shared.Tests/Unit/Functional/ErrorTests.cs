using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Shared.Tests.Unit.Functional;

[Trait("Category", "Unit")]
public class ErrorTests
{
    [Fact]
    public void BadRequest_ShouldCreateErrorWithBadRequestStatusCode()
    {
        // Arrange
        var message = "Bad request error";

        // Act
        var error = Error.BadRequest(message);

        // Assert
        error.StatusCode.Should().Be(400);
        error.Message.Should().Be(message);
    }

    [Fact]
    public void NotFound_ShouldCreateErrorWithNotFoundStatusCode()
    {
        // Arrange
        var message = "Not found error";

        // Act
        var error = Error.NotFound(message);

        // Assert
        error.StatusCode.Should().Be(404);
        error.Message.Should().Be(message);
    }

    [Fact]
    public void Unauthorized_ShouldCreateErrorWithUnauthorizedStatusCode()
    {
        // Arrange
        var message = "Unauthorized error";

        // Act
        var error = Error.Unauthorized(message);

        // Assert
        error.StatusCode.Should().Be(401);
        error.Message.Should().Be(message);
    }

    [Fact]
    public void Forbidden_ShouldCreateErrorWithForbiddenStatusCode()
    {
        // Arrange
        var message = "Forbidden error";

        // Act
        var error = Error.Forbidden(message);

        // Assert
        error.StatusCode.Should().Be(403);
        error.Message.Should().Be(message);
    }

    [Fact]
    public void Internal_ShouldCreateErrorWithInternalServerErrorStatusCode()
    {
        // Arrange
        var message = "Internal server error";

        // Act
        var error = Error.Internal(message);

        // Assert
        error.StatusCode.Should().Be(500);
        error.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_ShouldCreateErrorWithMessageAndStatusCode()
    {
        // Arrange
        var message = "Test error message";
        var statusCode = 422;

        // Act
        var error = new Error(message, statusCode);

        // Assert
        error.StatusCode.Should().Be(statusCode);
        error.Message.Should().Be(message);
    }

    [Fact]
    public void Equals_WithSameMessageAndStatusCode_ShouldReturnTrue()
    {
        // Arrange
        var error1 = Error.BadRequest("Same message");
        var error2 = Error.BadRequest("Same message");

        // Act & Assert
        error1.Equals(error2).Should().BeTrue();
        (error1 == error2).Should().BeTrue();
        (error1 != error2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentMessageOrStatusCode_ShouldReturnFalse()
    {
        // Arrange
        var error1 = Error.BadRequest("Message 1");
        var error2 = Error.BadRequest("Message 2");
        var error3 = Error.NotFound("Message 1");

        // Act & Assert
        error1.Equals(error2).Should().BeFalse();
        error1.Equals(error3).Should().BeFalse();
        (error1 == error2).Should().BeFalse();
        (error1 != error2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameMessageAndStatusCode_ShouldReturnSameHashCode()
    {
        // Arrange
        var error1 = Error.BadRequest("Same message");
        var error2 = Error.BadRequest("Same message");

        // Act & Assert
        error1.GetHashCode().Should().Be(error2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var error = Error.BadRequest("Test message");

        // Act
        var result = error.ToString();

        // Assert
        result.Should().Contain("Test message");
        result.Should().Contain("400");
    }
}