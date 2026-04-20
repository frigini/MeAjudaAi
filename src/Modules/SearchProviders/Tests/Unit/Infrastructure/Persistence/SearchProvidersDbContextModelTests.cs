using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Infrastructure.Persistence;

public class SearchProvidersDbContextModelTests
{
    [Fact]
    public void OnModelCreating_ShouldConfigureModelCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<SearchProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: "SearchProvidersTestDb_" + Guid.NewGuid())
            .Options;
        var domainEventProcessorMock = new Mock<IDomainEventProcessor>();

        // Act
        using var context = new SearchProvidersDbContext(options, domainEventProcessorMock.Object);
        var model = context.Model;

        // Assert
        model.Should().NotBeNull();
        model.GetDefaultSchema().Should().Be("search_providers");
        
        // Check if entities are registered
        model.FindEntityType(typeof(MeAjudaAi.Modules.SearchProviders.Domain.Entities.SearchableProvider)).Should().NotBeNull();
    }
}
