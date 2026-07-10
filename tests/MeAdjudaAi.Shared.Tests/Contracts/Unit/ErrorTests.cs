using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Shared.Tests.Contracts.Unit;

[Trait("Category", "Unit")]
[Trait("Component", "Contracts")]
public class ErrorTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldDefaultTo400()
    {
        // Arrange & Act
        var error = new Error("Something went wrong");

        // Assert
        error.Message.Should().Be("Something went wrong");
        error.StatusCode.Should().Be(400);
        error.Code.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndStatusCode_ShouldStoreValues()
    {
        // Arrange & Act
        var error = new Error("Not found", 404);

        // Assert
        error.Message.Should().Be("Not found");
        error.StatusCode.Should().Be(404);
    }

    [Fact]
    public void Constructor_WithMessageStatusCodeAndCode_ShouldStoreValues()
    {
        // Arrange & Act
        var error = new Error("Validation failed", 400, "VALIDATION_ERROR");

        // Assert
        error.Code.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public void NotFound_ShouldReturn404()
    {
        // Arrange & Act
        var error = Error.NotFound("Resource not found");

        // Assert
        error.StatusCode.Should().Be(404);
        error.Message.Should().Be("Resource not found");
    }

    [Fact]
    public void BadRequest_ShouldReturn400()
    {
        // Arrange & Act
        var error = Error.BadRequest("Invalid input");

        // Assert
        error.StatusCode.Should().Be(400);
        error.Message.Should().Be("Invalid input");
    }

    [Fact]
    public void Unauthorized_ShouldReturn401()
    {
        // Arrange & Act
        var error = Error.Unauthorized("Authentication required");

        // Assert
        error.StatusCode.Should().Be(401);
        error.Message.Should().Be("Authentication required");
    }

    [Fact]
    public void Forbidden_ShouldReturn403()
    {
        // Arrange & Act
        var error = Error.Forbidden("Access denied");

        // Assert
        error.StatusCode.Should().Be(403);
        error.Message.Should().Be("Access denied");
    }

    [Fact]
    public void Internal_ShouldReturn500()
    {
        // Arrange & Act
        var error = Error.Internal("Server error");

        // Assert
        error.StatusCode.Should().Be(500);
        error.Message.Should().Be("Server error");
    }

    [Fact]
    public void Conflict_ShouldReturn409()
    {
        // Arrange & Act
        var error = Error.Conflict("Resource already exists");

        // Assert
        error.StatusCode.Should().Be(409);
        error.Message.Should().Be("Resource already exists");
    }

    [Theory]
    [InlineData("ERR_001")]
    [InlineData(null)]
    public void FactoryMethod_WithCode_ShouldStoreCode(string? code)
    {
        // Arrange & Act
        var error = Error.BadRequest("Invalid", code);

        // Assert
        error.Code.Should().Be(code);
    }

    [Fact]
    public void RecordEquality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var error1 = new Error("msg", 400, "CODE");
        var error2 = new Error("msg", 400, "CODE");

        // Act & Assert
        error1.Should().Be(error2);
    }

    [Fact]
    public void RecordEquality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var error1 = new Error("msg1", 400);
        var error2 = new Error("msg2", 400);

        // Act & Assert
        error1.Should().NotBe(error2);
    }
}
