using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Users;
using MeAjudaAi.Contracts.Modules.Users.DTOs;
using MeAjudaAi.Modules.Communications.Application.Handlers.Events;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers.Events;

public class SubscriptionCanceledIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<ICommunicationLogQueries> _logQueriesMock;
    private readonly Mock<IUsersModuleApi> _usersModuleApiMock;
    private readonly Mock<ILogger<SubscriptionCanceledIntegrationEventHandler>> _loggerMock;
    private readonly Mock<ISerializer> _serializerMock;
    private readonly SubscriptionCanceledIntegrationEventHandler _handler;

    public SubscriptionCanceledIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _logQueriesMock = new Mock<ICommunicationLogQueries>();
        _usersModuleApiMock = new Mock<IUsersModuleApi>();
        _loggerMock = new Mock<ILogger<SubscriptionCanceledIntegrationEventHandler>>();
        _serializerMock = new Mock<ISerializer>();

        _serializerMock.Setup(x => x.Serialize(It.IsAny<object>())).Returns("{}");

        _handler = new SubscriptionCanceledIntegrationEventHandler(
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
        var integrationEvent = new SubscriptionCanceledIntegrationEvent("Payments", subscriptionId, userId);
        
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
}
