using MeAjudaAi.Modules.Payments.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Domain.Entities;

public class InboxMessageTests
{
    [Fact]
    public void Constructor_ShouldSetInitialValues()
    {
        // Arrange
        var type = "test.event";
        var content = "{\"id\": 123}";
        var externalId = "evt_123";

        // Act
        var msg = new InboxMessage(type, content, externalId);

        // Assert
        msg.Type.Should().Be(type);
        msg.Content.Should().Be(content);
        msg.ExternalEventId.Should().Be(externalId);
        msg.ProcessedAt.Should().BeNull();
        msg.RetryCount.Should().Be(0);
        msg.MaxRetries.Should().Be(5);
    }

    [Fact]
    public void MarkAsProcessed_ShouldSetDate()
    {
        // Arrange
        var msg = new InboxMessage("t", "{}", "e");

        // Act
        msg.MarkAsProcessed();

        // Assert
        msg.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void IncrementRetry_ShouldUpdateCount()
    {
        // Arrange
        var msg = new InboxMessage("t", "{}", "e");

        // Act
        msg.IncrementRetry();

        // Assert
        msg.RetryCount.Should().Be(1);
    }

    [Fact]
    public void RecordError_ShouldIncrementRetryAndSetNextAttempt()
    {
        // Arrange
        var msg = new InboxMessage("t", "{}", "e");
        var error = "Connection failed";

        // Act
        msg.RecordError(error);

        // Assert
        msg.Error.Should().Contain(error);
        msg.RetryCount.Should().Be(1);
        msg.NextAttemptAt.Should().BeAfter(msg.CreatedAt);
    }
}
