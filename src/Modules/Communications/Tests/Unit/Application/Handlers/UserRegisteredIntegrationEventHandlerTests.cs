using MeAjudaAi.Modules.Communications.Application.Handlers;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers;

public class UserRegisteredIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<ICommunicationLogRepository> _logRepositoryMock;
    private readonly Mock<ILogger<UserRegisteredIntegrationEventHandler>> _loggerMock;
    private readonly UserRegisteredIntegrationEventHandler _handler;

    public UserRegisteredIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _logRepositoryMock = new Mock<ICommunicationLogRepository>();
        _loggerMock = new Mock<ILogger<UserRegisteredIntegrationEventHandler>>();
        _handler = new UserRegisteredIntegrationEventHandler(
            _outboxRepositoryMock.Object,
            _logRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenNewUser_ShouldEnqueueOutboxMessage()
    {
        // Arrange
        var integrationEvent = new UserRegisteredIntegrationEvent(
            "Users",
            Guid.NewGuid(),
            "test@test.com",
            "testuser",
            "John",
            "Doe",
            "kc-123",
            new[] { "User" },
            DateTime.UtcNow);

        _logRepositoryMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenAlreadyProcessed_ShouldSkip()
    {
        // Arrange
        var integrationEvent = new UserRegisteredIntegrationEvent(
            "Users",
            Guid.NewGuid(),
            "test@test.com",
            "testuser",
            "John",
            "Doe",
            "kc-123",
            new[] { "User" },
            DateTime.UtcNow);

        _logRepositoryMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUniqueConstraintViolationOccurs_ShouldHandleGracefully()
    {
        // Arrange
        var integrationEvent = new UserRegisteredIntegrationEvent(
            "Users",
            Guid.NewGuid(),
            "test@test.com",
            "testuser",
            "John",
            "Doe",
            "kc-123",
            new[] { "User" },
            DateTime.UtcNow);

        _logRepositoryMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Throw UniqueConstraintException directly
        _outboxRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MeAjudaAi.Shared.Database.Exceptions.UniqueConstraintException("Already exists"));

        // Act
        var act = () => _handler.HandleAsync(integrationEvent);

        // Assert
        // Should not throw because handler catches it
        await act.Should().NotThrowAsync();

        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenGenericExceptionOccurs_ShouldPropagate()
    {
        // Arrange
        var integrationEvent = new UserRegisteredIntegrationEvent(
            "Users",
            Guid.NewGuid(),
            "test@test.com",
            "testuser",
            "John",
            "Doe",
            "kc-123",
            new[] { "User" },
            DateTime.UtcNow);

        _logRepositoryMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _outboxRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Generic database error"));

        // Act
        var act = () => _handler.HandleAsync(integrationEvent);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Generic database error");
        _logRepositoryMock.Verify(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
