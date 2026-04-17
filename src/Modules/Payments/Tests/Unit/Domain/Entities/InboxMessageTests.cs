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
    public void Constructor_ShouldThrow_WhenInvalidJson()
    {
        // Act
        var act = () => new InboxMessage("type", "{ invalid json }");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*valid JSON*");
    }

    [Fact]
    public void RecordError_ShouldFollowExponentialBackoff()
    {
        // Arrange
        var msg = new InboxMessage("t", "{}", "e");

        // Act 1
        msg.RecordError("err1");
        var firstAttempt = msg.NextAttemptAt;

        // Act 2
        msg.RecordError("err2");
        var secondAttempt = msg.NextAttemptAt;

        // Assert
        secondAttempt.Should().BeAfter(firstAttempt!.Value);
        (secondAttempt - DateTime.UtcNow).Should().BeGreaterThan(TimeSpan.FromSeconds(60)); // 2^2 * 30 = 120s
    }

    [Fact]
    public void ShouldRetry_ShouldReturnTrue_WhenPendingAndUnderMaxRetries()
    {
        // Arrange
        var msg = new InboxMessage("t", "{}", "e");

        // Assert
        msg.ShouldRetry.Should().BeTrue();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnFalse_WhenProcessed()
    {
        // Arrange
        var msg = new InboxMessage("t", "{}", "e");
        msg.MarkAsProcessed();

        // Assert
        msg.ShouldRetry.Should().BeFalse();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnFalse_WhenMaxRetriesReached()
    {
        // Arrange
        var msg = new InboxMessage("t", "{}", "e");
        for (int i = 0; i < 5; i++) msg.RecordError("err");

        // Assert
        msg.RetryCount.Should().Be(5);
        msg.ShouldRetry.Should().BeFalse();
    }

    [Fact]
    public void ShouldRetry_ShouldRespectNextAttemptAt()
    {
        // Arrange
        var msg = new InboxMessage("t", "{}", "e");
        msg.RecordError("err", nextAttemptAt: DateTime.UtcNow.AddHours(1));

        // Assert
        msg.ShouldRetry.Should().BeFalse();
    }
}
