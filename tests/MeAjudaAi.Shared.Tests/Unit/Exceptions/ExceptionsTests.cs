using FluentAssertions;
using FluentValidation.Results;
using MeAjudaAi.Shared.Exceptions;
using ValidationException = MeAjudaAi.Shared.Exceptions.ValidationException;

namespace MeAjudaAi.Shared.Tests.Unit.Exceptions;

public class ExceptionsTests
{
    [Fact]
    public void BusinessRuleException_ShouldStoreRuleNameAndMessage()
    {
        // Arrange
        var ruleName = "ProviderMustBeActive";
        var message = "Provider must be active to perform this operation";

        // Act
        var exception = new BusinessRuleException(ruleName, message);

        // Assert
        exception.RuleName.Should().Be(ruleName);
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void BusinessRuleException_ShouldInheritFromDomainException()
    {
        // Arrange & Act
        var exception = new BusinessRuleException("TestRule", "Test message");

        // Assert
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void NotFoundException_ShouldFormatMessage()
    {
        // Arrange
        var targetEntity = "Provider";
        var targetId = Guid.NewGuid();
        var expectedMsg = $"{targetEntity} with id {targetId} was not found";

        // Act
        var exception = new NotFoundException(targetEntity, targetId);

        // Assert
        exception.EntityName.Should().Be(targetEntity);
        exception.EntityId.Should().Be(targetId);
        exception.Message.Should().Be(expectedMsg);
    }

    [Fact]
    public void NotFoundException_ShouldInheritFromDomainException()
    {
        // Arrange & Act
        var exception = new NotFoundException("User", 123);

        // Assert
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void NotFoundException_WithIntegerId_ShouldWork()
    {
        // Arrange
        var entityName = "Order";
        var entityId = 42;

        // Act
        var exception = new NotFoundException(entityName, entityId);

        // Assert
        exception.EntityName.Should().Be(entityName);
        exception.EntityId.Should().Be(entityId);
        exception.Message.Should().Contain("42");
    }

    [Fact]
    public void NotFoundException_WithStringId_ShouldWork()
    {
        // Arrange
        var entityName = "Document";
        var entityId = "DOC-123";

        // Act
        var exception = new NotFoundException(entityName, entityId);

        // Assert
        exception.EntityName.Should().Be(entityName);
        exception.EntityId.Should().Be(entityId);
        exception.Message.Should().Contain("DOC-123");
    }

    [Fact]
    public void ForbiddenAccessException_DefaultConstructor_ShouldUseDefaultMessage()
    {
        // Act
        var exception = new ForbiddenAccessException();

        // Assert
        exception.Message.Should().Be("Access to this resource is forbidden.");
    }

    [Fact]
    public void ForbiddenAccessException_WithMessage_ShouldStoreMessage()
    {
        // Arrange
        var message = "You don't have permission to access this resource";

        // Act
        var exception = new ForbiddenAccessException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void ForbiddenAccessException_WithInnerException_ShouldStoreInnerException()
    {
        // Arrange
        var message = "Access denied";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new ForbiddenAccessException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void ForbiddenAccessException_ShouldInheritFromException()
    {
        // Arrange & Act
        var exception = new ForbiddenAccessException();

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void ValidationException_ShouldStoreErrors()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Email", "Email is invalid")
        };

        // Act
        var exception = new ValidationException(failures);

        // Assert
        exception.Errors.Should().BeEquivalentTo(failures);
    }

    [Fact]
    public void ValidationException_ShouldHaveDefaultMessage()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Field", "Error message")
        };

        // Act
        var exception = new ValidationException(failures);

        // Assert
        exception.Message.Should().Be("One or more validation failures have occurred.");
    }

    [Fact]
    public void ValidationException_DefaultConstructor_ShouldHaveEmptyErrors()
    {
        // Act
        var exception = new ValidationException();

        // Assert
        exception.Errors.Should().BeEmpty();
        exception.Message.Should().Be("One or more validation failures have occurred.");
    }
}
