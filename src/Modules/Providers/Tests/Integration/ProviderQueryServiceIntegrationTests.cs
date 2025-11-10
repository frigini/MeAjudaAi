using MeAjudaAi.Modules.Providers.Application.Services.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Tests.Integration;

/// <summary>
/// Testes de integração para ProviderQueryService.
/// Valida consultas complexas e paginação com dados reais no banco.
/// </summary>
public sealed class ProviderQueryServiceIntegrationTests : ProvidersIntegrationTestBase
{
    [Fact]
    public async Task GetProvidersAsync_WithValidFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        await CleanupDatabase(); // Garantir isolamento
        var queryService = GetService<IProviderQueryService>();

        // Criar dados de teste
        var businessProfile1 = CreateTestBusinessProfile("provider1@test.com");
        var businessProfile2 = CreateTestBusinessProfile("provider2@test.com");

        var provider1 = await CreateProviderAsync(
            Guid.NewGuid(),
            "Test Provider 1",
            EProviderType.Individual,
            businessProfile1);

        var provider2 = await CreateProviderAsync(
            Guid.NewGuid(),
            "Test Provider 2",
            EProviderType.Company,
            businessProfile2);

        // Act
        var result = await queryService.GetProvidersAsync(
            page: 1,
            pageSize: 10,
            nameFilter: "Test Provider");

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().Contain(p => p.Name == "Test Provider 1");
        result.Items.Should().Contain(p => p.Name == "Test Provider 2");
    }

    [Fact]
    public async Task GetProvidersAsync_WithTypeFilter_ShouldReturnOnlyMatchingType()
    {
        // Arrange
        await CleanupDatabase();

        var queryService = GetService<IProviderQueryService>();

        var businessProfile = CreateTestBusinessProfile("individual@test.com");
        await CreateProviderAsync(
            Guid.NewGuid(),
            "Individual Provider",
            EProviderType.Individual,
            businessProfile);

        // Act
        var result = await queryService.GetProvidersAsync(
            page: 1,
            pageSize: 10,
            typeFilter: EProviderType.Individual);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Individual Provider");
    }

    [Fact]
    public async Task GetProvidersAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        await CleanupDatabase(); // Garantir isolamento
        var queryService = GetService<IProviderQueryService>();

        // Criar múltiplos providers
        for (int i = 1; i <= 5; i++)
        {
            var businessProfile = CreateTestBusinessProfile($"provider{i}@test.com");
            await CreateProviderAsync(
                Guid.NewGuid(),
                $"Provider {i}",
                EProviderType.Individual,
                businessProfile);
        }

        // Act
        var result = await queryService.GetProvidersAsync(page: 2, pageSize: 2);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetProvidersAsync_EmptyDatabase_ShouldReturnEmptyResult()
    {
        // Arrange
        await ForceCleanDatabase();
        
        var queryService = GetService<IProviderQueryService>();
        
        // Act - teste com banco vazio usando container-backed services
        var resultWithoutFilter = await queryService.GetProvidersAsync(page: 1, pageSize: 10);

        // Assert - o banco deve estar vazio
        resultWithoutFilter.Should().NotBeNull();
        resultWithoutFilter.Items.Should().BeEmpty();
        resultWithoutFilter.TotalCount.Should().Be(0);
        resultWithoutFilter.TotalPages.Should().Be(0);
        
        // Act - Agora com filtro único que não existe
        var uniqueFilter = $"VERY_UNIQUE_TEST_FILTER__{Guid.NewGuid():N}";
        var result = await queryService.GetProvidersAsync(
            page: 1, 
            pageSize: 10,
            nameFilter: uniqueFilter);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    /// <summary>
    /// Limpeza específica do teste (opcional)
    /// </summary>
    private async Task OnTestDisposeAsync()
    {
        await CleanupDatabase();
    }
}
