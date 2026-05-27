using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
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
        var tableExists = await dbContext.Database.CanConnectAsync();
        tableExists.Should().BeTrue();
    }

    [Fact]
    public async Task ProviderQueryService_ShouldBeRegistered()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var queryService = scope.ServiceProvider.GetService<IProviderQueries>();

        // Act & Assert
        queryService.Should().NotBeNull();
    }

    [Fact]
    public async Task ProviderQueryService_ShouldBeAbleToQuery()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var queryService = scope.ServiceProvider.GetRequiredService<IProviderQueries>();

        // Act
        var result = await queryService.GetPagedAsync(1, 10);

        // Assert
        result.Should().NotBeNull();
    }
}
