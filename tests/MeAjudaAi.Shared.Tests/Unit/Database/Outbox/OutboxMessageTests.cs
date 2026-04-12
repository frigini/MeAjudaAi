using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Contracts.Shared;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Database.Outbox;

public class OutboxMessageTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateInPendingStatus()
    {
        // Act
        var message = OutboxMessage.Create("Type", "Payload", ECommunicationPriority.High);

        // Assert
        message.Type.Should().Be("Type");
        message.Payload.Should().Be("Payload");
        message.Status.Should().Be(EOutboxMessageStatus.Pending);
        message.Priority.Should().Be(ECommunicationPriority.High);
        message.RetryCount.Should().Be(0);
    }

    [Fact]
    public void IsReadyToProcess_WhenPendingAndNoScheduledDate_ShouldBeTrue()
    {
        // Arrange
        var message = OutboxMessage.Create("T", "P");

        // Act & Assert
        message.IsReadyToProcess(DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsReadyToProcess_WhenFutureScheduledDate_ShouldBeFalse()
    {
        // Arrange
        var message = OutboxMessage.Create("T", "P", scheduledAt: DateTime.UtcNow.AddMinutes(5));

        // Act & Assert
        message.IsReadyToProcess(DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void MarkAsProcessing_ShouldUpdateStatus()
    {
        // Arrange
        var message = OutboxMessage.Create("T", "P");

        // Act
        message.MarkAsProcessing();

        // Assert
        message.Status.Should().Be(EOutboxMessageStatus.Processing);
    }

    [Fact]
    public void MarkAsSent_ShouldUpdateStatusAndSentAt()
    {
        // Arrange
        var message = OutboxMessage.Create("T", "P");
        var sentAt = DateTime.UtcNow;

        // Act
        message.MarkAsSent(sentAt);

        // Assert
        message.Status.Should().Be(EOutboxMessageStatus.Sent);
        message.SentAt.Should().Be(sentAt);
    }

    [Fact]
    public void MarkAsFailed_WhenBelowMaxRetries_ShouldStayPendingAndIncrementCount()
    {
        // Arrange
        var message = OutboxMessage.Create("T", "P", maxRetries: 3);

        // Act
        message.MarkAsFailed("Error");

        // Assert
        message.Status.Should().Be(EOutboxMessageStatus.Pending);
        message.RetryCount.Should().Be(1);
        message.ErrorMessage.Should().Be("Error");
    }

    [Fact]
    public void MarkAsFailed_WhenMaxRetriesReached_ShouldSetStatusToFailed()
    {
        // Arrange
        var message = OutboxMessage.Create("T", "P", maxRetries: 1);

        // Act
        message.MarkAsFailed("Fatal");

        // Assert
        message.Status.Should().Be(EOutboxMessageStatus.Failed);
        message.HasRetriesLeft.Should().BeFalse();
    }
}
