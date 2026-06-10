using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Contracts.Modules.Users;
using MeAjudaAi.Contracts.Modules.Users.DTOs;
using MeAjudaAi.Modules.Communications.Application.Handlers;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Messaging.Messages.Bookings;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers;

public class BookingCreatedIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<ICommunicationLogQueries> _logQueriesMock;
    private readonly Mock<IProvidersModuleApi> _providersModuleApiMock;
    private readonly Mock<IUsersModuleApi> _usersModuleApiMock;
    private readonly Mock<ILogger<BookingCreatedIntegrationEventHandler>> _loggerMock;
    private readonly Mock<ISerializer> _serializerMock;
    private readonly BookingCreatedIntegrationEventHandler _handler;

    public BookingCreatedIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _logQueriesMock = new Mock<ICommunicationLogQueries>();
        _providersModuleApiMock = new Mock<IProvidersModuleApi>();
        _usersModuleApiMock = new Mock<IUsersModuleApi>();
        _loggerMock = new Mock<ILogger<BookingCreatedIntegrationEventHandler>>();
        _serializerMock = new Mock<ISerializer>();

        _handler = new BookingCreatedIntegrationEventHandler(
            _outboxRepositoryMock.Object,
            _logQueriesMock.Object,
            _providersModuleApiMock.Object,
            _usersModuleApiMock.Object,
            _serializerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidEvent_ShouldEnqueueOutboxMessages()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var integrationEvent = new BookingCreatedIntegrationEvent(
            "Bookings", bookingId, providerId, clientId, Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow));

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _providersModuleApiMock.Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(new ModuleProviderDto(
                providerId, "Provider", "slug", "provider@test.com", "123", "Individual", "Active", DateTime.UtcNow, DateTime.UtcNow, true)));

        _usersModuleApiMock.Setup(x => x.GetUserByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleUserDto?>.Success(new ModuleUserDto(
                clientId, "Username", "client@test.com", "Client", "Last", "Client Last")));

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenEventAlreadyProcessed_ShouldReturnEarly()
    {
        // Arrange
        var integrationEvent = new BookingCreatedIntegrationEvent("Bookings", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow));

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderApiFails_ShouldLogWarningAndReturn()
    {
        // Arrange
        var integrationEvent = new BookingCreatedIntegrationEvent("Bookings", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow));

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _providersModuleApiMock.Setup(x => x.GetProviderByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Failure("Not Found"));

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _loggerMock.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }
}

