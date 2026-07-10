using MeAjudaAi.Shared.Authorization.Exceptions;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization.Exceptions;

public class PermissionServiceExceptionTests
{
    [Fact]
    public void Constructor_Default_SetsMessage()
    {
        // Arrange & Act
        var exception = new PermissionServiceException();

        // Assert
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        // Arrange
        var message = "Custom error message";

        // Act
        var exception = new PermissionServiceException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsMessageAndInnerException()
    {
        // Arrange
        var message = "Custom error message";
        var innerException = new Exception("Inner exception");

        // Act
        var exception = new PermissionServiceException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }
}
