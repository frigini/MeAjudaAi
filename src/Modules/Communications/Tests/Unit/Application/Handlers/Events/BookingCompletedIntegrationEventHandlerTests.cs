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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers.Events;

public class BookingCompletedIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<ICommunicationLogQueries> _logQueriesMock;
    private readonly Mock<IProvidersModuleApi> _providersModuleApiMock;
    private readonly Mock<IUsersModuleApi> _usersModuleApiMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<BookingCompletedIntegrationEventHandler>> _loggerMock;
    private readonly Mock<ISerializer> _serializerMock;
    private readonly BookingCompletedIntegrationEventHandler _handler;

    public BookingCompletedIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _logQueriesMock = new Mock<ICommunicationLogQueries>();
        _providersModuleApiMock = new Mock<IProvidersModuleApi>();
        _usersModuleApiMock = new Mock<IUsersModuleApi>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<BookingCompletedIntegrationEventHandler>>();
        _serializerMock = new Mock<ISerializer>();

        _serializerMock.Setup(x => x.Serialize(It.IsAny<object>())).Returns("{}");
        _configurationMock.Setup(x => x["ClientBaseUrl"]).Returns("http://localhost:5165");

        _handler = new BookingCompletedIntegrationEventHandler(
            _outboxRepositoryMock.Object,
            _logQueriesMock.Object,
            _providersModuleApiMock.Object,
            _usersModuleApiMock.Object,
            _configurationMock.Object,
            _serializerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidEvent_ShouldEnqueueOutboxMessage()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var integrationEvent = new BookingCompletedIntegrationEvent("Bookings", bookingId, providerId, clientId);

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
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
