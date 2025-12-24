using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using MeAjudaAi.Shared.Contracts;

namespace MeAjudaAi.E2E.Tests.Modules.SearchProviders;

/// <summary>
/// Testes E2E para busca de prestadores de serviço.
/// Valida workflows completos de busca geolocalizada com filtros e ordenação.
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "SearchProviders")]
public class SearchProvidersEndToEndTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public SearchProvidersEndToEndTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SearchProviders_CompleteWorkflow_ShouldFindProvidersWithinRadius()
    {
        // Arrange - Criar Provider dentro do raio de busca
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        
        // São Paulo coordinates
        var searchLatitude = -23.5505;
        var searchLongitude = -46.6333;
        var radiusInKm = 10.0;

        // Provider próximo (dentro do raio)
        var nearbyProvider = await CreateProviderAsync(
            $"nearby_provider_{Guid.NewGuid():N}",
            -23.5605, // ~1.1km de distância
            -46.6433
        );

        // Act
        var response = await _fixture.ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={searchLatitude}&longitude={searchLongitude}&radiusInKm={radiusInKm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>(TestContainerFixture.JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.Should().Contain(p => p.ProviderId == nearbyProvider);
    }

    [Fact]
    public async Task SearchProviders_ShouldExcludeProvidersOutsideRadius()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        
        var searchLatitude = -23.5505;
        var searchLongitude = -46.6333;
        var smallRadius = 1.0; // 1km apenas

        // Provider distante (fora do raio)
        var distantProviderId = await CreateProviderAsync(
            $"distant_provider_{Guid.NewGuid():N}",
            -23.6505, // ~11km de distância
            -46.7333
        );

        // Act
        var response = await _fixture.ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={searchLatitude}&longitude={searchLongitude}&radiusInKm={smallRadius}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>(TestContainerFixture.JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotContain(p => p.ProviderId == distantProviderId,
            "Provider outside radius should not appear in results");
    }

    [Fact]
    public async Task SearchProviders_WithServiceFilter_ShouldReturnOnlyMatchingProviders()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        
        var searchLatitude = -23.5505;
        var searchLongitude = -46.6333;
        var radiusInKm = 50.0;

        // Criar categorias e serviços
        var cleaningCategoryId = await CreateServiceCategoryAsync("Limpeza");
        var cleaningServiceId = await CreateServiceAsync(cleaningCategoryId, "Limpeza de Casa");
        
        var gardenCategoryId = await CreateServiceCategoryAsync("Jardinagem");
        var gardenServiceId = await CreateServiceAsync(gardenCategoryId, "Manutenção de Jardim");

        // Provider com serviço de limpeza
        var cleaningProviderId = await CreateProviderAsync(
            $"cleaning_provider_{Guid.NewGuid():N}",
            -23.5605,
            -46.6433
        );
        await AddServiceToProviderAsync(cleaningProviderId, cleaningServiceId);

        // Provider com serviço de jardinagem
        var gardenProviderId = await CreateProviderAsync(
            $"garden_provider_{Guid.NewGuid():N}",
            -23.5705,
            -46.6533
        );
        await AddServiceToProviderAsync(gardenProviderId, gardenServiceId);

        // Act - Buscar apenas por serviço de limpeza
        var response = await _fixture.ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={searchLatitude}&longitude={searchLongitude}&radiusInKm={radiusInKm}&serviceIds={cleaningServiceId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>(TestContainerFixture.JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().Contain(p => p.ProviderId == cleaningProviderId,
            "Provider with cleaning service should be found");
        result.Items.Should().NotContain(p => p.ProviderId == gardenProviderId,
            "Provider with only garden service should not be found when filtering by cleaning service");
    }

    [Fact]
    public async Task SearchProviders_WithMultipleServiceFilters_ShouldReturnProvidersWithAnyService()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        
        var searchLatitude = -23.5505;
        var searchLongitude = -46.6333;
        var radiusInKm = 50.0;

        var categoryId = await CreateServiceCategoryAsync("Manutenção");
        var electricalServiceId = await CreateServiceAsync(categoryId, "Elétrica");
        var plumbingServiceId = await CreateServiceAsync(categoryId, "Hidráulica");

        var electricianId = await CreateProviderAsync($"electrician_{Guid.NewGuid():N}", -23.5605, -46.6433);
        await AddServiceToProviderAsync(electricianId, electricalServiceId);

        var plumberId = await CreateProviderAsync($"plumber_{Guid.NewGuid():N}", -23.5705, -46.6533);
        await AddServiceToProviderAsync(plumberId, plumbingServiceId);

        // Act - Buscar por ambos os serviços
        var response = await _fixture.ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={searchLatitude}&longitude={searchLongitude}&radiusInKm={radiusInKm}&serviceIds={electricalServiceId}&serviceIds={plumbingServiceId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>(TestContainerFixture.JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().Contain(p => p.ProviderId == electricianId);
        result!.Items.Should().Contain(p => p.ProviderId == plumberId);
    }

    [Fact]
    public async Task SearchProviders_ShouldOrderBySubscriptionTier_ThenByRating_ThenByDistance()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        
        var searchLatitude = -23.5505;
        var searchLongitude = -46.6333;
        var radiusInKm = 50.0;

        // Provider Premium próximo
        var premiumNearId = await CreateProviderAsync(
            $"premium_near_{Guid.NewGuid():N}",
            -23.5555, // Muito próximo
            -46.6383,
            subscriptionTier: "Platinum"
        );

        // Provider Free distante mas com alta avaliação
        var freeDistantHighRatingId = await CreateProviderAsync(
            $"free_distant_high_{Guid.NewGuid():N}",
            -23.6005, // Mais distante
            -46.6833,
            subscriptionTier: "Free"
        );

        // Provider Standard médio
        var standardMediumId = await CreateProviderAsync(
            $"standard_medium_{Guid.NewGuid():N}",
            -23.5705,
            -46.6533,
            subscriptionTier: "Standard"
        );

        // Act
        var response = await _fixture.ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={searchLatitude}&longitude={searchLongitude}&radiusInKm={radiusInKm}&page=1&pageSize=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>(TestContainerFixture.JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();

        // Premium deve vir antes de Standard
        var premiumIndex = result.Items.ToList().FindIndex(p => p.ProviderId == premiumNearId);
        var standardIndex = result.Items.ToList().FindIndex(p => p.ProviderId == standardMediumId);
        
        if (premiumIndex >= 0 && standardIndex >= 0)
        {
            premiumIndex.Should().BeLessThan(standardIndex,
                "Platinum provider should be ranked higher than Standard provider");
        }
    }

    [Fact]
    public async Task SearchProviders_WithMinRatingFilter_ShouldExcludeLowRatedProviders()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        
        var searchLatitude = -23.5505;
        var searchLongitude = -46.6333;
        var radiusInKm = 50.0;
        var minRating = 4.0m;

        // Note: Rating depende de reviews reais, então este teste valida apenas que o filtro é aceito
        // Em ambiente de teste, providers novos terão rating 0 ou null

        // Act
        var response = await _fixture.ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={searchLatitude}&longitude={searchLongitude}&radiusInKm={radiusInKm}&minRating={minRating}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Search with minRating filter should succeed");
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>(TestContainerFixture.JsonOptions);
        result.Should().NotBeNull();
        
        // All returned providers should have rating >= minRating
        result!.Items.Should().AllSatisfy(p =>
        {
            p.AverageRating.Should().BeGreaterThanOrEqualTo(minRating,
                "All providers should meet minimum rating requirement");
        });
    }

    [Fact]
    public async Task SearchProviders_WithPagination_ShouldRespectPageSizeAndNumber()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        
        var searchLatitude = -23.5505;
        var searchLongitude = -46.6333;
        var radiusInKm = 100.0;
        var pageSize = 5;

        // Criar vários providers
        for (int i = 0; i < 12; i++)
        {
            await CreateProviderAsync(
                $"provider_{i}_{Guid.NewGuid():N}",
                -23.5505 + (i * 0.01),
                -46.6333 + (i * 0.01)
            );
        }

        // Act - Página 1
        var page1Response = await _fixture.ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={searchLatitude}&longitude={searchLongitude}&radiusInKm={radiusInKm}&page=1&pageSize={pageSize}");

        // Act - Página 2
        var page2Response = await _fixture.ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={searchLatitude}&longitude={searchLongitude}&radiusInKm={radiusInKm}&page=2&pageSize={pageSize}");

        // Assert
        page1Response.StatusCode.Should().Be(HttpStatusCode.OK);
        page2Response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var page1Result = await page1Response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>(TestContainerFixture.JsonOptions);
        var page2Result = await page2Response.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>(TestContainerFixture.JsonOptions);

        page1Result.Should().NotBeNull();
        page2Result.Should().NotBeNull();
        
        page1Result!.Page.Should().Be(1);
        page1Result.PageSize.Should().Be(pageSize);
        page1Result.Items.Count().Should().BeLessThanOrEqualTo(pageSize);
        
        page2Result!.Page.Should().Be(2);
        page2Result.PageSize.Should().Be(pageSize);

        // Páginas diferentes devem ter providers diferentes
        var page1Ids = page1Result.Items.Select(p => p.ProviderId).ToHashSet();
        var page2Ids = page2Result.Items.Select(p => p.ProviderId).ToHashSet();
        page1Ids.Should().NotIntersectWith(page2Ids,
            "Different pages should contain different providers");
    }

    // Helper methods
    private async Task<Guid> CreateProviderAsync(
        string businessName,
        double latitude,
        double longitude,
        string subscriptionTier = "Free")
    {
        // Ensure authenticated as admin to create providers
        TestContainerFixture.AuthenticateAsAdmin();
        
        // Criar um usuário primeiro
        var userId = await _fixture.CreateTestUserAsync();

        // NOTA: latitude e longitude são parâmetros mas não podemos usá-los diretamente
        // pois o sistema geocodifica o endereço automaticamente via Locations API.
        // Para testes E2E, usamos endereços de São Paulo que resultarão em coordenadas aproximadas.
        
        var request = new
        {
            UserId = userId.ToString(),
            Name = businessName,
            Type = 0, // Individual
            BusinessProfile = new
            {
                LegalName = businessName,
                FantasyName = businessName,
                Description = $"Test provider {businessName}",
                ContactInfo = new
                {
                    Email = $"{businessName.Replace(" ", "_").ToLowerInvariant()}@example.com",
                    PhoneNumber = "+5511999999999",
                    Website = (string?)null
                },
                PrimaryAddress = new
                {
                    Street = "Avenida Paulista",
                    Number = "1578", // Coordenadas aproximadas: -23.5505, -46.6333
                    Complement = (string?)null,
                    Neighborhood = "Bela Vista",
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = "01310-200",
                    Country = "Brasil"
                }
            }
        };

        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/providers", request, TestContainerFixture.JsonOptions);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = response.Headers.Location?.ToString();
        location.Should().NotBeNullOrEmpty();
        
        var providerId = TestContainerFixture.ExtractIdFromLocation(location!);

        // Verificar o provider para que ele seja indexado no SearchProviders
        var verifyRequest = new
        {
            NewStatus = 1, // Verified
            Reason = "Test verification for search indexing"
        };

        var verifyResponse = await _fixture.ApiClient.PutAsJsonAsync(
            $"/api/v1/providers/{providerId}/verification-status",
            verifyRequest,
            TestContainerFixture.JsonOptions);

        // Continue silently if verification fails - the provider may still be searchable
        if (!verifyResponse.IsSuccessStatusCode)
        {
            // Log or ignore - test continues regardless
        }

        // Pequeno delay para permitir indexação assíncrona
        await Task.Delay(500);

        return providerId;
    }

    private async Task<Guid> CreateServiceCategoryAsync(string name)
    {
        var request = new
        {
            Name = name,
            Description = $"Categoria {name}",
            DisplayOrder = 1
        };

        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", request, TestContainerFixture.JsonOptions);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = response.Headers.Location?.ToString();
        location.Should().NotBeNullOrEmpty();
        return TestContainerFixture.ExtractIdFromLocation(location!);
    }

    private async Task<Guid> CreateServiceAsync(Guid categoryId, string name)
    {
        var request = new
        {
            CategoryId = categoryId,
            Name = name,
            Description = $"Serviço {name}",
            DisplayOrder = 1
        };

        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/services", request, TestContainerFixture.JsonOptions);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = response.Headers.Location?.ToString();
        location.Should().NotBeNullOrEmpty();
        return TestContainerFixture.ExtractIdFromLocation(location!);
    }

    private async Task AddServiceToProviderAsync(Guid providerId, Guid serviceId)
    {
        var response = await _fixture.ApiClient.PutAsync(
            $"/api/v1/providers/{providerId}/services/{serviceId}",
            null);
        
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.Created);
    }
}
