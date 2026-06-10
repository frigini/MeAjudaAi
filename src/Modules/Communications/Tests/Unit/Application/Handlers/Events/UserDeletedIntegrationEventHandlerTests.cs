using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Users;
using MeAjudaAi.Modules.Communications.Application.Handlers.Events;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers.Events;

public class UserDeletedIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<ICommunicationLogQueries> _logQueriesMock;
    private readonly Mock<IUsersModuleApi> _usersModuleApiMock;
    private readonly Mock<ILogger<UserDeletedIntegrationEventHandler>> _loggerMock;
    private readonly Mock<ISerializer> _serializerMock;
    private readonly UserDeletedIntegrationEventHandler _handler;

    public UserDeletedIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _logQueriesMock = new Mock<ICommunicationLogQueries>();
        _usersModuleApiMock = new Mock<IUsersModuleApi>();
        _loggerMock = new Mock<ILogger<UserDeletedIntegrationEventHandler>>();
        _serializerMock = new Mock<ISerializer>();

        _serializerMock.Setup(x => x.Serialize(It.IsAny<object>())).Returns("{}");

        _handler = new UserDeletedIntegrationEventHandler(
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
        var integrationEvent = new UserDeletedIntegrationEvent("Users", userId, DateTime.UtcNow);
        
        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _usersModuleApiMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto?>.Success(new UserDto(userId, "john_doe", "john@example.com", "John", "Doe", "John Doe", "keycloak-id", DateTime.UtcNow, null)));

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
