using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Providers;

/// <summary>
/// Testes de integração para as tabelas e DbContext do módulo Providers.
/// </summary>
public class ProvidersDatabaseTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Providers;

    [Fact]
    public async Task Database_ShouldHaveCorrectSchema()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>();

        // Act & Assert
        var entityTypes = dbContext.Model.GetEntityTypes();
        var providerEntity = entityTypes.FirstOrDefault(e => e.ClrType == typeof(MeAjudaAi.Modules.Providers.Domain.Entities.Provider));
        providerEntity.Should().NotBeNull("Provider entity should be present in the EF model");

        var canConnect = await dbContext.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task ProvidersTable_ShouldExist()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>();

        // Act & Assert
        var entityTypes = dbContext.Model.GetEntityTypes();
        var providerEntity = entityTypes.FirstOrDefault(e => e.Name == "Providers" || e.ClrType == typeof(MeAjudaAi.Modules.Providers.Domain.Entities.Provider));
        providerEntity.Should().NotBeNull("Providers entity should be mapped");

        var canQuery = await dbContext.Providers.Take(0).ToListAsync(CancellationToken.None);
        canQuery.Should().NotBeNull("Should be able to query Providers table");
    }

    [Fact]
    public async Task ProviderQueries_ShouldBeRegistered()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var queryService = scope.ServiceProvider.GetService<IProviderQueries>();

        // Act & Assert
        queryService.Should().NotBeNull();
    }

    [Fact]
    public async Task ProviderQueries_ShouldBeAbleToQuery()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var queryService = scope.ServiceProvider.GetRequiredService<IProviderQueries>();

        // Act
        var result = await queryService.GetPagedAsync(1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.TotalItems.Should().BeGreaterThanOrEqualTo(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
}
