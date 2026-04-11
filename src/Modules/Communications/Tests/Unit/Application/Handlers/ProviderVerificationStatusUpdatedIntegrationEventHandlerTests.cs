using MeAjudaAi.Modules.Communications.Application.Handlers;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Contracts.Modules.Users;
using MeAjudaAi.Contracts.Modules.Users.DTOs;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers;

public class ProviderVerificationStatusUpdatedIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<IUsersModuleApi> _usersModuleApiMock;
    private readonly Mock<ILogger<ProviderVerificationStatusUpdatedIntegrationEventHandler>> _loggerMock;
    private readonly ProviderVerificationStatusUpdatedIntegrationEventHandler _handler;

    public ProviderVerificationStatusUpdatedIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _usersModuleApiMock = new Mock<IUsersModuleApi>();
        _loggerMock = new Mock<ILogger<ProviderVerificationStatusUpdatedIntegrationEventHandler>>();
        _handler = new ProviderVerificationStatusUpdatedIntegrationEventHandler(
            _outboxRepositoryMock.Object,
            _usersModuleApiMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenStatusUpdated_ShouldEnqueueNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var integrationEvent = new ProviderVerificationStatusUpdatedIntegrationEvent(
            "Providers",
            Guid.NewGuid(),
            userId,
            "Test Provider",
            "Pending",
            "Verified",
            "Admin",
            "Looks good");

        var userDto = new ModuleUserDto(userId, "testuser", "test@test.com", "John", "Doe", "John Doe");
        _usersModuleApiMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleUserDto?>.Success(userDto));

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ShouldSkip()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var integrationEvent = new ProviderVerificationStatusUpdatedIntegrationEvent(
            "Providers",
            Guid.NewGuid(),
            userId,
            "Test Provider",
            "Pending",
            "Verified");

        _usersModuleApiMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleUserDto?>.Failure("Not found"));

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
