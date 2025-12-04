using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Shared.Tests.Unit.Exceptions;

public class BusinessRuleExceptionTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithRuleNameAndMessage_ShouldSetProperties()
    {
        // Arrange
        var ruleName = "MaximumOrderValue";
        var message = "Order value cannot exceed $10,000";

        // Act
        var exception = new BusinessRuleException(ruleName, message);

        // Assert
        exception.RuleName.Should().Be(ruleName);
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithDifferentRules_ShouldMaintainIndependence()
    {
        // Arrange
        var rule1 = "MinimumAge";
        var message1 = "User must be at least 18 years old";
        var rule2 = "ValidEmail";
        var message2 = "Email must be in valid format";

        // Act
        var exception1 = new BusinessRuleException(rule1, message1);
        var exception2 = new BusinessRuleException(rule2, message2);

        // Assert
        exception1.RuleName.Should().Be(rule1);
        exception1.Message.Should().Be(message1);
        exception2.RuleName.Should().Be(rule2);
        exception2.Message.Should().Be(message2);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void BusinessRuleException_ShouldInheritFromDomainException()
    {
        // Arrange & Act
        var exception = new BusinessRuleException("TestRule", "Test message");

        // Assert
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void BusinessRuleException_ShouldBeThrowable()
    {
        // Arrange
        var ruleName = "UniqueUsername";
        var message = "Username already exists";

        // Act
        Action act = () => throw new BusinessRuleException(ruleName, message);

        // Assert
        act.Should().Throw<BusinessRuleException>()
            .WithMessage(message)
            .And.RuleName.Should().Be(ruleName);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void RuleName_ShouldBeReadOnly()
    {
        // Arrange
        var exception = new BusinessRuleException("TestRule", "Test message");

        // Assert
        var property = typeof(BusinessRuleException).GetProperty(nameof(BusinessRuleException.RuleName));
        property!.CanWrite.Should().BeFalse();
    }

    [Fact]
    public void RuleName_ShouldRetainOriginalValue()
    {
        // Arrange
        var originalRuleName = "InventoryAvailability";
        var exception = new BusinessRuleException(originalRuleName, "Product out of stock");

        // Act
        var retrievedRuleName = exception.RuleName;

        // Assert
        retrievedRuleName.Should().Be(originalRuleName);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_WithEmptyRuleName_ShouldAccept()
    {
        // Arrange
        var emptyRuleName = "";
        var message = "Some business rule violation";

        // Act
        var exception = new BusinessRuleException(emptyRuleName, message);

        // Assert
        exception.RuleName.Should().BeEmpty();
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithEmptyMessage_ShouldAccept()
    {
        // Arrange
        var ruleName = "SomeRule";
        var emptyMessage = "";

        // Act
        var exception = new BusinessRuleException(ruleName, emptyMessage);

        // Assert
        exception.RuleName.Should().Be(ruleName);
        exception.Message.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullRuleName_ShouldAcceptNull()
    {
        // Arrange
        string? nullRuleName = null;
        var message = "Rule violation";

        // Act
        var exception = new BusinessRuleException(nullRuleName!, message);

        // Assert
        exception.RuleName.Should().BeNull();
    }

    #endregion

    #region Real-World Business Rules

    [Fact]
    public void BusinessRule_OrderMinimumValue_ShouldProvideContext()
    {
        // Arrange
        var ruleName = "OrderMinimumValue";
        var minimumValue = 50m;
        var actualValue = 25m;
        var message = $"Order value ${actualValue} is below minimum required ${minimumValue}";

        // Act
        var exception = new BusinessRuleException(ruleName, message);

        // Assert
        exception.RuleName.Should().Be(ruleName);
        exception.Message.Should().Contain(actualValue.ToString());
        exception.Message.Should().Contain(minimumValue.ToString());
    }

    [Fact]
    public void BusinessRule_MaximumRetryAttempts_ShouldIndicateFailure()
    {
        // Arrange
        var ruleName = "MaximumRetryAttempts";
        var message = "Maximum retry attempts (3) exceeded for operation";

        // Act
        var exception = new BusinessRuleException(ruleName, message);

        // Assert
        exception.RuleName.Should().Be(ruleName);
        exception.Message.Should().Contain("Maximum retry attempts");
    }

    [Fact]
    public void BusinessRule_DateValidation_ShouldDescribeViolation()
    {
        // Arrange
        var ruleName = "ValidEventDate";
        var eventDate = DateTime.UtcNow.AddDays(-1);
        var message = $"Event date {eventDate:yyyy-MM-dd} cannot be in the past";

        // Act
        var exception = new BusinessRuleException(ruleName, message);

        // Assert
        exception.RuleName.Should().Be(ruleName);
        exception.Message.Should().Contain("cannot be in the past");
    }

    [Fact]
    public void BusinessRule_ConcurrencyViolation_ShouldExplainConflict()
    {
        // Arrange
        var ruleName = "OptimisticConcurrency";
        var message = "The record has been modified by another user. Please refresh and try again.";

        // Act
        var exception = new BusinessRuleException(ruleName, message);

        // Assert
        exception.RuleName.Should().Be(ruleName);
        exception.Message.Should().Contain("modified by another user");
    }

    #endregion

    #region Catch and Handle Tests

    [Fact]
    public void Exception_ShouldBeCatchableAsBusinessRuleException()
    {
        // Arrange
        var ruleName = "TestRule";
        var message = "Test violation";
        var caught = false;

        // Act
        try
        {
            throw new BusinessRuleException(ruleName, message);
        }
        catch (BusinessRuleException ex)
        {
            caught = true;
            ex.RuleName.Should().Be(ruleName);
        }

        // Assert
        caught.Should().BeTrue();
    }

    [Fact]
    public void Exception_ShouldBeCatchableAsDomainException()
    {
        // Arrange
        var caught = false;

        // Act
        try
        {
            throw new BusinessRuleException("Rule", "Message");
        }
        catch (DomainException)
        {
            caught = true;
        }

        // Assert
        caught.Should().BeTrue();
    }

    #endregion
}
