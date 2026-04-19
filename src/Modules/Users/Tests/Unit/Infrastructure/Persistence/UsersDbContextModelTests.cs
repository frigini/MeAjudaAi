using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Persistence;

public class UsersDbContextModelTests
{
    [Fact]
    public void OnModelCreating_ShouldConfigureModelCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName: "UsersTestDb_" + Guid.NewGuid())
            .Options;
        var domainEventProcessorMock = new Mock<IDomainEventProcessor>();

        // Act
        using var context = new UsersDbContext(options, domainEventProcessorMock.Object);
        var model = context.Model;

        // Assert
        model.Should().NotBeNull();
        model.GetDefaultSchema().Should().Be("users");
        
        // Check if entities are registered
        model.FindEntityType(typeof(MeAjudaAi.Modules.Users.Domain.Entities.User)).Should().NotBeNull();
    }
}
