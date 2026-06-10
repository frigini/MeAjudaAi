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

public class BookingCancelledIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<ICommunicationLogQueries> _logQueriesMock;
    private readonly Mock<IProvidersModuleApi> _providersModuleApiMock;
    private readonly Mock<IUsersModuleApi> _usersModuleApiMock;
    private readonly Mock<ILogger<BookingCancelledIntegrationEventHandler>> _loggerMock;
    private readonly Mock<ISerializer> _serializerMock;
    private readonly BookingCancelledIntegrationEventHandler _handler;

    public BookingCancelledIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _logQueriesMock = new Mock<ICommunicationLogQueries>();
        _providersModuleApiMock = new Mock<IProvidersModuleApi>();
        _usersModuleApiMock = new Mock<IUsersModuleApi>();
        _loggerMock = new Mock<ILogger<BookingCancelledIntegrationEventHandler>>();
        _serializerMock = new Mock<ISerializer>();

        _handler = new BookingCancelledIntegrationEventHandler(
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
        var integrationEvent = new BookingCancelledIntegrationEvent("Bookings", bookingId, providerId, clientId, "Reason");

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
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.Is<OutboxMessage>(m => 
            m.CorrelationId.Contains(":provider") && 
            m.Payload.Contains("To") && 
            m.Payload.Contains("booking_cancelled")), It.IsAny<CancellationToken>()), Times.Once);
        
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.Is<OutboxMessage>(m => 
            m.CorrelationId.Contains(":client") && 
            m.Payload.Contains("To") && 
            m.Payload.Contains("booking_cancelled")), It.IsAny<CancellationToken>()), Times.Once);

        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

