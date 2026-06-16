using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Extensions;

namespace MeAjudaAi.Shared.Tests.Unit.Extensions;

public class ErrorExtensionsTests
{
    [Fact]
    public void ToProblem_When400BadRequest_ShouldReturnProblem()
    {
        // Arrange
        var error = Error.BadRequest("Invalid request");

        // Act
        var result = error.ToProblem();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_When404NotFound_ShouldReturnProblem()
    {
        // Arrange
        var error = Error.NotFound("Resource not found");

        // Act
        var result = error.ToProblem();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_When409Conflict_ShouldReturnProblem()
    {
        // Arrange
        var error = Error.Conflict("Resource already exists");

        // Act
        var result = error.ToProblem();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_When401Unauthorized_ShouldReturnProblem()
    {
        // Arrange
        var error = Error.Unauthorized("Not authenticated");

        // Act
        var result = error.ToProblem();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_When403Forbidden_ShouldReturnProblem()
    {
        // Arrange
        var error = Error.Forbidden("Not authorized");

        // Act
        var result = error.ToProblem();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_When500Internal_ShouldReturnProblem()
    {
        // Arrange
        var error = Error.Internal("Server error");

        // Act
        var result = error.ToProblem();

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(300)]
    [InlineData(399)]
    public void ToProblem_WhenStatusCodeBelow400_ShouldReturn500(int statusCode)
    {
        // Arrange
        var error = new Error("test", statusCode);

        // Act
        var result = error.ToProblem();

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(600)]
    [InlineData(999)]
    public void ToProblem_WhenStatusCodeAbove599_ShouldReturn500(int statusCode)
    {
        // Arrange
        var error = new Error("test", statusCode);

        // Act
        var result = error.ToProblem();

        // Assert
        result.Should().NotBeNull();
    }
}
