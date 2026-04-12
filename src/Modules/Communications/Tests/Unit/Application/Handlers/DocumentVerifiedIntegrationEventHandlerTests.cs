using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Modules.Communications.Application.Handlers;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Messaging.Messages.Documents;
using MeAjudaAi.Shared.Database.Exceptions;
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

        // Simular exceção de constraint única no SaveChangesAsync
        // Nota: O teste real do PostgreSqlExceptionProcessor é feito em testes de infra,
        // aqui simulamos que a exceção lançada pelo EF é uma DbUpdateException 
        // que contém a UniqueConstraintException internamente ou simulamos o comportamento direto
        // se o handler chamar o processador.
        
        // No handler, ele chama PostgreSqlExceptionProcessor.ProcessException(dbEx)
        // Para simular isso, precisamos que a DbUpdateException tenha uma InnerException que o Npgsql entenda,
        // ou que o processador retorne a UniqueConstraintException.
        
        // Como o PostgreSqlExceptionProcessor é estático, é difícil de mockar.
        // Vou criar uma DbUpdateException com uma InnerException (PostgresException) que resulte em UniqueConstraintException.
        
        var postgresException = (Npgsql.PostgresException)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Npgsql.PostgresException));
        // Definir campos via reflexão se necessário, mas o processador usa SqlState "23505"
        var sqlStateField = typeof(Npgsql.PostgresException).GetField("_sqlState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) 
                            ?? typeof(Npgsql.PostgresException).GetField("sqlState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        sqlStateField?.SetValue(postgresException, "23505");
        
        var constraintNameField = typeof(Npgsql.PostgresException).GetField("_constraintName", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                                  ?? typeof(Npgsql.PostgresException).GetField("constraintName", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        constraintNameField?.SetValue(postgresException, "ix_outbox_messages_correlation_id");

        var dbUpdateException = new DbUpdateException("Duplicate", postgresException);

        _outboxRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbUpdateException);

        // Act
        var act = () => _handler.HandleAsync(integrationEvent);

        // Assert
        await act.Should().NotThrowAsync();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
