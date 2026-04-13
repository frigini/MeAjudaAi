using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Modules.Communications.Application.Handlers;
using OutboxMessage = MeAjudaAi.Modules.Communications.Domain.Entities.OutboxMessage;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Messaging.Messages.Documents;
using MeAjudaAi.Shared.Database.Exceptions;
using MeAjudaAi.Shared.Database.Outbox;
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
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderFetchFails_ShouldThrowException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new DocumentRejectedIntegrationEvent(
            "Documents", Guid.NewGuid(), providerId, "Identity", "Invalid photo", DateTime.UtcNow);

        _providersModuleApiMock.Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Failure(new Error("Failed to fetch provider", 500)));

        // Act
        var act = () => _handler.HandleAsync(integrationEvent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to fetch provider*");
        
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderHasNoEmail_ShouldSkipEnqueue()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new DocumentRejectedIntegrationEvent(
            "Documents", Guid.NewGuid(), providerId, "Identity", "Invalid photo", DateTime.UtcNow);

        var providerDto = new ModuleProviderDto(
            providerId,
            "Test Provider",
            "test-provider",
            null!, // No email
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
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenOutboxDuplicateExists_ShouldNotCreateDuplicate()
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

        // Simular exceção de duplicidade
        _outboxRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UniqueConstraintException(OutboxMessageConstraints.CorrelationIdIndexName, "correlation_id", new Exception()));

        // Act
        var act = () => _handler.HandleAsync(integrationEvent);

        // Assert
        await act.Should().NotThrowAsync();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
