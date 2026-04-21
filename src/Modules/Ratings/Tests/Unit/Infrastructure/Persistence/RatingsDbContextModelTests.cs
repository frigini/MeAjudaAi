using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Infrastructure.Persistence;

public class RatingsDbContextModelTests
{
    [Fact]
    public void OnModelCreating_ShouldConfigureModelCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<RatingsDbContext>()
            .UseInMemoryDatabase(databaseName: "RatingsTestDb_" + Guid.NewGuid())
            .Options;
        var domainEventProcessorMock = new Mock<IDomainEventProcessor>();

        // Act
        using var context = new RatingsDbContext(options, domainEventProcessorMock.Object);
        var model = context.Model;

        // Assert
        model.Should().NotBeNull();
        model.GetDefaultSchema().Should().Be("ratings");
        
        // Check if entities are registered
        model.FindEntityType(typeof(MeAjudaAi.Modules.Ratings.Domain.Entities.Review)).Should().NotBeNull();
    }
}
