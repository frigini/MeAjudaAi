using MeAjudaAi.Modules.Communications.Application.Handlers;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers;

public class ProviderAwaitingVerificationIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<ProviderAwaitingVerificationIntegrationEventHandler>> _loggerMock;
    private readonly ProviderAwaitingVerificationIntegrationEventHandler _handler;

    public ProviderAwaitingVerificationIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<ProviderAwaitingVerificationIntegrationEventHandler>>();
        _handler = new ProviderAwaitingVerificationIntegrationEventHandler(
            _outboxRepositoryMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderAwaiting_ShouldEnqueueAdminNotification()
    {
        // Arrange
        var integrationEvent = new ProviderAwaitingVerificationIntegrationEvent(
            "Providers",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "New Provider");

        _configurationMock.Setup(x => x["Communications:AdminEmail"]).Returns("admin@test.com");

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
