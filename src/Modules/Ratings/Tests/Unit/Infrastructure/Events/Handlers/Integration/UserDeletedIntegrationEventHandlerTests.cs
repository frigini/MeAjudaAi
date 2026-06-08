using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers.Integration;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using System.Linq.Expressions;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Infrastructure.Events.Handlers.Integration;

public class UserDeletedIntegrationEventHandlerTests
{
    private readonly RatingsDbContext _dbContext;
    private readonly Mock<ILogger<UserDeletedIntegrationEventHandler>> _loggerMock = new();
    private readonly UserDeletedIntegrationEventHandler _handler;

    public UserDeletedIntegrationEventHandlerTests()
    {
        var options = new DbContextOptionsBuilder<RatingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new RatingsDbContext(options);
        _handler = new UserDeletedIntegrationEventHandler(_dbContext, _loggerMock.Object);
    }
    
    [Fact]
    public async Task HandleAsync_WhenReviewsExist_ShouldRemoveAllReviews()
    {
        var userId = Guid.NewGuid();
        _dbContext.Reviews.Add(Review.Create(Guid.NewGuid(), userId, 5, "Good"));
        _dbContext.Reviews.Add(Review.Create(Guid.NewGuid(), userId, 4, "Nice"));
        await _dbContext.SaveChangesAsync();

        var evt = new UserDeletedIntegrationEvent("Users", userId, DateTime.UtcNow);

        await _handler.HandleAsync(evt);

        var remainingReviews = await _dbContext.Reviews.CountAsync();
        remainingReviews.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WhenNoReviewsExist_ShouldStillRecordProcessedEventAndSaveChanges()
    {
        var userId = Guid.NewGuid();
        var evt = new UserDeletedIntegrationEvent("Users", userId, DateTime.UtcNow);

        await _handler.HandleAsync(evt);

        var processedEvent = await _dbContext.ProcessedIntegrationEvents.FirstOrDefaultAsync(e => e.CorrelationId == evt.Id.ToString());
        processedEvent.Should().NotBeNull();
        _dbContext.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenDatabaseFails_ShouldThrowException()
    {
        // Forçar falha no SaveChangesAsync
        var userId = Guid.NewGuid();
        var evt = new UserDeletedIntegrationEvent("Users", userId, DateTime.UtcNow);

        var options = new DbContextOptionsBuilder<RatingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Usar um mock para o DbContext para forçar falha no SaveChangesAsync
        var mockContext = new Mock<RatingsDbContext>(options);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var handler = new UserDeletedIntegrationEventHandler(mockContext.Object, _loggerMock.Object);

        Func<Task> act = () => handler.HandleAsync(evt);

        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
    }

}
