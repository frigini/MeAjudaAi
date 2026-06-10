using MeAjudaAi.Modules.Communications.Application.Handlers;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers.Events;

public class UserProfileUpdatedIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<ICommunicationLogQueries> _logQueriesMock;
    private readonly Mock<ILogger<UserProfileUpdatedIntegrationEventHandler>> _loggerMock;
    private readonly Mock<ISerializer> _serializerMock;
    private readonly UserProfileUpdatedIntegrationEventHandler _handler;

    public UserProfileUpdatedIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _logQueriesMock = new Mock<ICommunicationLogQueries>();
        _loggerMock = new Mock<ILogger<UserProfileUpdatedIntegrationEventHandler>>();
        _serializerMock = new Mock<ISerializer>();

        _serializerMock.Setup(x => x.Serialize(It.IsAny<object>())).Returns("{}");

        _handler = new UserProfileUpdatedIntegrationEventHandler(
            _outboxRepositoryMock.Object,
            _logQueriesMock.Object,
            _serializerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidEvent_ShouldEnqueueOutboxMessage()
    {
        // Arrange
        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var evt = new UserProfileUpdatedIntegrationEvent(
            "Users", Guid.NewGuid(), "joao@test.com", "João", "Silva", DateTime.UtcNow);

        // Act
        await _handler.HandleAsync(evt);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCorrelationIdAlreadyExists_ShouldNotEnqueueMessage()
    {
        // Arrange
        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var evt = new UserProfileUpdatedIntegrationEvent(
            "Users", Guid.NewGuid(), "joao@test.com", "João", "Silva", DateTime.UtcNow);

        // Act
        await _handler.HandleAsync(evt);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenOutboxFails_ShouldRethrowException()
    {
        // Arrange
        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _outboxRepositoryMock.Setup(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));
        
        var evt = new UserProfileUpdatedIntegrationEvent(
            "Users", Guid.NewGuid(), "joao@test.com", "João", "Silva", DateTime.UtcNow);

        // Act
        Func<Task> act = () => _handler.HandleAsync(evt);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
    }
}
