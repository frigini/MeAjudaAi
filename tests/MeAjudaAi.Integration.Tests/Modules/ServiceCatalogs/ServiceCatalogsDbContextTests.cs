using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Integration.Tests.Modules.ServiceCatalogs;

/// <summary>
/// Testes de integração para o DbContext do módulo Catalogs.
/// Valida configurações do EF Core, relacionamentos e constraints.
/// </summary>
public class ServiceCatalogsDbContextTests : BaseApiTest
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
