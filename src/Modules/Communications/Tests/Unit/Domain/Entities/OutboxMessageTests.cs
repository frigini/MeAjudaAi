using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Communications;

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
        var message = new OutboxMessageBuilder()
            .WithChannel(channel)
            .WithPayload(payload)
            .WithPriority(priority)
            .AsScheduled(scheduledAt)
            .WithMaxRetries(5)
            .WithCorrelationId(correlationId)
            .Build();

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
        var act = () => new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload(invalidPayload!)
            .Build();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithInvalidMaxRetries_ShouldThrowArgumentOutOfRangeException()
    {
        // Act
        var act = () => new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("payload")
            .WithMaxRetries(0)
            .Build();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void IsReadyToProcess_WhenPendingAndScheduledAtIsPast_ShouldReturnTrue()
    {
        // Arrange
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("payload")
            .AsScheduled(DateTime.UtcNow.AddMinutes(-1))
            .Build();

        // Act
        var result = message.IsReadyToProcess(DateTime.UtcNow);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsReadyToProcess_WhenPendingAndScheduledAtIsFuture_ShouldReturnFalse()
    {
        // Arrange
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("payload")
            .AsScheduled(DateTime.UtcNow.AddMinutes(10))
            .Build();

        // Act
        var result = message.IsReadyToProcess(DateTime.UtcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MarkAsProcessing_WhenPending_ShouldUpdateStatus()
    {
        // Arrange
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("payload")
            .Build();

        // Act
        message.MarkAsProcessing();

        // Assert
        message.Status.Should().Be(EOutboxMessageStatus.Processing);
    }

    [Fact]
    public void ResetToPending_WhenProcessing_ShouldUpdateStatus()
    {
        // Arrange
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("payload")
            .Build();
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
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("payload")
            .Build();
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
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("payload")
            .WithMaxRetries(3)
            .Build();
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
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("payload")
            .WithMaxRetries(1)
            .Build();
        message.MarkAsProcessing();

        // Act
        message.MarkAsFailed("Fatal Error");

        // Assert
        message.RetryCount.Should().Be(1);
        message.Status.Should().Be(EOutboxMessageStatus.Failed);
        message.HasRetriesLeft.Should().BeFalse();
    }
}
