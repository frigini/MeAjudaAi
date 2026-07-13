using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Shared.Tests.Unit.Exceptions;

[Trait("Category", "Unit")]
[Trait("Component", "Exceptions")]
public class BadRequestExceptionTests
{
    private sealed class TestBadRequestException(string message) : BadRequestException(message);

    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Invalid request data";

        // Act
        var ex = new TestBadRequestException(message);

        // Assert
        ex.Message.Should().Be(message);
    }

    [Fact]
    public void ShouldInheritFromException()
    {
        // Arrange & Act
        var ex = new TestBadRequestException("msg");

        // Assert
        ex.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void ShouldBeAbstract()
    {
        // Arrange & Act & Assert
        typeof(BadRequestException).IsAbstract.Should().BeTrue();
    }

    [Fact]
    public void ShouldBeCatchableAsException()
    {
        // Arrange & Act & Assert
        var act = () => ThrowHelper();
        act.Should().Throw<Exception>()
            .Which.Message.Should().Be("bad request");
    }

    [Fact]
    public void Constructor_WithEmptyMessage_ShouldAllowEmpty()
    {
        // Arrange & Act
        var ex = new TestBadRequestException(string.Empty);

        // Assert
        ex.Message.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullMessage_ShouldUseDefaultMessage()
    {
        // Arrange & Act
        var ex = new TestBadRequestException(null!);

        // Assert - Exception.Message never returns null, uses default message
        ex.Message.Should().NotBeNull();
    }

    private static void ThrowHelper()
    {
        throw new TestBadRequestException("bad request");
    }
}
