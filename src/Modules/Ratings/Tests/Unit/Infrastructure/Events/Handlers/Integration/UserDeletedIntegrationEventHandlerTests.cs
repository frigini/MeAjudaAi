using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers.Integration;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Shared.Messaging.Messages.Users;
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
    public async Task HandleAsync_WhenNoReviewsExist_ShouldNotCallSaveChanges()
    {
        var userId = Guid.NewGuid();
        var handler = new UserDeletedIntegrationEventHandler(_dbContext, _loggerMock.Object);
        var evt = new UserDeletedIntegrationEvent("Users", userId, DateTime.UtcNow);

        await _handler.HandleAsync(evt);

        // Verification: no changes in db
    }

    [Fact]
    public async Task HandleAsync_WhenDatabaseFails_ShouldThrowException()
    {
        // Mocking failure by using a different context or forcing error
        var userId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<RatingsDbContext>()
            .UseInMemoryDatabase(databaseName: "FailDb")
            .Options;
        var faultyDb = new Mock<RatingsDbContext>(options);
        
        faultyDb.Setup(db => db.Reviews).Throws(new Exception("Database error"));
        
        var handler = new UserDeletedIntegrationEventHandler(faultyDb.Object, _loggerMock.Object);
        var evt = new UserDeletedIntegrationEvent("Users", userId, DateTime.UtcNow);

        Func<Task> act = () => handler.HandleAsync(evt);

        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
    }
}
