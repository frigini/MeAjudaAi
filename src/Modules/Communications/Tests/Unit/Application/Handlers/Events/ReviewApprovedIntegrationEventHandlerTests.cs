using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Modules.Communications.Application.Handlers.Events;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Database.Exceptions;
using MeAjudaAi.Shared.Messaging.Messages.Ratings;
using MeAjudaAi.Shared.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers.Events;

public class ReviewApprovedIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<ICommunicationLogQueries> _logQueriesMock;
    private readonly Mock<IProvidersModuleApi> _providersModuleApiMock;
    private readonly Mock<ILogger<ReviewApprovedIntegrationEventHandler>> _loggerMock;
    private readonly Mock<ISerializer> _serializerMock;
    private readonly ReviewApprovedIntegrationEventHandler _handler;

    public ReviewApprovedIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _logQueriesMock = new Mock<ICommunicationLogQueries>();
        _providersModuleApiMock = new Mock<IProvidersModuleApi>();
        _loggerMock = new Mock<ILogger<ReviewApprovedIntegrationEventHandler>>();
        _serializerMock = new Mock<ISerializer>();

        _serializerMock.Setup(x => x.Serialize(It.IsAny<object>())).Returns("{}");

        _handler = new ReviewApprovedIntegrationEventHandler(
            _outboxRepositoryMock.Object,
            _logQueriesMock.Object,
            _providersModuleApiMock.Object,
            _serializerMock.Object,
            _loggerMock.Object);
    }

    private static ModuleProviderDto CreateProviderDto(Guid providerId) =>
        new(providerId, "Provider", "provider-slug", "provider@test.com", "123", "Individual",
            "Verified", DateTime.UtcNow, DateTime.UtcNow, true);

    [Fact]
    public async Task HandleAsync_WhenValidEvent_ShouldEnqueueOutboxMessage()
    {
        var providerId = Guid.NewGuid();
        var integrationEvent = new ReviewApprovedIntegrationEvent("Ratings", providerId, Guid.NewGuid(), 4.5m, 10, 5, "Great", DateTime.UtcNow);

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _providersModuleApiMock.Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(CreateProviderDto(providerId)));

        await _handler.HandleAsync(integrationEvent);

        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCorrelationAlreadyExists_ShouldSkip()
    {
        var integrationEvent = new ReviewApprovedIntegrationEvent("Ratings", Guid.NewGuid(), Guid.NewGuid(), 4.5m, 10, 5, null, DateTime.UtcNow);

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _handler.HandleAsync(integrationEvent);

        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldSkip()
    {
        var providerId = Guid.NewGuid();
        var integrationEvent = new ReviewApprovedIntegrationEvent("Ratings", providerId, Guid.NewGuid(), 4.5m, 10, 5, null, DateTime.UtcNow);

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _providersModuleApiMock.Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Failure(new Error("Not found", StatusCodes.Status404NotFound)));

        await _handler.HandleAsync(integrationEvent);

        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderApiFails_ShouldReturnEarly()
    {
        var providerId = Guid.NewGuid();
        var integrationEvent = new ReviewApprovedIntegrationEvent("Ratings", providerId, Guid.NewGuid(), 4.5m, 10, 5, null, DateTime.UtcNow);

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _providersModuleApiMock.Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Failure(new Error("Timeout", StatusCodes.Status503ServiceUnavailable)));

        await _handler.HandleAsync(integrationEvent);

        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUniqueConstraintViolation_ShouldSkip()
    {
        var providerId = Guid.NewGuid();
        var integrationEvent = new ReviewApprovedIntegrationEvent("Ratings", providerId, Guid.NewGuid(), 4.5m, 10, 5, null, DateTime.UtcNow);

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _providersModuleApiMock.Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(CreateProviderDto(providerId)));
        _outboxRepositoryMock.Setup(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("unique", new PostgresException("Unique constraint violation", "ERROR", "ERROR", "23505")));

        await _handler.HandleAsync(integrationEvent);

        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenOtherException_ShouldRethrow()
    {
        var providerId = Guid.NewGuid();
        var integrationEvent = new ReviewApprovedIntegrationEvent("Ratings", providerId, Guid.NewGuid(), 4.5m, 10, 5, null, DateTime.UtcNow);

        _logQueriesMock.Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _providersModuleApiMock.Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(CreateProviderDto(providerId)));
        _outboxRepositoryMock.Setup(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Network error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(integrationEvent));
    }
}
