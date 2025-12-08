using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events.Handlers;

public class ProviderVerificationStatusUpdatedDomainEventHandlerTests
{
    private readonly Mock<IMessageBus> _mockMessageBus;
    private readonly Mock<ILogger<ProviderVerificationStatusUpdatedDomainEventHandler>> _mockLogger;
    private readonly ProviderVerificationStatusUpdatedDomainEventHandler _handler;

    public ProviderVerificationStatusUpdatedDomainEventHandlerTests()
    {
        _mockMessageBus = new Mock<IMessageBus>();
        _mockLogger = new Mock<ILogger<ProviderVerificationStatusUpdatedDomainEventHandler>>();
        _handler = new ProviderVerificationStatusUpdatedDomainEventHandler(_mockMessageBus.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var providerId = ProviderId.New();
        var domainEvent = new ProviderVerificationStatusUpdatedDomainEvent(
            providerId,
            VerificationStatus.Pending,
            VerificationStatus.Verified);

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockMessageBus.Verify(
            x => x.PublishAsync(
                It.IsAny<ProviderVerificationStatusUpdatedIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMessageBusFails_ShouldLogError()
    {
        // Arrange
        var providerId = ProviderId.New();
        var domainEvent = new ProviderVerificationStatusUpdatedDomainEvent(
            providerId,
            VerificationStatus.Pending,
            VerificationStatus.Verified);

        _mockMessageBus
            .Setup(x => x.PublishAsync(
                It.IsAny<ProviderVerificationStatusUpdatedIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Message bus error"));

        // Act
        var act = async () => await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Message bus error");
    }
}
