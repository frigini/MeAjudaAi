using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Contracts.Shared;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Domain.Entities;

public class OutboxMessageTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateMessageInPendingStatus()
    {
        // Arrange
        var channel = ECommunicationChannel.Email;
        var payload = "{\"To\":\"test@example.com\"}";
        var priority = ECommunicationPriority.High;
        var scheduledAt = DateTime.UtcNow.AddHours(1);
        var correlationId = "test-correlation";

        // Act
        var message = OutboxMessage.Create(channel, payload, priority, scheduledAt, 5, correlationId);

        // Assert
        message.Channel.Should().Be(channel);
        message.Payload.Should().Be(payload);
        message.Priority.Should().Be(priority);
        message.ScheduledAt.Should().Be(scheduledAt);
        message.MaxRetries.Should().Be(5);
        message.CorrelationId.Should().Be(correlationId);
        message.Status.Should().Be(EOutboxMessageStatus.Pending);
        message.RetryCount.Should().Be(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidPayload_ShouldThrowArgumentException(string? invalidPayload)
    {
        // Act
        var act = () => OutboxMessage.Create(ECommunicationChannel.Email, invalidPayload!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithInvalidMaxRetries_ShouldThrowArgumentOutOfRangeException()
    {
        // Act
        var act = () => OutboxMessage.Create(ECommunicationChannel.Email, "payload", maxRetries: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void IsReadyToProcess_WhenPendingAndScheduledAtIsPast_ShouldReturnTrue()
    {
        // Arrange
        var message = OutboxMessage.Create(ECommunicationChannel.Email, "payload", scheduledAt: DateTime.UtcNow.AddMinutes(-1));

        // Act
        var result = message.IsReadyToProcess(DateTime.UtcNow);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsReadyToProcess_WhenPendingAndScheduledAtIsFuture_ShouldReturnFalse()
    {
        // Arrange
        var message = OutboxMessage.Create(ECommunicationChannel.Email, "payload", scheduledAt: DateTime.UtcNow.AddMinutes(10));

        // Act
        var result = message.IsReadyToProcess(DateTime.UtcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MarkAsProcessing_WhenPending_ShouldUpdateStatus()
    {
        // Arrange
        var message = OutboxMessage.Create(ECommunicationChannel.Email, "payload");

        // Act
        message.MarkAsProcessing();

        // Assert
        message.Status.Should().Be(EOutboxMessageStatus.Processing);
    }

    [Fact]
    public void ResetToPending_WhenProcessing_ShouldUpdateStatus()
    {
        // Arrange
        var message = OutboxMessage.Create(ECommunicationChannel.Email, "payload");
        message.MarkAsProcessing();

        // Act
        message.ResetToPending();

        // Assert
        message.Status.Should().Be(EOutboxMessageStatus.Pending);
    }

    [Fact]
    public void MarkAsSent_WhenProcessing_ShouldUpdateStatusAndSentAt()
    {
        // Arrange
        var message = OutboxMessage.Create(ECommunicationChannel.Email, "payload");
        message.MarkAsProcessing();
        var sentAt = DateTime.UtcNow;

        // Act
        message.MarkAsSent(sentAt);

        // Assert
        message.Status.Should().Be(EOutboxMessageStatus.Sent);
        message.SentAt.Should().Be(sentAt);
    }

    [Fact]
    public void MarkAsFailed_WhenBelowMaxRetries_ShouldIncrementRetryCountAndStayPending()
    {
        // Arrange
        var message = OutboxMessage.Create(ECommunicationChannel.Email, "payload", maxRetries: 3);
        message.MarkAsProcessing();

        // Act
        message.MarkAsFailed("Error 1");

        // Assert
        message.RetryCount.Should().Be(1);
        message.ErrorMessage.Should().Be("Error 1");
        message.Status.Should().Be(EOutboxMessageStatus.Pending);
    }

    [Fact]
    public void MarkAsFailed_WhenReachesMaxRetries_ShouldUpdateStatusToFailed()
    {
        // Arrange
        var message = OutboxMessage.Create(ECommunicationChannel.Email, "payload", maxRetries: 1);
        message.MarkAsProcessing();

        // Act
        message.MarkAsFailed("Fatal Error");

        // Assert
        message.RetryCount.Should().Be(1);
        message.Status.Should().Be(EOutboxMessageStatus.Failed);
        message.HasRetriesLeft.Should().BeFalse();
    }
}
