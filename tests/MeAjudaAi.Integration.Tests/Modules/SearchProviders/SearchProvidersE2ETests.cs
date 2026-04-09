using MeAjudaAi.Integration.Tests.Base;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.SearchProviders.Application.DTOs;

namespace MeAjudaAi.Integration.Tests.Modules.SearchProviders;

public class SearchProvidersE2ETests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.SearchProviders | TestModule.Providers | TestModule.ServiceCatalogs;

    [Fact]
    public async Task Search_ByServiceAndRadius_ShouldReturnNearbyProviders()
    {
        // 1. Arrange: Coordenadas do centro de São Paulo (onde as sementes provavelmente estão localizadas)
        double lat = -23.5505; 
        double lon = -46.6333;
        double radius = 10.0;

        // 2. Act
        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={radius}");

        // 3. Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty(); 
    }

    [Fact]
    public async Task Search_WithSmallRadius_ShouldFilterOutDistantProviders()
    {
        // Arrange
        double lat = -23.5505;
        double lon = -46.6333;
        double tinyRadius = 0.1; // 100 metros

        // Act
        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={tinyRadius}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>();
        result.Should().NotBeNull();
        
        foreach (var provider in result!.Items)
        {
            provider.DistanceInKm.Should().BeLessThanOrEqualTo(tinyRadius);
        }
    }

    [Fact]
    public async Task Search_ShouldBeOrderedByDistanceAscending()
    {
        // Arrange
        double lat = -23.5505;
        double lon = -46.6333;
        double radius = 50.0;

        // Act
        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={radius}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>();
        result!.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(x => x.DistanceInKm.HasValue);
        result.Items.Should().BeInAscendingOrder(x => x.DistanceInKm);
    }

    [Fact]
    public async Task Search_WithNoResults_ShouldReturnEmptyPage()
    {
        // Arrange: Coordenadas da Antártida (nenhum prestador esperado)
        double lat = -90.0;
        double lon = 0.0;
        double radius = 1.0;

        // Act
        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={radius}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>();
        result!.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task Search_Pagination_ShouldWork()
    {
        // Arrange
        double lat = -23.5505;
        double lon = -46.6333;
        double radius = 100.0;
        int pageSize = 2;

        // Act
        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={radius}&page=1&pageSize={pageSize}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>();
        result!.Items.Count.Should().BeLessThanOrEqualTo(pageSize);
        result.PageSize.Should().Be(pageSize);
    }

    [Fact]
    public async Task Search_Performance_ShouldBeWithinLimit()
    {
        // Arrange
        double lat = -23.5505;
        double lon = -46.6333;
        double radius = 20.0;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={radius}");

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThanOrEqualTo(1500, "O desempenho da busca deve ser inferior a 1500ms em ambiente de teste");
    }
}
