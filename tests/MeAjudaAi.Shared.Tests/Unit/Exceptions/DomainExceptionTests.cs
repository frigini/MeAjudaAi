using FluentAssertions;
using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Shared.Tests.Unit.Exceptions;

/// <summary>
/// Testes para DomainException - classe base abstrata para exceções de domínio
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Exceptions")]
public class DomainExceptionTests
{
    // Concrete implementation for testing abstract class
    private class TestDomainException : DomainException
    {
        public TestDomainException(string message) : base(message) { }
        public TestDomainException(string message, Exception innerException) 
            : base(message, innerException) { }
    }

    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        const string expectedMessage = "Test domain error";

        // Act
        var exception = new TestDomainException(expectedMessage);

        // Assert
        exception.Message.Should().Be(expectedMessage);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        const string expectedMessage = "Domain error occurred";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new TestDomainException(expectedMessage, innerException);

        // Assert
        exception.Message.Should().Be(expectedMessage);
        exception.InnerException.Should().BeSameAs(innerException);
    }

    [Fact]
    public void DomainException_ShouldInheritFromException()
    {
        // Act
        var exception = new TestDomainException("Test");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void Constructor_WithEmptyMessage_ShouldAccept()
    {
        // Act
        var exception = new TestDomainException(string.Empty);

        // Assert
        exception.Message.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullInnerException_ShouldNotThrow()
    {
        // Act
        var exception = new TestDomainException("Test", null!);

        // Assert
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void InnerException_ShouldPreserveStackTrace()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner");

        // Act
        var exception = new TestDomainException("Outer", innerException);

        // Assert
        exception.InnerException.Should().NotBeNull();
        exception.InnerException!.Message.Should().Be("Inner");
    }

    [Fact]
    public void Constructor_WithLongMessage_ShouldPreserveFullMessage()
    {
        // Arrange
        var longMessage = new string('A', 10000);

        // Act
        var exception = new TestDomainException(longMessage);

        // Assert
        exception.Message.Should().Be(longMessage);
        exception.Message.Length.Should().Be(10000);
    }

    [Fact]
    public void Constructor_WithSpecialCharacters_ShouldPreserveMessage()
    {
        // Arrange
        const string messageWithSpecialChars = "Error: <tag> & \"quotes\" 'apostrophe' \n\r\t";

        // Act
        var exception = new TestDomainException(messageWithSpecialChars);

        // Assert
        exception.Message.Should().Be(messageWithSpecialChars);
    }

    [Fact]
    public void Constructor_WithInnerExceptionChain_ShouldPreserveChain()
    {
        // Arrange
        var innermost = new ArgumentException("Innermost");
        var middle = new InvalidOperationException("Middle", innermost);

        // Act
        var exception = new TestDomainException("Outermost", middle);

        // Assert
        exception.InnerException.Should().BeSameAs(middle);
        exception.InnerException!.InnerException.Should().BeSameAs(innermost);
    }

    [Fact]
    public void ToString_ShouldIncludeMessage()
    {
        // Arrange
        const string message = "Domain validation failed";
        var exception = new TestDomainException(message);

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain(message);
        result.Should().Contain(nameof(TestDomainException));
    }
}
