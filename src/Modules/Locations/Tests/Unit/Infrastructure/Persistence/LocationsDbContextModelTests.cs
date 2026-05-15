using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Infrastructure.Persistence;

public class LocationsDbContextModelTests
{
    [Fact]
    public void OnModelCreating_ShouldConfigureModelCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LocationsDbContext>()
            .UseInMemoryDatabase(databaseName: "LocationsTestDb_" + Guid.NewGuid())
            .Options;
        var domainEventProcessorMock = new Mock<IDomainEventProcessor>();

        // Act
        using var context = new LocationsDbContext(options, domainEventProcessorMock.Object);
        var model = context.Model;

        // Assert
        model.Should().NotBeNull();
        model.GetDefaultSchema().Should().Be("locations");
        
        // Check if entities are registered
        model.FindEntityType(typeof(MeAjudaAi.Modules.Locations.Domain.Entities.AllowedCity)).Should().NotBeNull();
    }

    [Fact]
    public void GetRepository_WithSupportedType_ShouldReturnRepository()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LocationsDbContext>()
            .UseInMemoryDatabase("GetRepository_HappyPath_" + Guid.NewGuid())
            .Options;
        var domainEventProcessorMock = new Mock<IDomainEventProcessor>();

        // Act
        using var context = new LocationsDbContext(options, domainEventProcessorMock.Object);

        var repository = context.GetRepository<MeAjudaAi.Modules.Locations.Domain.Entities.AllowedCity, Guid>();

        // Assert
        repository.Should().NotBeNull();
        repository.Should().BeAssignableTo<IRepository<MeAjudaAi.Modules.Locations.Domain.Entities.AllowedCity, Guid>>();
    }

    [Fact]
    public void GetRepository_WithUnsupportedType_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LocationsDbContext>()
            .UseInMemoryDatabase("GetRepository_Error_" + Guid.NewGuid())
            .Options;
        var domainEventProcessorMock = new Mock<IDomainEventProcessor>();

        // Act
        using var context = new LocationsDbContext(options, domainEventProcessorMock.Object);

        var act = () => context.GetRepository<SomeUnsupportedEntity, int>();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*LocationsDbContext does not implement*");
    }

    private class SomeUnsupportedEntity { }
}
