using FluentAssertions;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Events;
using MeAjudaAi.Modules.Documents.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Documents;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Infrastructure.Events;

[Trait("Category", "Unit")]
[Trait("Module", "Documents")]
[Trait("Layer", "Infrastructure")]
public class DocumentVerifiedDomainEventHandlerTests
{
    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly Mock<ILogger<DocumentVerifiedDomainEventHandler>> _loggerMock;
    private readonly DocumentVerifiedDomainEventHandler _handler;

    public DocumentVerifiedDomainEventHandlerTests()
    {
        _messageBusMock = new Mock<IMessageBus>();
        _loggerMock = new Mock<ILogger<DocumentVerifiedDomainEventHandler>>();
        _handler = new DocumentVerifiedDomainEventHandler(_messageBusMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidEvent_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.IdentityDocument;
        var hasOcrData = true;
        var domainEvent = new DocumentVerifiedDomainEvent(documentId, 1, providerId, documentType, hasOcrData);

        _messageBusMock
            .Setup(x => x.PublishAsync(
                It.IsAny<DocumentVerifiedIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.Is<DocumentVerifiedIntegrationEvent>(e =>
                    e.DocumentId == documentId &&
                    e.ProviderId == providerId &&
                    e.DocumentType == documentType.ToString() &&
                    e.HasOcrData == hasOcrData &&
                    e.Source == "Documents"),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidEvent_ShouldLogInformation()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var domainEvent = new DocumentVerifiedDomainEvent(documentId, 1, providerId, EDocumentType.ProofOfResidence, false);

        _messageBusMock
            .Setup(x => x.PublishAsync(It.IsAny<DocumentVerifiedIntegrationEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Handling DocumentVerifiedDomainEvent for document {documentId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Successfully published DocumentVerified integration event for document {documentId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMessageBusThrows_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var domainEvent = new DocumentVerifiedDomainEvent(documentId, 1, Guid.NewGuid(), EDocumentType.CriminalRecord, true);
        var exception = new InvalidOperationException("Message bus error");

        _messageBusMock
            .Setup(x => x.PublishAsync(It.IsAny<DocumentVerifiedIntegrationEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var act = async () => await _handler.HandleAsync(domainEvent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Message bus error");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error handling DocumentVerifiedDomainEvent for document {documentId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithOtherDocumentType_ShouldPublishCorrectly()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var domainEvent = new DocumentVerifiedDomainEvent(documentId, 1, providerId, EDocumentType.Other, false);

        _messageBusMock
            .Setup(x => x.PublishAsync(It.IsAny<DocumentVerifiedIntegrationEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.Is<DocumentVerifiedIntegrationEvent>(e =>
                    e.DocumentType == "Other" &&
                    e.HasOcrData == false),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldSetVerifiedAtToCurrentUtcTime()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var domainEvent = new DocumentVerifiedDomainEvent(Guid.NewGuid(), 1, Guid.NewGuid(), EDocumentType.IdentityDocument, true);

        DocumentVerifiedIntegrationEvent? capturedEvent = null;
        _messageBusMock
            .Setup(x => x.PublishAsync(It.IsAny<DocumentVerifiedIntegrationEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<DocumentVerifiedIntegrationEvent, string, CancellationToken>((e, _, _) => capturedEvent = e)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.VerifiedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }
}
