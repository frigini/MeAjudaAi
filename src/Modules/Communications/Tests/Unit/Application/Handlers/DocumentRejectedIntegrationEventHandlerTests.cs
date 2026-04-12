using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Modules.Communications.Application.Handlers;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Messaging.Messages.Documents;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers;

public class DocumentRejectedIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<IProvidersModuleApi> _providersModuleApiMock;
    private readonly Mock<ILogger<DocumentRejectedIntegrationEventHandler>> _loggerMock;
    private readonly DocumentRejectedIntegrationEventHandler _handler;

    public DocumentRejectedIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _providersModuleApiMock = new Mock<IProvidersModuleApi>();
        _loggerMock = new Mock<ILogger<DocumentRejectedIntegrationEventHandler>>();
        
        _handler = new DocumentRejectedIntegrationEventHandler(
            _outboxRepositoryMock.Object,
            _providersModuleApiMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidEvent_ShouldEnqueueOutboxMessage()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new DocumentRejectedIntegrationEvent(
            "Documents", Guid.NewGuid(), providerId, "Identity", "Invalid photo", DateTime.UtcNow);

        var providerDto = new ModuleProviderDto(
            providerId,
            "Test Provider",
            "test-provider",
            "provider@test.com",
            "123456789",
            "Individual",
            "Verified",
            DateTime.UtcNow,
            DateTime.UtcNow,
            true);

        _providersModuleApiMock.Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(providerDto));

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldSkip()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new DocumentRejectedIntegrationEvent(
            "Documents", Guid.NewGuid(), providerId, "Identity", "Invalid photo", DateTime.UtcNow);

        _providersModuleApiMock.Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(null));

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
