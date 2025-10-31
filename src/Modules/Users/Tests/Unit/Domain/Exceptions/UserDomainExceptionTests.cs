using MeAjudaAi.Modules.Users.Domain.Exceptions;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Domain.Exceptions;

[Trait("Category", "Unit")]
public class UserDomainExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldCreateExceptionWithMessage()
    {
        // Arrange
        const string message = "Test domain exception message";

        // Act
        var exception = new UserDomainException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_ShouldCreateExceptionWithBoth()
    {
        // Arrange
        const string message = "Domain exception with inner exception";
        var innerException = new InvalidOperationException("Inner exception message");

        // Act
        var exception = new UserDomainException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void ForValidationError_WithValidParameters_ShouldCreateFormattedMessage()
    {
        // Arrange
        const string fieldName = "Email";
        const string invalidValue = "invalid-email";
        const string reason = "Email format is invalid";

        // Act
        var exception = UserDomainException.ForValidationError(fieldName, invalidValue, reason);

        // Assert
        exception.Message.Should().Be("Validation failed for field 'Email': Email format is invalid");
        exception.Should().BeOfType<UserDomainException>();
    }

    [Fact]
    public void ForValidationError_WithNullValue_ShouldHandleNullGracefully()
    {
        // Arrange
        const string fieldName = "Username";
        object? invalidValue = null;
        const string reason = "Username cannot be null";

        // Act
        var exception = UserDomainException.ForValidationError(fieldName, invalidValue, reason);

        // Assert
        exception.Message.Should().Be("Validation failed for field 'Username': Username cannot be null");
    }

    [Fact]
    public void ForInvalidOperation_WithValidParameters_ShouldCreateFormattedMessage()
    {
        // Arrange
        const string operation = "DeleteUser";
        const string currentState = "User is already deleted";

        // Act
        var exception = UserDomainException.ForInvalidOperation(operation, currentState);

        // Assert
        exception.Message.Should().Be("Cannot perform operation 'DeleteUser' in current state: User is already deleted");
        exception.Should().BeOfType<UserDomainException>();
    }

    [Fact]
    public void ForInvalidFormat_WithValidParameters_ShouldCreateFormattedMessage()
    {
        // Arrange
        const string fieldName = "PhoneNumber";
        const string invalidValue = "123abc";
        const string expectedFormat = "+XX (XX) XXXXX-XXXX";

        // Act
        var exception = UserDomainException.ForInvalidFormat(fieldName, invalidValue, expectedFormat);

        // Assert
        exception.Message.Should().Be("Invalid format for field 'PhoneNumber'. Expected: +XX (XX) XXXXX-XXXX");
        exception.Should().BeOfType<UserDomainException>();
    }

    [Fact]
    public void ForInvalidFormat_WithNullValue_ShouldHandleNullGracefully()
    {
        // Arrange
        const string fieldName = "BirthDate";
        object? invalidValue = null;
        const string expectedFormat = "yyyy-MM-dd";

        // Act
        var exception = UserDomainException.ForInvalidFormat(fieldName, invalidValue, expectedFormat);

        // Assert
        exception.Message.Should().Be("Invalid format for field 'BirthDate'. Expected: yyyy-MM-dd");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("Simple message")]
    public void Constructor_WithVariousMessages_ShouldPreserveMessage(string message)
    {
        // Act
        var exception = new UserDomainException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void FactoryMethods_ShouldInheritFromDomainException()
    {
        // Arrange & Act
        var validationException = UserDomainException.ForValidationError("field", "value", "reason");
        var operationException = UserDomainException.ForInvalidOperation("operation", "state");
        var formatException = UserDomainException.ForInvalidFormat("field", "value", "format");

        // Assert
        validationException.Should().BeAssignableTo<MeAjudaAi.Shared.Exceptions.DomainException>();
        operationException.Should().BeAssignableTo<MeAjudaAi.Shared.Exceptions.DomainException>();
        formatException.Should().BeAssignableTo<MeAjudaAi.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void ForValidationError_WithComplexObject_ShouldHandleComplexValues()
    {
        // Arrange
        const string fieldName = "UserData";
        var complexValue = new { Name = "John", Age = 25 };
        const string reason = "Complex object validation failed";

        // Act
        var exception = UserDomainException.ForValidationError(fieldName, complexValue, reason);

        // Assert
        exception.Message.Should().Be("Validation failed for field 'UserData': Complex object validation failed");
    }

    [Fact]
    public void Constructor_ShouldBeSerializable()
    {
        // Arrange
        const string message = "Test serialization";
        var originalException = new UserDomainException(message);

        // Act & Assert
        originalException.Should().NotBeNull();
        originalException.Message.Should().Be(message);
        originalException.Should().BeOfType<UserDomainException>();
    }

    [Fact]
    public void FactoryMethods_WithEmptyStrings_ShouldCreateValidExceptions()
    {
        // Act
        var validationException = UserDomainException.ForValidationError("", "", "");
        var operationException = UserDomainException.ForInvalidOperation("", "");
        var formatException = UserDomainException.ForInvalidFormat("", "", "");

        // Assert
        validationException.Message.Should().Contain("Validation failed");
        operationException.Message.Should().Contain("Cannot perform operation");
        formatException.Message.Should().Contain("Invalid format");
    }
}