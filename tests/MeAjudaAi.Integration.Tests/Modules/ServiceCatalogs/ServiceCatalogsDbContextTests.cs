using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Integration.Tests.Modules.ServiceCatalogs;

/// <summary>
/// Testes de integração para o DbContext do módulo Catalogs.
/// Valida configurações do EF Core, relacionamentos e constraints.
/// </summary>
public class ServiceCatalogsDbContextTests : ApiTestBase
{
    [Fact]
    public void ServiceCatalogsDbContext_ShouldBeRegistered()
    {
        // Arrange & Act
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<ServiceCatalogsDbContext>();

        // Assert
        dbContext.Should().NotBeNull("ServiceCatalogsDbContext should be registered in DI");
    }

    [Fact]
    public async Task ServiceCategories_Table_ShouldExist()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();

        // Act
        var canConnect = await dbContext.Database.CanConnectAsync();

        // Assert
        canConnect.Should().BeTrue("Database should be accessible");

        // Check if we can query the table (will throw if table doesn't exist)
        var count = await dbContext.ServiceCategories.CountAsync();
        count.Should().BeGreaterThanOrEqualTo(0, "ServiceCategories table should exist");
    }

    [Fact]
    public async Task Services_Table_ShouldExist()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();

        // Act
        var canConnect = await dbContext.Database.CanConnectAsync();

        // Assert
        canConnect.Should().BeTrue("Database should be accessible");

        // Check if we can query the table
        var count = await dbContext.Services.CountAsync();
        count.Should().BeGreaterThanOrEqualTo(0, "Services table should exist");
    }

    [Fact]
    public void Services_ShouldHaveForeignKeyToServiceCategories()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();

        // Act
        var serviceEntity = dbContext.Model.FindEntityType(typeof(MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service));
        var foreignKeys = serviceEntity?.GetForeignKeys();

        // Assert
        foreignKeys.Should().NotBeNull();
        foreignKeys.Should().NotBeEmpty("Services table should have foreign key constraint to ServiceCategories");
    }

    [Fact]
    public void CatalogsSchema_ShouldExist()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();

        // Act
        var defaultSchema = dbContext.Model.GetDefaultSchema();

        // Assert
        defaultSchema.Should().Be("catalogs", "Catalogs schema should exist in database");
    }

    [Fact]
    public async Task Database_ShouldAllowBasicOperations()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();

        // Act & Assert - Should be able to execute queries
        var canConnect = await dbContext.Database.CanConnectAsync();
        canConnect.Should().BeTrue("Should be able to connect to database");

        // Should be able to begin transaction
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        transaction.Should().NotBeNull("Should be able to begin transaction");
        await transaction.RollbackAsync();
    }
}
