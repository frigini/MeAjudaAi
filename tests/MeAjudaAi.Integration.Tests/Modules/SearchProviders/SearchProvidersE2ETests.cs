using MeAjudaAi.Integration.Tests.Base;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Globalization;
using System.Text.Json;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Modules.SearchProviders.Application.DTOs;

namespace MeAjudaAi.Integration.Tests.Modules.SearchProviders;

[Collection("Integration")]
public class SearchProvidersE2ETests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.SearchProviders;

    [Fact]
    public async Task Search_ByServiceAndRadius_ShouldReturnNearbyProviders()
    {
        // 1. Arrange: Coordenadas do centro de São Paulo e o ID do serviço de teste
        string lat = (-23.5505).ToString(CultureInfo.InvariantCulture); 
        string lon = (-46.6333).ToString(CultureInfo.InvariantCulture);
        string radius = (10.0).ToString(CultureInfo.InvariantCulture);
        var serviceId = BaseApiTest.TestServiceId;

        // 2. Act: Busca com filtro de serviço
        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={radius}&serviceIds={serviceId}");

        // 3. Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.Should().BeTrue($"Search request failed with status {response.StatusCode}. Body: {responseBody}");
        
        var result = JsonSerializer.Deserialize<PagedResult<SearchableProviderDto>>(responseBody, SerializationDefaults.Api);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty("At least one provider should match the service and radius filter");
        result.Items.Should().OnlyContain(x => x.ServiceIds.Contains(serviceId), "All returned providers must offer the requested service");
    }

    [Fact]
    public async Task Search_WithSmallRadius_ShouldFilterOutDistantProviders()
    {
        // Arrange
        string lat = (-23.5505).ToString(CultureInfo.InvariantCulture);
        string lon = (-46.6333).ToString(CultureInfo.InvariantCulture);
        double tinyRadiusVal = 0.1;
        string tinyRadius = tinyRadiusVal.ToString(CultureInfo.InvariantCulture); // 100 metros

        // Act
        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={tinyRadius}");

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.Should().BeTrue($"Search request failed with status {response.StatusCode}. Body: {responseBody}");
        
        var result = JsonSerializer.Deserialize<PagedResult<SearchableProviderDto>>(responseBody, SerializationDefaults.Api);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty("At least one provider should be within the tiny radius for this test to be valid");
        
        foreach (var provider in result.Items)
        {
            provider.DistanceInKm.HasValue.Should().BeTrue("DistanceInKm should be calculated");
            if (provider.DistanceInKm.HasValue)
            {
                provider.DistanceInKm.Value.Should().BeLessThanOrEqualTo(tinyRadiusVal, $"Provider {provider.Name} should be within {tinyRadiusVal}km");
            }
        }
    }

    [Fact]
    public async Task Search_ShouldBeOrderedByDistanceAscending()
    {
        // Arrange
        string lat = (-23.5505).ToString(CultureInfo.InvariantCulture);
        string lon = (-46.6333).ToString(CultureInfo.InvariantCulture);
        string radius = (50.0).ToString(CultureInfo.InvariantCulture);

        // Act
        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={radius}");

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.Should().BeTrue($"Search request failed with status {response.StatusCode}. Body: {responseBody}");
        
        var result = JsonSerializer.Deserialize<PagedResult<SearchableProviderDto>>(responseBody, SerializationDefaults.Api);
        result!.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(x => x.DistanceInKm.HasValue);
        result.Items.Should().BeInAscendingOrder(x => x.DistanceInKm);
    }

    [Fact]
    public async Task Search_WithNoResults_ShouldReturnEmptyPage()
    {
        // Arrange: Coordenadas da Antártida (nenhum prestador esperado)
        string lat = (-90.0).ToString(CultureInfo.InvariantCulture);
        string lon = (0.0).ToString(CultureInfo.InvariantCulture);
        string radius = (1.0).ToString(CultureInfo.InvariantCulture);

        // Act
        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={radius}");

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.Should().BeTrue($"Search request failed with status {response.StatusCode}. Body: {responseBody}");
        
        var result = JsonSerializer.Deserialize<PagedResult<SearchableProviderDto>>(responseBody, SerializationDefaults.Api);
        result!.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task Search_Pagination_ShouldWork()
    {
        // Arrange
        string lat = (-23.5505).ToString(CultureInfo.InvariantCulture);
        string lon = (-46.6333).ToString(CultureInfo.InvariantCulture);
        string radius = (100.0).ToString(CultureInfo.InvariantCulture);
        int pageSize = 2;

        // Act
        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={radius}&page=1&pageSize={pageSize}");

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.Should().BeTrue($"Search request failed with status {response.StatusCode}. Body: {responseBody}");
        
        var result = JsonSerializer.Deserialize<PagedResult<SearchableProviderDto>>(responseBody, SerializationDefaults.Api);
        result!.Items.Count.Should().BeLessThanOrEqualTo(pageSize);
        result.PageSize.Should().Be(pageSize);
    }

    [Fact]
    public async Task Search_Performance_ShouldBeWithinLimit()
    {
        // Arrange
        string lat = (-23.5505).ToString(CultureInfo.InvariantCulture);
        string lon = (-46.6333).ToString(CultureInfo.InvariantCulture);
        string radius = (20.0).ToString(CultureInfo.InvariantCulture);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync($"/api/v1/search/providers?latitude={lat}&longitude={lon}&radiusInKm={radius}");

        // Assert
        stopwatch.Stop();
        var responseBody = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.Should().BeTrue($"Search request failed with status {response.StatusCode}. Body: {responseBody}");
        
        var result = JsonSerializer.Deserialize<PagedResult<SearchableProviderDto>>(responseBody, SerializationDefaults.Api);
        stopwatch.ElapsedMilliseconds.Should().BeLessThanOrEqualTo(5000, $"A busca deve ser rápida (< 5s). Tempo: {stopwatch.ElapsedMilliseconds}ms");
    }
}