using MeAjudaAi.Modules.Communications.Application.Handlers.Events;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Database.Exceptions;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers.Events;

public class ProviderDeletedIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<ICommunicationLogQueries> _logQueriesMock;
    private readonly Mock<ILogger<ProviderDeletedIntegrationEventHandler>> _loggerMock;
    private readonly Mock<ISerializer> _serializerMock;
    private readonly ProviderDeletedIntegrationEventHandler _handler;

    public ProviderDeletedIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _logQueriesMock = new Mock<ICommunicationLogQueries>();
        _loggerMock = new Mock<ILogger<ProviderDeletedIntegrationEventHandler>>();
        _serializerMock = new Mock<ISerializer>();

        _serializerMock.Setup(x => x.Serialize(It.IsAny<object>())).Returns("{}");

        _handler = new ProviderDeletedIntegrationEventHandler(
            _outboxRepositoryMock.Object,
            _logQueriesMock.Object,
            _serializerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidEvent_ShouldEnqueueOutboxMessage()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new ProviderDeletedIntegrationEvent("Providers", providerId, Guid.NewGuid(), "provider@test.com", "Provider Name", "Reason", DateTime.UtcNow);

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCorrelationExists_ShouldSkipAndLog()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new ProviderDeletedIntegrationEvent("Providers", providerId, Guid.NewGuid(), "provider@test.com", "Provider Name", "Reason", DateTime.UtcNow);

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenMissingEmail_ShouldSkip()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new ProviderDeletedIntegrationEvent("Providers", providerId, Guid.NewGuid(), null!, "Provider Name", "Reason", DateTime.UtcNow);

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenMissingName_ShouldSkip()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new ProviderDeletedIntegrationEvent("Providers", providerId, Guid.NewGuid(), "provider@test.com", null!, "Reason", DateTime.UtcNow);

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUniqueConstraintViolation_ShouldSkipAndLog()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new ProviderDeletedIntegrationEvent("Providers", providerId, Guid.NewGuid(), "provider@test.com", "Provider Name", "Reason", DateTime.UtcNow);

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _outboxRepositoryMock.Setup(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UniqueConstraintException("ix_outbox_messages_correlation_id", "correlation_id", new Exception()));

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert - Should not throw, should be handled gracefully
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenOtherException_ShouldRethrow()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new ProviderDeletedIntegrationEvent("Providers", providerId, Guid.NewGuid(), "provider@test.com", "Provider Name", "Reason", DateTime.UtcNow);

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _outboxRepositoryMock.Setup(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(integrationEvent));
    }
}
