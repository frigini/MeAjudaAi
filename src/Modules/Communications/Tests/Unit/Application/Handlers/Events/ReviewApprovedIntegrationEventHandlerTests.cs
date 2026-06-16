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
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers.Events;

[Trait("Category", "Unit")]
[Trait("Module", "Communications")]
[Trait("Layer", "Application")]
public class ReviewApprovedIntegrationEventHandlerTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<ICommunicationLogQueries> _logQueriesMock;
    private readonly Mock<IProvidersModuleApi> _providersModuleApiMock;
    private readonly Mock<ISerializer> _serializerMock;
    private readonly Mock<ILogger<ReviewApprovedIntegrationEventHandler>> _loggerMock;
    private readonly ReviewApprovedIntegrationEventHandler _handler;

    private static ModuleProviderDto MakeProvider(Guid providerId) =>
        new(providerId, "Provider Name", "provider-slug", "provider@test.com",
            "12345678900", "Individual", "Active", DateTime.UtcNow, DateTime.UtcNow, true);


    public ReviewApprovedIntegrationEventHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _logQueriesMock = new Mock<ICommunicationLogQueries>();
        _providersModuleApiMock = new Mock<IProvidersModuleApi>();
        _serializerMock = new Mock<ISerializer>();
        _loggerMock = new Mock<ILogger<ReviewApprovedIntegrationEventHandler>>();

        _serializerMock.Setup(x => x.Serialize(It.IsAny<object>())).Returns("{}");

        _handler = new ReviewApprovedIntegrationEventHandler(
            _outboxRepositoryMock.Object,
            _logQueriesMock.Object,
            _providersModuleApiMock.Object,
            _serializerMock.Object,
            _loggerMock.Object);
    }

    private static ReviewApprovedIntegrationEvent MakeEvent(Guid? providerId = null, Guid? reviewId = null) =>
        new(
            Source: "Ratings",
            ProviderId: providerId ?? Guid.NewGuid(),
            ReviewId: reviewId ?? Guid.NewGuid(),
            NewAverageRating: 4.8m,
            TotalReviews: 15,
            ReviewRating: 5,
            ReviewComment: "Excellent!",
            CreatedAt: DateTime.UtcNow);

    [Fact]
    public async Task HandleAsync_WhenValidEvent_ShouldEnqueueOutboxMessage()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = MakeEvent(providerId: providerId);

        _logQueriesMock
            .Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _providersModuleApiMock
            .Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(MakeProvider(providerId)));

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
        var integrationEvent = MakeEvent();

        _logQueriesMock
            .Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _providersModuleApiMock.Verify(x => x.GetProviderByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldLogWarningAndReturn()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = MakeEvent(providerId: providerId);

        _logQueriesMock
            .Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _providersModuleApiMock
            .Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Failure(Error.NotFound("Provider not found")));

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderApiReturnsTransientError_ShouldThrow()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = MakeEvent(providerId: providerId);

        _logQueriesMock
            .Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _providersModuleApiMock
            .Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Failure(Error.Internal("Service unavailable")));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(integrationEvent));

        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUniqueConstraintViolation_ShouldSkipAndNotThrow()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = MakeEvent(providerId: providerId);

        _logQueriesMock
            .Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _providersModuleApiMock
            .Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(MakeProvider(providerId)));

        _outboxRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UniqueConstraintException("ix_outbox_messages_correlation_id", "correlation_id", new Exception()));

        // Act — should not throw
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenOutboxThrowsUnexpectedException_ShouldRethrow()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = MakeEvent(providerId: providerId);

        _logQueriesMock
            .Setup(x => x.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _providersModuleApiMock
            .Setup(x => x.GetProviderByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(MakeProvider(providerId)));

        _outboxRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected DB error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(integrationEvent));
    }
}
