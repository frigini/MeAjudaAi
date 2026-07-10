using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using System.Text.Json;

namespace MeAjudaAi.Shared.Tests.Contracts.Unit.DTOs;

[Trait("Category", "Unit")]
[Trait("Component", "Contracts")]
public class CommunicationLogDtoTests
{
    [Fact]
    public void CommunicationLogDto_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var dto = new CommunicationLogDto(
            Id: Guid.NewGuid(),
            CorrelationId: "corr-123",
            Channel: "email",
            Recipient: "user@example.com",
            TemplateKey: "welcome-email",
            IsSuccess: true,
            ErrorMessage: null,
            AttemptCount: 1,
            CreatedAt: DateTime.UtcNow
        );

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<CommunicationLogDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Channel.Should().Be("email");
        deserialized.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CommunicationLogDto_WithFailure_ShouldIncludeError()
    {
        // Arrange
        var dto = new CommunicationLogDto(
            Id: Guid.NewGuid(),
            CorrelationId: "corr-456",
            Channel: "sms",
            Recipient: "+5511999999999",
            TemplateKey: null,
            IsSuccess: false,
            ErrorMessage: "Delivery failed",
            AttemptCount: 3,
            CreatedAt: DateTime.UtcNow
        );

        // Act & Assert
        dto.IsSuccess.Should().BeFalse();
        dto.ErrorMessage.Should().Be("Delivery failed");
        dto.AttemptCount.Should().Be(3);
    }

    [Fact]
    public void CommunicationLogDto_WithOutboxMessageId_ShouldSerialize()
    {
        // Arrange
        var outboxId = Guid.NewGuid();
        var dto = new CommunicationLogDto(
            Id: Guid.NewGuid(),
            CorrelationId: "corr-789",
            Channel: "push",
            Recipient: "device-token-abc",
            TemplateKey: "booking-confirmed",
            IsSuccess: true,
            ErrorMessage: null,
            AttemptCount: 1,
            CreatedAt: DateTime.UtcNow,
            OutboxMessageId: outboxId
        );

        // Act & Assert
        dto.OutboxMessageId.Should().Be(outboxId);
    }

    [Theory]
    [InlineData("email")]
    [InlineData("sms")]
    [InlineData("push")]
    public void CommunicationLogDto_ShouldSupportMultipleChannels(string channel)
    {
        // Arrange & Act
        var dto = new CommunicationLogDto(
            Id: Guid.NewGuid(),
            CorrelationId: "corr",
            Channel: channel,
            Recipient: "recipient",
            TemplateKey: null,
            IsSuccess: true,
            ErrorMessage: null,
            AttemptCount: 1,
            CreatedAt: DateTime.UtcNow
        );

        // Assert
        dto.Channel.Should().Be(channel);
    }
}
