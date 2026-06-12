using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers.Integration;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Infrastructure.Events.Handlers.Integration;

public class UserDeletedIntegrationEventHandlerTests
{
    private readonly RatingsDbContext _dbContext;
    private readonly Mock<ILogger<UserDeletedIntegrationEventHandler>> _loggerMock = new();
    private readonly Mock<IIdempotencyRepository> _idempotencyRepositoryMock = new();
    private readonly UserDeletedIntegrationEventHandler _handler;

    public UserDeletedIntegrationEventHandlerTests()
    {
        var options = new DbContextOptionsBuilder<RatingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new RatingsDbContext(options);
        _handler = new UserDeletedIntegrationEventHandler(_dbContext, _idempotencyRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task HandleAsync_WhenReviewsExist_ShouldRemoveAllReviews()
    {
        var userId = Guid.NewGuid();
        _dbContext.Reviews.Add(Review.Create(Guid.NewGuid(), userId, 5, "Good"));
        _dbContext.Reviews.Add(Review.Create(Guid.NewGuid(), userId, 4, "Nice"));
        await _dbContext.SaveChangesAsync();

        var evt = new UserDeletedIntegrationEvent("Users", userId, "user@test.com", "Test", DateTime.UtcNow);

        await _handler.HandleAsync(evt);

        var remainingReviews = await _dbContext.Reviews.CountAsync();
        remainingReviews.Should().Be(0);
        _idempotencyRepositoryMock.Verify(x => x.MarkAsProcessedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task HandleAsync_WhenNoReviewsExist_ShouldStillRecordProcessedEventAndSaveChanges()
    {
        var userId = Guid.NewGuid();
        var evt = new UserDeletedIntegrationEvent("Users", userId, "user@test.com", "Test", DateTime.UtcNow);

        await _handler.HandleAsync(evt);

        _idempotencyRepositoryMock.Verify(x => x.MarkAsProcessedAsync(evt.Id.ToString(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenDatabaseFails_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid(); 
        var evt = new UserDeletedIntegrationEvent("Users", userId, "user@test.com", "Test", DateTime.UtcNow); 

        // Configura DbContext em memória
        var options = new DbContextOptionsBuilder<RatingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new RatingsDbContext(options); 

        // Cria handler
        var handler = new UserDeletedIntegrationEventHandler(context, _idempotencyRepositoryMock.Object, _loggerMock.Object);

        // Descarta contexto para forçar falha no SaveChangesAsync
        context.Dispose(); 

        // Act
        Func<Task> act = () => handler.HandleAsync(evt); 

        // Assert
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }
    }
