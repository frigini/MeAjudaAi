using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Modules.Communications.Application.Handlers.Events;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers.Events;

public class ProviderDeletedIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<ICommunicationLogQueries> _logQueriesMock;
    private readonly Mock<IProvidersModuleApi> _providersModuleApiMock;
    private readonly Mock<ILogger<ProviderDeletedIntegrationEventHandler>> _loggerMock;
    private readonly Mock<ISerializer> _serializerMock;
    private readonly ProviderDeletedIntegrationEventHandler _handler;

    public ProviderDeletedIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _logQueriesMock = new Mock<ICommunicationLogQueries>();
        _providersModuleApiMock = new Mock<IProvidersModuleApi>();
        _loggerMock = new Mock<ILogger<ProviderDeletedIntegrationEventHandler>>();
        _serializerMock = new Mock<ISerializer>();

        _serializerMock.Setup(x => x.Serialize(It.IsAny<object>())).Returns("{}");

        _handler = new ProviderDeletedIntegrationEventHandler(
            _outboxRepositoryMock.Object,
            _logQueriesMock.Object,
            _providersModuleApiMock.Object,
            _serializerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidEvent_ShouldEnqueueOutboxMessage()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new ProviderDeletedIntegrationEvent("Providers", providerId, Guid.NewGuid(), "Provider Name", "Reason", DateTime.UtcNow);
        
        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _providersModuleApiMock.Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(new ModuleProviderDto(
                providerId, "Provider Name", "provider-slug", "provider@test.com", "123456789", "Individual", "Verified", DateTime.UtcNow, DateTime.UtcNow, true)));

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
