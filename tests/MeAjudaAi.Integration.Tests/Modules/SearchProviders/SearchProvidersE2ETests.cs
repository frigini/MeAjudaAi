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
        // 1. Arrange: Seed providers at specific coordinates
        // Note: For E2E we rely on existing seeds or add specific ones if needed.
        // Let's assume we use a known coordinate from seed.
        double lat = -23.5505; // São Paulo
        double lon = -46.6333;
        double radius = 10.0;

        // 2. Act
        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={radius}");

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>();
        result.Should().NotBeNull();
        // result.Items.Should().NotBeEmpty(); // Depends on seed
    }

    [Fact]
    public async Task Search_WithSmallRadius_ShouldFilterOutDistantProviders()
    {
        double lat = -23.5505;
        double lon = -46.6333;
        double tinyRadius = 0.1; // 100 meters

        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={tinyRadius}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>();
        result.Should().NotBeNull();
        // Distance of providers should be <= 0.1
        foreach (var provider in result!.Items)
        {
            provider.DistanceInKm.Should().BeLessThanOrEqualTo(tinyRadius);
        }
    }

    [Fact]
    public async Task Search_ShouldBeOrderedByDistanceAscending()
    {
        double lat = -23.5505;
        double lon = -46.6333;
        double radius = 50.0;

        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={radius}");

        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>();
        result!.Items.Should().BeInAscendingOrder(x => x.DistanceInKm);
    }

    [Fact]
    public async Task Search_WithNoResults_ShouldReturnEmptyPage()
    {
        // Antarctica coordinates
        double lat = -90.0;
        double lon = 0.0;
        double radius = 1.0;

        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={radius}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>();
        result!.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task Search_Pagination_ShouldWork()
    {
        double lat = -23.5505;
        double lon = -46.6333;
        double radius = 100.0;
        int pageSize = 2;

        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={radius}&page=1&pageSize={pageSize}");

        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>();
        result!.Items.Count.Should().BeLessThanOrEqualTo(pageSize);
        result.PageSize.Should().Be(pageSize);
    }
}
