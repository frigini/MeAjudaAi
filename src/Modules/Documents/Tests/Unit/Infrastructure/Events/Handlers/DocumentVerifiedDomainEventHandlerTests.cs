using FluentAssertions;
using MeAjudaAi.Modules.Documents.Domain.Events;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Documents;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Infrastructure.Events.Handlers;

public class DocumentVerifiedDomainEventHandlerTests
{
    private readonly Mock<IMessageBus> _mockBus;
    private readonly Mock<ILogger<DocumentVerifiedDomainEventHandler>> _mockLogger;
    private readonly DocumentVerifiedDomainEventHandler _handler;

    public DocumentVerifiedDomainEventHandlerTests()
    {
        _mockBus = new Mock<IMessageBus>();
        _mockLogger = new Mock<ILogger<DocumentVerifiedDomainEventHandler>>();
        _handler = new DocumentVerifiedDomainEventHandler(_mockBus.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishIntegrationEvent()
    {
        var @event = new DocumentVerifiedDomainEvent(Guid.NewGuid(), 1, Guid.NewGuid(), EDocumentType.IdentityDocument, true);

        await _handler.HandleAsync(@event);

        _mockBus.Verify(x => x.PublishAsync(It.IsAny<DocumentVerifiedIntegrationEvent>(), null, default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMessageBusFails_ShouldThrow()
    {
        var @event = new DocumentVerifiedDomainEvent(Guid.NewGuid(), 1, Guid.NewGuid(), EDocumentType.IdentityDocument, true);
        _mockBus.Setup(x => x.PublishAsync(It.IsAny<DocumentVerifiedIntegrationEvent>(), null, default))
            .ThrowsAsync(new Exception("Bus error"));

        var act = async () => await _handler.HandleAsync(@event);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
