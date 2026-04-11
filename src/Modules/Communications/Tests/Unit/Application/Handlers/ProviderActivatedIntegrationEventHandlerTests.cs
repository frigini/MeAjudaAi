using MeAjudaAi.Modules.Communications.Application.Handlers;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers;

public class ProviderActivatedIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<ICommunicationLogRepository> _logRepositoryMock;
    private readonly Mock<ILogger<ProviderActivatedIntegrationEventHandler>> _loggerMock;
    private readonly ProviderActivatedIntegrationEventHandler _handler;

    public ProviderActivatedIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _logRepositoryMock = new Mock<ICommunicationLogRepository>();
        _loggerMock = new Mock<ILogger<ProviderActivatedIntegrationEventHandler>>();
        _handler = new ProviderActivatedIntegrationEventHandler(
            _outboxRepositoryMock.Object,
            _logRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenNewActivation_ShouldEnqueueOutboxMessage()
    {
        // Arrange
        var integrationEvent = new ProviderActivatedIntegrationEvent(
            "Providers",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Provider",
            "Admin",
            DateTime.UtcNow);

        _logRepositoryMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

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

        _logRepositoryMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
