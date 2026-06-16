using MeAjudaAi.Modules.Communications.Application.Handlers.Events;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Database.Exceptions;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers.Events;

public class UserDeletedIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<ICommunicationLogQueries> _logQueriesMock;
    private readonly Mock<ILogger<UserDeletedIntegrationEventHandler>> _loggerMock;
    private readonly Mock<ISerializer> _serializerMock;
    private readonly UserDeletedIntegrationEventHandler _handler;

    public UserDeletedIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _logQueriesMock = new Mock<ICommunicationLogQueries>();
        _loggerMock = new Mock<ILogger<UserDeletedIntegrationEventHandler>>();
        _serializerMock = new Mock<ISerializer>();

        _serializerMock.Setup(x => x.Serialize(It.IsAny<object>())).Returns("{}");

        _handler = new UserDeletedIntegrationEventHandler(
            _outboxRepositoryMock.Object,
            _logQueriesMock.Object,
            _serializerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidEvent_ShouldEnqueueOutboxMessage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var integrationEvent = new UserDeletedIntegrationEvent("Users", userId, "john@example.com", "John", DateTime.UtcNow);

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
        var userId = Guid.NewGuid();
        var integrationEvent = new UserDeletedIntegrationEvent("Users", userId, "john@example.com", "John", DateTime.UtcNow);

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
        var userId = Guid.NewGuid();
        var integrationEvent = new UserDeletedIntegrationEvent("Users", userId, null!, "John", DateTime.UtcNow);

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenMissingFirstName_ShouldSkip()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var integrationEvent = new UserDeletedIntegrationEvent("Users", userId, "john@example.com", null!, DateTime.UtcNow);

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
        var userId = Guid.NewGuid();
        var integrationEvent = new UserDeletedIntegrationEvent("Users", userId, "john@example.com", "John", DateTime.UtcNow);

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
        var userId = Guid.NewGuid();
        var integrationEvent = new UserDeletedIntegrationEvent("Users", userId, "john@example.com", "John", DateTime.UtcNow);

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _outboxRepositoryMock.Setup(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(integrationEvent));
    }
}
