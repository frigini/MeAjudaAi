using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events.Handlers;

[Trait("Category", "Unit")]
public class ProviderServiceRemovedDomainEventHandlerTests
{
    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly Mock<ILogger<ProviderServiceRemovedDomainEventHandler>> _loggerMock;
    private readonly ProviderServiceRemovedDomainEventHandler _handler;

    public ProviderServiceRemovedDomainEventHandlerTests()
    {
        _messageBusMock = new Mock<IMessageBus>();
        _loggerMock = new Mock<ILogger<ProviderServiceRemovedDomainEventHandler>>();
        _handler = new ProviderServiceRemovedDomainEventHandler(_messageBusMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var domainEvent = new ProviderServiceRemovedDomainEvent(providerId, 1, serviceId);

        _messageBusMock
            .Setup(x => x.PublishAsync(It.IsAny<ProviderServicesUpdatedIntegrationEvent>(), null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.Is<ProviderServicesUpdatedIntegrationEvent>(e => e.ProviderId == providerId),
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenPublishThrowsException_ShouldNotPropagateException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var domainEvent = new ProviderServiceRemovedDomainEvent(providerId, 1, Guid.NewGuid());

        _messageBusMock
            .Setup(x => x.PublishAsync(It.IsAny<ProviderServicesUpdatedIntegrationEvent>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Message bus error"));

        // Act
        var act = async () => await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
