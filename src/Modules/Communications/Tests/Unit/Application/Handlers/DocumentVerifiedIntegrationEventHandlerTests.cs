using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Modules.Communications.Application.Handlers;
using OutboxMessage = MeAjudaAi.Modules.Communications.Domain.Entities.OutboxMessage;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Messaging.Messages.Documents;
using MeAjudaAi.Shared.Database.Exceptions;
using MeAjudaAi.Shared.Database.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers;

public class DocumentVerifiedIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<IProvidersModuleApi> _providersModuleApiMock;
    private readonly Mock<ILogger<DocumentVerifiedIntegrationEventHandler>> _loggerMock;
    private readonly DocumentVerifiedIntegrationEventHandler _handler;

    public DocumentVerifiedIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _providersModuleApiMock = new Mock<IProvidersModuleApi>();
        _loggerMock = new Mock<ILogger<DocumentVerifiedIntegrationEventHandler>>();
        
        _handler = new DocumentVerifiedIntegrationEventHandler(
            _outboxRepositoryMock.Object,
            _providersModuleApiMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidEvent_ShouldEnqueueOutboxMessage()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new DocumentVerifiedIntegrationEvent(
            "Documents", Guid.NewGuid(), providerId, "Identity", true, DateTime.UtcNow);

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
        var integrationEvent = new DocumentVerifiedIntegrationEvent(
            "Documents", Guid.NewGuid(), providerId, "Identity", true, DateTime.UtcNow);

        _providersModuleApiMock.Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(null));

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderApiFails_ShouldThrowException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new DocumentVerifiedIntegrationEvent(
            "Documents", Guid.NewGuid(), providerId, "Identity", true, DateTime.UtcNow);

        _providersModuleApiMock.Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Failure(new MeAjudaAi.Contracts.Functional.Error("Failed to fetch provider", 500)));

        // Act
        var act = () => _handler.HandleAsync(integrationEvent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to fetch provider*");
    }

    [Fact]
    public async Task HandleAsync_WhenDuplicateEvent_ShouldHandleUniqueConstraintException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new DocumentVerifiedIntegrationEvent(
            "Documents", Guid.NewGuid(), providerId, "Identity", true, DateTime.UtcNow);

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

        // Como o PostgreSqlExceptionProcessor é estático e o PostgresException do Npgsql 
        // é difícil de instanciar manualmente com detalhes via reflexão,
        // vamos simular que o repositório lance diretamente a exceção processada
        // ou que o handler a receba após o processamento (se o repositório já usasse o processador).
        
        // No entanto, o handler chama o processador explicitamente.
        // Para este teste unitário passar e validar a lógica do catch,
        // vamos lançar uma UniqueConstraintException customizada que herda de DbUpdateException.
        
        var uniqueException = new UniqueConstraintException(
            "ix_outbox_messages_correlation_id", 
            "correlation_id", 
            new Exception("Simulated inner"));

        _outboxRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(uniqueException);

        // Act
        var act = () => _handler.HandleAsync(integrationEvent);

        // Assert
        await act.Should().NotThrowAsync();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
