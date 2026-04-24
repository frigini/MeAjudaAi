using FluentAssertions;
using MeAjudaAi.Shared.Exceptions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Exceptions;

[Trait("Category", "Unit")]
public class ExceptionTests
{
    [Fact]
    public void BusinessRuleException_Constructor_Should_SetProperties()
    {
        // Arrange
        var ruleName = "MinAgeRule";
        var message = "User must be at least 18 years old";

        // Act
        var exception = new BusinessRuleException(ruleName, message);

        // Assert
        exception.Message.Should().Be(message);
        exception.RuleName.Should().Be(ruleName);
    }

    [Fact]
    public void NotFoundException_Constructor_Should_SetProperties()
    {
        // Arrange
        var entityName = "User";
        var entityId = "123";

        // Act
        var exception = new NotFoundException(entityName, entityId);

        // Assert
        exception.Message.Should().Be($"{entityName} with id {entityId} was not found");
        exception.EntityName.Should().Be(entityName);
        exception.EntityId.Should().Be(entityId);
    }

    [Fact]
    public void ForbiddenAccessException_Constructor_Should_SetMessage()
    {
        // Arrange
        var message = "Access denied to this resource";

        // Act
        var exception = new ForbiddenAccessException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void BadRequestException_Constructor_Should_SetMessage()
    {
        // Arrange
        var message = "Invalid request data";

        // Act
        var exception = new TestBadRequestException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void UnprocessableEntityException_Constructor_Should_SetProperties()
    {
        // Arrange
        var entityName = "Booking";
        var message = "Invalid state transition";

        // Act
        var exception = new UnprocessableEntityException(message, entityName);

        // Assert
        exception.Message.Should().Be(message);
        exception.EntityName.Should().Be(entityName);
    }

    [Fact]
    public void ConcurrencyConflictException_Constructor_Should_SetProperties()
    {
        // Arrange
        var message = "Concurrency conflict";
        var entityType = "Order";
        var aggregateId = "789";

        // Act
        var exception = new ConcurrencyConflictException(message, aggregateId, entityType);

        // Assert
        exception.Message.Should().Be(message);
        exception.EntityType.Should().Be(entityType);
        exception.AggregateId.Should().Be(aggregateId);
    }

    private class TestBadRequestException(string message) : BadRequestException(message);
}
