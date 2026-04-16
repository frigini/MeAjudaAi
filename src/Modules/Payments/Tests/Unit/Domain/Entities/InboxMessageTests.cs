using System.Reflection;
using MeAjudaAi.Modules.Payments.Domain.Entities;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Domain.Entities;

public class InboxMessageTests
{
    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        // Act
        var message = new InboxMessage("default_type", "{}");

        // Assert
        message.Id.Should().NotBeEmpty();
        message.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        message.ProcessedAt.Should().BeNull();
        message.Error.Should().BeNull();
        message.RetryCount.Should().Be(0);
        message.MaxRetries.Should().Be(5);
        message.NextAttemptAt.Should().BeNull();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnTrue_WhenNotProcessedAndUnderMaxRetries()
    {
        // Act
        var message = new InboxMessage("test", "{}");

        // Assert
        message.ShouldRetry.Should().BeTrue();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnFalse_WhenProcessed()
    {
        // Arrange
        var message = new InboxMessage("test", "{}");
        message.MarkAsProcessed();

        // Assert
        message.ShouldRetry.Should().BeFalse();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnFalse_WhenMaxRetriesReached()
    {
        // Arrange
        var message = new InboxMessage("test", "{}");
        for (int i = 0; i < message.MaxRetries; i++)
        {
            message.IncrementRetry();
        }

        // Assert
        message.ShouldRetry.Should().BeFalse();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnFalse_WhenNextAttemptInFuture()
    {
        // Arrange
        var message = new InboxMessage("test", "{}");
        message.RecordError("Some error"); // Sets NextAttemptAt in the future

        // Assert
        message.ShouldRetry.Should().BeFalse();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnTrue_WhenNextAttemptInPast()
    {
        // Arrange
        var message = new InboxMessage("test", "{}");
        message.IncrementRetry();
        
        // Use o novo método de domínio para definir uma data no passado sem reflexão
        message.RecordError("Some error", nextAttemptAt: DateTime.UtcNow.AddMinutes(-1));

        // Assert
        message.ShouldRetry.Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeReflectedAfterDomainMethods()
    {
        // Arrange
        var message = new InboxMessage("checkout.session.completed", "{\"event\":\"test\"}");

        // Act
        message.IncrementRetry();
        message.RecordError("Test error");

        // Assert
        message.Type.Should().Be("checkout.session.completed");
        message.Content.Should().Be("{\"event\":\"test\"}");
        message.Error.Should().Be("Test error");
        message.RetryCount.Should().Be(1);
        message.NextAttemptAt.Should().BeAfter(DateTime.UtcNow);
        
        var processedDate = DateTime.UtcNow.AddSeconds(1);
        message.MarkAsProcessed(processedDate);
        message.ProcessedAt.Should().Be(processedDate);
        message.Error.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenTypeTooLong()
    {
        // Arrange
        var longType = new string('a', 256);

        // Act
        var act = () => new InboxMessage(longType, "{}");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Type length cannot exceed 255*");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenContentIsNotValidJson()
    {
        // Act
        var act = () => new InboxMessage("test", "invalid-json");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*must be a valid JSON*");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenTypeIsNull()
    {
        // Act
        var act = () => new InboxMessage(null!, "{}");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenTypeIsEmpty()
    {
        // Act
        var act = () => new InboxMessage("  ", "{}");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenContentIsNull()
    {
        // Act
        var act = () => new InboxMessage("test", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenContentIsEmpty()
    {
        // Act
        var act = () => new InboxMessage("test", "   ");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldSetProperties_WhenValidInput()
    {
        // Act
        var message = new InboxMessage("checkout.session.completed", "{\"id\":\"evt_123\"}");

        // Assert
        message.Type.Should().Be("checkout.session.completed");
        message.Content.Should().Be("{\"id\":\"evt_123\"}");
        message.ProcessedAt.Should().BeNull();
        message.RetryCount.Should().Be(0);
        message.MaxRetries.Should().Be(5);
    }
}