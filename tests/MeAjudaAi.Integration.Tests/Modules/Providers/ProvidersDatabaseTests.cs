using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.Providers;

/// <summary>
/// Testes específicos para verificar se o banco de dados do módulo Providers está funcionando
/// </summary>
public class ProvidersDatabaseTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Providers;

    [Fact]
    public async Task ProvidersDbContext_ShouldBeConfiguredCorrectly()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();

        // Act & Assert
        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task ProvidersTable_ShouldExist()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();

        // Act & Assert
        var providersCount = await context.Providers.CountAsync();
        providersCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public Task ProviderQueryService_ShouldBeRegistered()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var queryService = scope.ServiceProvider.GetService<MeAjudaAi.Modules.Providers.Application.Services.Interfaces.IProviderQueryService>();

        // Act & Assert
        queryService.Should().NotBeNull();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task ProviderQueryService_ShouldBeAbleToQuery()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var queryService = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Modules.Providers.Application.Services.Interfaces.IProviderQueryService>();

        // Act
        var result = await queryService.GetProvidersAsync(page: 1, pageSize: 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.TotalItems.Should().BeGreaterThanOrEqualTo(0);
    }
}
