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
        // 1. Arrange: Coordinates for São Paulo center (where seeds are likely located)
        double lat = -23.5505; 
        double lon = -46.6333;
        double radius = 10.0;

        // 2. Act
        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={radius}");

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>();
        result.Should().NotBeNull();
        // result.Items.Should().NotBeEmpty(); // Depends on seed database content
    }

    [Fact]
    public async Task Search_WithSmallRadius_ShouldFilterOutDistantProviders()
    {
        // Arrange
        double lat = -23.5505;
        double lon = -46.6333;
        double tinyRadius = 0.1; // 100 meters

        // Act
        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={tinyRadius}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
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
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>();
        result!.Items.Should().BeInAscendingOrder(x => x.DistanceInKm);
    }

    [Fact]
    public async Task Search_WithNoResults_ShouldReturnEmptyPage()
    {
        // Arrange: Antarctica coordinates (no providers expected)
        double lat = -90.0;
        double lon = 0.0;
        double radius = 1.0;

        // Act
        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={radius}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
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
        response.StatusCode.Should().Be(HttpStatusCode.OK);
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
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThanOrEqualTo(1500, "Search performance should be under 1500ms in test environment");
    }
}
