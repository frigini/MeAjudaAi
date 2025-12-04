using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Shared.Tests.Unit.Exceptions;

public class ForbiddenAccessExceptionTests
{
    #region Default Constructor Tests

    [Fact]
    public void DefaultConstructor_ShouldSetDefaultMessage()
    {
        // Act
        var exception = new ForbiddenAccessException();

        // Assert
        exception.Message.Should().Be("Access to this resource is forbidden.");
    }

    [Fact]
    public void DefaultConstructor_ShouldBeThrowable()
    {
        // Act
        Action act = () => throw new ForbiddenAccessException();

        // Assert
        act.Should().Throw<ForbiddenAccessException>()
            .WithMessage("Access to this resource is forbidden.");
    }

    #endregion

    #region Constructor With Message Tests

    [Fact]
    public void Constructor_WithCustomMessage_ShouldSetMessage()
    {
        // Arrange
        var customMessage = "You don't have permission to access this resource";

        // Act
        var exception = new ForbiddenAccessException(customMessage);

        // Assert
        exception.Message.Should().Be(customMessage);
    }

    [Fact]
    public void Constructor_WithEmptyMessage_ShouldAcceptEmptyString()
    {
        // Arrange
        var emptyMessage = "";

        // Act
        var exception = new ForbiddenAccessException(emptyMessage);

        // Assert
        exception.Message.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullMessage_ShouldHandleGracefully()
    {
        // Arrange
        string? nullMessage = null;

        // Act
        var exception = new ForbiddenAccessException(nullMessage!);

        // Assert
        exception.Message.Should().NotBeNull();
    }

    #endregion

    #region Constructor With InnerException Tests

    [Fact]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        var message = "Access denied";
        var innerException = new InvalidOperationException("Underlying error");

        // Act
        var exception = new ForbiddenAccessException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
        exception.InnerException!.Message.Should().Be("Underlying error");
    }

    [Fact]
    public void Constructor_WithNullInnerException_ShouldAcceptNull()
    {
        // Arrange
        var message = "Access denied";

        // Act
        var exception = new ForbiddenAccessException(message, null!);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void ForbiddenAccessException_ShouldInheritFromException()
    {
        // Act
        var exception = new ForbiddenAccessException();

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void ForbiddenAccessException_ShouldNotBeSystemException()
    {
        // Act
        var exception = new ForbiddenAccessException();

        // Assert
        exception.Should().NotBeOfType<SystemException>();
    }

    #endregion

    #region Throw and Catch Tests

    [Fact]
    public void Exception_ShouldBeCatchableAsException()
    {
        // Arrange
        var caught = false;

        // Act
        try
        {
            throw new ForbiddenAccessException("Test");
        }
        catch (Exception)
        {
            caught = true;
        }

        // Assert
        caught.Should().BeTrue();
    }

    [Fact]
    public void Exception_ShouldBeCatchableAsForbiddenAccessException()
    {
        // Arrange
        var caught = false;
        var expectedMessage = "Specific forbidden message";

        // Act
        try
        {
            throw new ForbiddenAccessException(expectedMessage);
        }
        catch (ForbiddenAccessException ex)
        {
            caught = true;
            ex.Message.Should().Be(expectedMessage);
        }

        // Assert
        caught.Should().BeTrue();
    }

    #endregion

    #region Real-World Scenarios

    [Fact]
    public void Scenario_UnauthorizedResourceAccess_ShouldProvideContext()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var resourceId = "RESOURCE-123";
        var message = $"User {userId} is not authorized to access resource {resourceId}";

        // Act
        var exception = new ForbiddenAccessException(message);

        // Assert
        exception.Message.Should().Contain(userId.ToString());
        exception.Message.Should().Contain(resourceId);
    }

    [Fact]
    public void Scenario_InsufficientPermissions_ShouldWrapOriginalException()
    {
        // Arrange
        var originalException = new UnauthorizedAccessException("Token expired");
        var message = "Insufficient permissions to perform this operation";

        // Act
        var exception = new ForbiddenAccessException(message, originalException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeOfType<UnauthorizedAccessException>();
    }

    #endregion
}
