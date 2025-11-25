using System.Net;
using System.Text.Json;
using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.Integration;

/// <summary>
/// Testes de integração entre o módulo ServiceCatalogs e outros módulos
/// Demonstra como o módulo de catálogos pode ser consumido por outros módulos
/// </summary>
public class ServiceCatalogsModuleIntegrationTests : TestContainerTestBase
{
    // TODO: Create GitHub issue to track E2E authentication infrastructure refactor.
    // 14+ E2E tests affected by ConfigurableTestAuthenticationHandler race condition.
    [Fact(Skip = "AUTH: Returns 403 instead of expected success. ConfigurableTestAuthenticationHandler race condition - see issue tracking comment above.")]
    public async Task ServicesModule_Can_Validate_Services_From_Catalogs()
    {
        // Arrange - Create test service categories and services
        AuthenticateAsAdmin();
        var category = await CreateServiceCategoryAsync("Limpeza", "Serviços de limpeza");
        var service1 = await CreateServiceAsync(category.Id, "Limpeza de Piscina", "Limpeza completa");
        var service2 = await CreateServiceAsync(category.Id, "Limpeza de Jardim", "Manutenção de jardim");

        // Act - Services module would validate service IDs by querying individual services
        var response1 = await ApiClient.GetAsync($"/api/v1/service-catalogs/services/{service1.Id}");
        var response2 = await ApiClient.GetAsync($"/api/v1/service-catalogs/services/{service2.Id}");

        // Assert - Both services should exist
        response1.StatusCode.Should().Be(HttpStatusCode.OK, "service1 should exist");
        response2.StatusCode.Should().Be(HttpStatusCode.OK, "service2 should exist");

        var content1 = await response1.Content.ReadAsStringAsync();
        var result1 = JsonSerializer.Deserialize<JsonElement>(content1, JsonOptions);
        result1.TryGetProperty("data", out var data1).Should().BeTrue();
        data1.TryGetProperty("id", out var id1).Should().BeTrue();
        id1.GetGuid().Should().Be(service1.Id);

        var content2 = await response2.Content.ReadAsStringAsync();
        var result2 = JsonSerializer.Deserialize<JsonElement>(content2, JsonOptions);
        result2.TryGetProperty("data", out var data2).Should().BeTrue();
        data2.TryGetProperty("id", out var id2).Should().BeTrue();
        id2.GetGuid().Should().Be(service2.Id);
    }

    [Fact(Skip = "AUTH: Returns 403 instead of expected success. ConfigurableTestAuthenticationHandler race condition - see issue tracking comment above.")]
    public async Task ProvidersModule_Can_Query_Active_Services_Only()
    {
        // Arrange - Create services with different states
        AuthenticateAsAdmin();
        var category = await CreateServiceCategoryAsync("Manutenção", "Serviços de manutenção");
        var activeService = await CreateServiceAsync(category.Id, "Manutenção Elétrica", "Serviços elétricos");
        var inactiveService = await CreateServiceAsync(category.Id, "Manutenção Antiga", "Serviço descontinuado");

        // Deactivate one service
        await PostJsonAsync($"/api/v1/service-catalogs/services/{inactiveService.Id}/deactivate", new { });

        // Act - Query only active services
        var response = await ApiClient.GetAsync("/api/v1/service-catalogs/services?activeOnly=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);

        result.TryGetProperty("data", out var data).Should().BeTrue();
        var services = data.Deserialize<ServiceListDto[]>(JsonOptions);
        services.Should().NotBeNull();
        services!.Should().Contain(s => s.Id == activeService.Id);
        services!.Should().NotContain(s => s.Id == inactiveService.Id);
    }

    [Fact]
    public async Task RequestsModule_Can_Filter_Services_By_Category()
    {
        // Arrange - Create multiple categories and services
        AuthenticateAsAdmin();
        var category1 = await CreateServiceCategoryAsync("Limpeza", "Limpeza geral");
        var category2 = await CreateServiceCategoryAsync("Reparos", "Reparos diversos");

        var service1 = await CreateServiceAsync(category1.Id, "Limpeza de Casa", "Limpeza residencial");
        var service2 = await CreateServiceAsync(category1.Id, "Limpeza de Escritório", "Limpeza comercial");
        var service3 = await CreateServiceAsync(category2.Id, "Reparo de Torneira", "Hidráulica");

        // Act - Filter services by category (Requests module would do this)
        var response = await ApiClient.GetAsync($"/api/v1/service-catalogs/services/category/{category1.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);

        result.TryGetProperty("data", out var data).Should().BeTrue();
        var services = data.Deserialize<ServiceListDto[]>(JsonOptions);
        services.Should().NotBeNull();
        services!.Length.Should().Be(2);
        services!.Should().AllSatisfy(s => s.CategoryId.Should().Be(category1.Id));
    }

    [Fact]
    public async Task MultipleModules_Can_Read_Same_ServiceCategory_Concurrently()
    {
        // Arrange
        AuthenticateAsAdmin();
        var category = await CreateServiceCategoryAsync("Popular Service", "Very popular category");

        // Act - Simulate multiple modules reading the same category concurrently
        var tasks = Enumerable.Range(0, 10).Select(async _ =>
        {
            var response = await ApiClient.GetAsync($"/api/v1/service-catalogs/categories/{category.Id}");
            return response;
        });

        var responses = await Task.WhenAll(tasks);

        // Assert - All requests should succeed
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task Dashboard_Module_Can_Get_All_Categories_For_Statistics()
    {
        // Arrange - Create diverse categories
        AuthenticateAsAdmin();
        await CreateServiceCategoryAsync("Limpeza", "Serviços de limpeza");
        await CreateServiceCategoryAsync("Reparos", "Serviços de reparo");
        await CreateServiceCategoryAsync("Jardinagem", "Serviços de jardim");

        // Act - Dashboard module gets all categories for statistics
        var response = await ApiClient.GetAsync("/api/v1/service-catalogs/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);

        result.TryGetProperty("data", out var data).Should().BeTrue();
        var categories = data.Deserialize<ServiceCategoryDto[]>(JsonOptions);
        categories.Should().NotBeNull();
        categories!.Length.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task Admin_Module_Can_Manage_Service_Lifecycle()
    {
        // Arrange
        AuthenticateAsAdmin();
        var category = await CreateServiceCategoryAsync("Temporário", "Categoria temporária");
        var service = await CreateServiceAsync(category.Id, "Serviço Teste", "Para testes");

        // Act & Assert - Full lifecycle management

        // 1. Update service
        var updateRequest = new
        {
            Name = "Serviço Atualizado",
            Description = "Descrição atualizada",
            DisplayOrder = 10
        };
        var updateResponse = await PutJsonAsync($"/api/v1/service-catalogs/services/{service.Id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 2. Deactivate service
        var deactivateResponse = await PostJsonAsync($"/api/v1/service-catalogs/services/{service.Id}/deactivate", new { });
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 3. Verify service is inactive
        var checkResponse = await ApiClient.GetAsync($"/api/v1/service-catalogs/services/{service.Id}");
        checkResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var checkContent = await checkResponse.Content.ReadAsStringAsync();
        var checkResult = JsonSerializer.Deserialize<JsonElement>(checkContent, JsonOptions);
        checkResult.TryGetProperty("data", out var serviceData).Should().BeTrue();
        serviceData.TryGetProperty("isActive", out var isActiveProperty).Should().BeTrue();
        isActiveProperty.GetBoolean().Should().BeFalse();

        // 4. Delete service (should work now that it's inactive)
        // Re-autenticar antes de DELETE para evitar perda de estado em testes paralelos
        AuthenticateAsAdmin();
        var deleteResponse = await ApiClient.DeleteAsync($"/api/v1/service-catalogs/services/{service.Id}");
        deleteResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.OK);
    }

    #region Helper Methods

    private async Task<ServiceCategoryDto> CreateServiceCategoryAsync(string name, string description)
    {
        var request = new
        {
            Name = name,
            Description = description,
            DisplayOrder = 1
        };

        var response = await PostJsonAsync("/api/v1/service-catalogs/categories", request);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create service category. Status: {response.StatusCode}, Content: {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        result.TryGetProperty("data", out var data).Should().BeTrue();

        return data.Deserialize<ServiceCategoryDto>(JsonOptions)!;
    }

    private async Task<ServiceDto> CreateServiceAsync(Guid categoryId, string name, string description)
    {
        // Re-autenticar para evitar problemas de state em testes paralelos
        AuthenticateAsAdmin();

        var request = new
        {
            CategoryId = categoryId,
            Name = name,
            Description = description,
            DisplayOrder = 1
        };

        var response = await PostJsonAsync("/api/v1/service-catalogs/services", request);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create service. Status: {response.StatusCode}, Content: {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        result.TryGetProperty("data", out var data).Should().BeTrue();

        return data.Deserialize<ServiceDto>(JsonOptions)!;
    }

    #endregion

    #region DTOs

    // NOTE: Local DTOs are intentionally defined here instead of importing from Application.DTOs
    // to ensure E2E tests validate the actual API contract independently from internal DTOs.
    // This prevents breaking changes in internal DTOs from being masked in integration tests.

    private record ServiceCategoryDto(
        Guid Id,
        string Name,
        string? Description,
        int DisplayOrder,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );

    private record ServiceDto(
        Guid Id,
        Guid CategoryId,
        string CategoryName,
        string Name,
        string? Description,
        int DisplayOrder,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );

    private record ServiceListDto(
        Guid Id,
        Guid CategoryId,
        string Name,
        string? Description,
        bool IsActive
    );

    #endregion
}
