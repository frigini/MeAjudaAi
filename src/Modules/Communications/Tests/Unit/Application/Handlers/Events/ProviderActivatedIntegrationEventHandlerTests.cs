using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Users;
using MeAjudaAi.Contracts.Modules.Users.DTOs;
using MeAjudaAi.Modules.Communications.Application.Handlers;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers.Events;

public class ProviderActivatedIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<ICommunicationLogQueries> _logQueriesMock;
    private readonly Mock<IUsersModuleApi> _usersModuleApiMock;
    private readonly Mock<ILogger<ProviderActivatedIntegrationEventHandler>> _loggerMock;
    private readonly Mock<ISerializer> _serializerMock;
    private readonly ProviderActivatedIntegrationEventHandler _handler;

    public ProviderActivatedIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _logQueriesMock = new Mock<ICommunicationLogQueries>();
        _usersModuleApiMock = new Mock<IUsersModuleApi>();
        _loggerMock = new Mock<ILogger<ProviderActivatedIntegrationEventHandler>>();
        _serializerMock = new Mock<ISerializer>();

        _serializerMock.Setup(x => x.Serialize(It.IsAny<object>())).Returns("{}");

        _handler = new ProviderActivatedIntegrationEventHandler(
            _outboxRepositoryMock.Object,
            _logQueriesMock.Object,
            _usersModuleApiMock.Object,
            _serializerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenNewActivation_ShouldEnqueueOutboxMessage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var integrationEvent = new ProviderActivatedIntegrationEvent(
            "Providers",
            Guid.NewGuid(),
            userId,
            "Test Provider",
            "Admin",
            DateTime.UtcNow);

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var userDto = new ModuleUserDto(userId, "testuser", "test@test.com", "First", "Last", "Full Name");
        _usersModuleApiMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleUserDto?>.Success(userDto));

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
        var integrationEvent = new ProviderActivatedIntegrationEvent(
            "Providers",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Provider",
            "Admin",
            DateTime.UtcNow);

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
