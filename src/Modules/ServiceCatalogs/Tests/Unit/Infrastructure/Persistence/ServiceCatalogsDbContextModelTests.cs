using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Infrastructure.Persistence;

public class ServiceCatalogsDbContextModelTests
{
    [Fact]
    public void OnModelCreating_ShouldConfigureModelCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ServiceCatalogsDbContext>()
            .UseInMemoryDatabase(databaseName: "ServiceCatalogsTestDb_" + Guid.NewGuid())
            .Options;

        // Act
        using var context = new ServiceCatalogsDbContext(options);
        var model = context.Model;

        // Assert
        model.Should().NotBeNull();
        model.GetDefaultSchema().Should().Be("service_catalogs");
        
        // Check if entities are registered
        var categoryType = model.FindEntityType(typeof(MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory));
        categoryType.Should().NotBeNull();
        categoryType!.GetSchema().Should().Be("service_catalogs");

        var serviceType = model.FindEntityType(typeof(MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service));
        serviceType.Should().NotBeNull();
        serviceType!.GetSchema().Should().Be("service_catalogs");
    }
}
