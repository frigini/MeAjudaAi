using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Communications;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Domain.Entities;

public class CommunicationLogTests
{
    [Fact]
    public void CreateSuccess_WithValidData_ShouldCreateLog()
    {
        // Arrange
        var correlationId = "corr-123";
        var channel = ECommunicationChannel.Email;
        var recipient = "test@test.com";
        var attemptCount = 1;
        var outboxId = Guid.NewGuid();
        var templateKey = "template-1";

        // Act
        var log = new CommunicationLogBuilder()
            .WithCorrelationId(correlationId)
            .WithChannel(channel)
            .WithRecipient(recipient)
            .WithAttemptCount(attemptCount)
            .WithOutboxMessageId(outboxId)
            .WithTemplateKey(templateKey)
            .AsSuccess()
            .Build();

        // Assert
        log.CorrelationId.Should().Be(correlationId);
        log.Channel.Should().Be(channel);
        log.Recipient.Should().Be(recipient);
        log.AttemptCount.Should().Be(attemptCount);
        log.OutboxMessageId.Should().Be(outboxId);
        log.TemplateKey.Should().Be(templateKey);
        log.IsSuccess.Should().BeTrue();
        log.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void CreateFailure_WithValidData_ShouldCreateLog()
    {
        // Arrange
        var correlationId = "corr-456";
        var channel = ECommunicationChannel.Sms;
        var recipient = "+5511999999999";
        var errorMessage = "Provider Down";
        var attemptCount = 3;

        // Act
        var log = new CommunicationLogBuilder()
            .WithCorrelationId(correlationId)
            .WithChannel(channel)
            .WithRecipient(recipient)
            .WithAttemptCount(attemptCount)
            .AsFailure(errorMessage)
            .Build();

        // Assert
        log.CorrelationId.Should().Be(correlationId);
        log.Channel.Should().Be(channel);
        log.Recipient.Should().Be(recipient);
        log.ErrorMessage.Should().Be(errorMessage);
        log.AttemptCount.Should().Be(attemptCount);
        log.IsSuccess.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateSuccess_WithInvalidCorrelationId_ShouldThrowArgumentException(string? invalidId)
    {
        // Act
        var act = () => new CommunicationLogBuilder()
            .WithCorrelationId(invalidId!)
            .WithChannel(ECommunicationChannel.Email)
            .WithRecipient("test@test.com")
            .WithAttemptCount(1)
            .AsSuccess()
            .Build();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateSuccess_WithNegativeAttemptCount_ShouldThrowArgumentOutOfRangeException()
    {
        // Act
        var act = () => new CommunicationLogBuilder()
            .WithCorrelationId("id")
            .WithChannel(ECommunicationChannel.Email)
            .WithRecipient("test@test.com")
            .WithAttemptCount(-1)
            .AsSuccess()
            .Build();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null, ECommunicationChannel.Email, "test@test.com", "Error")]
    [InlineData("id", ECommunicationChannel.Email, null, "Error")]
    [InlineData("id", ECommunicationChannel.Email, "test@test.com", null)]
    public void CreateFailure_WithInvalidData_ShouldThrowArgumentException(string? corrId, ECommunicationChannel channel, string? recipient, string? error)
    {
        // Act
        var act = () => new CommunicationLogBuilder()
            .WithCorrelationId(corrId!)
            .WithChannel(channel)
            .WithRecipient(recipient!)
            .WithAttemptCount(1)
            .AsFailure(error!)
            .Build();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateFailure_WithNegativeAttemptCount_ShouldThrowArgumentOutOfRangeException()
    {
        // Act
        var act = () => new CommunicationLogBuilder()
            .WithCorrelationId("id")
            .WithChannel(ECommunicationChannel.Email)
            .WithRecipient("test@test.com")
            .WithAttemptCount(-1)
            .AsFailure("Error")
            .Build();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
