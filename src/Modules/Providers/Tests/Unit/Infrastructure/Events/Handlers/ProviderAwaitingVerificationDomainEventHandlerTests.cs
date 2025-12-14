using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events.Handlers;

public class ProviderAwaitingVerificationDomainEventHandlerTests
{
    private readonly Mock<IMessageBus> _mockMessageBus;
    private readonly Mock<ILogger<ProviderAwaitingVerificationDomainEventHandler>> _mockLogger;
    private readonly ProviderAwaitingVerificationDomainEventHandler _handler;

    public ProviderAwaitingVerificationDomainEventHandlerTests()
    {
        _mockMessageBus = new Mock<IMessageBus>();
        _mockLogger = new Mock<ILogger<ProviderAwaitingVerificationDomainEventHandler>>();
        _handler = new ProviderAwaitingVerificationDomainEventHandler(_mockMessageBus.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var domainEvent = new ProviderAwaitingVerificationDomainEvent(
            providerId,
            1, // Version
            userId,
            "Test Provider",
            null // UpdatedBy
        );

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockMessageBus.Verify(
            x => x.PublishAsync(
                It.IsAny<ProviderAwaitingVerificationIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMessageBusFails_ShouldLogError()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var domainEvent = new ProviderAwaitingVerificationDomainEvent(
            providerId,
            1, // Version
            userId,
            "Test Provider",
            null // UpdatedBy
        );

        _mockMessageBus
            .Setup(x => x.PublishAsync(
                It.IsAny<ProviderAwaitingVerificationIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Message bus error"));

        // Act
        var act = async () => await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.InnerException.Should().BeOfType<Exception>();
        ex.Which.InnerException!.Message.Should().Be("Message bus error");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error handling") || v.ToString()!.Contains("Error publishing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
