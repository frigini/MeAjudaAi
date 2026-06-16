using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Users;
using MeAjudaAi.Contracts.Modules.Users.DTOs;
using MeAjudaAi.Modules.Communications.Application.Handlers.Events;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Database.Exceptions;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using MeAjudaAi.Shared.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers.Events;

public class SubscriptionRenewedIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<ICommunicationLogQueries> _logQueriesMock;
    private readonly Mock<IUsersModuleApi> _usersModuleApiMock;
    private readonly Mock<ILogger<SubscriptionRenewedIntegrationEventHandler>> _loggerMock;
    private readonly Mock<ISerializer> _serializerMock;
    private readonly SubscriptionRenewedIntegrationEventHandler _handler;

    public SubscriptionRenewedIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _logQueriesMock = new Mock<ICommunicationLogQueries>();
        _usersModuleApiMock = new Mock<IUsersModuleApi>();
        _loggerMock = new Mock<ILogger<SubscriptionRenewedIntegrationEventHandler>>();
        _serializerMock = new Mock<ISerializer>();

        _serializerMock.Setup(x => x.Serialize(It.IsAny<object>())).Returns("{}");

        _handler = new SubscriptionRenewedIntegrationEventHandler(
            _outboxRepositoryMock.Object,
            _logQueriesMock.Object,
            _usersModuleApiMock.Object,
            _serializerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidEvent_ShouldEnqueueOutboxMessage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var integrationEvent = new SubscriptionRenewedIntegrationEvent("Payments", subscriptionId, userId, DateTime.UtcNow.AddYears(1));
        
        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _usersModuleApiMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleUserDto?>.Success(new ModuleUserDto(userId, "john_doe", "john@example.com", "John", "Doe", "John Doe")));

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCorrelationAlreadyExists_ShouldSkip()
    {
        var integrationEvent = new SubscriptionRenewedIntegrationEvent("Payments", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddYears(1));
        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _handler.HandleAsync(integrationEvent);

        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUserLookupFails_ShouldThrow()
    {
        var userId = Guid.NewGuid();
        var integrationEvent = new SubscriptionRenewedIntegrationEvent("Payments", Guid.NewGuid(), userId, DateTime.UtcNow.AddYears(1));
        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _usersModuleApiMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleUserDto?>.Failure(new Error("Not found", 404)));

        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(integrationEvent));
    }

    [Fact]
    public async Task HandleAsync_WhenUniqueConstraintViolation_ShouldSkip()
    {
        var userId = Guid.NewGuid();
        var integrationEvent = new SubscriptionRenewedIntegrationEvent("Payments", Guid.NewGuid(), userId, DateTime.UtcNow.AddYears(1));
        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _usersModuleApiMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleUserDto?>.Success(new ModuleUserDto(userId, "john_doe", "john@example.com", "John", "Doe", "John Doe")));
        _outboxRepositoryMock.Setup(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("unique", new PostgresException("Unique constraint violation", "ERROR", "ERROR", "23505")));

        await _handler.HandleAsync(integrationEvent);

        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenOtherException_ShouldRethrow()
    {
        var userId = Guid.NewGuid();
        var integrationEvent = new SubscriptionRenewedIntegrationEvent("Payments", Guid.NewGuid(), userId, DateTime.UtcNow.AddYears(1));
        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _usersModuleApiMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleUserDto?>.Success(new ModuleUserDto(userId, "john_doe", "john@example.com", "John", "Doe", "John Doe")));
        _outboxRepositoryMock.Setup(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Network error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(integrationEvent));
    }
}
