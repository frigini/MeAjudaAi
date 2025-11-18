using System.Net;
using System.Text.Json;
using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.Integration;

/// <summary>
/// Testes de integração entre o módulo Catalogs e outros módulos
/// Demonstra como o módulo de catálogos pode ser consumido por outros módulos
/// </summary>
public class CatalogsModuleIntegrationTests : TestContainerTestBase
{
    [Fact]
    public async Task ServicesModule_Can_Validate_Services_From_Catalogs()
    {
        // Arrange - Create test service categories and services
        AuthenticateAsAdmin();
        var category = await CreateServiceCategoryAsync("Limpeza", "Serviços de limpeza");
        var service1 = await CreateServiceAsync(category.Id, "Limpeza de Piscina", "Limpeza completa");
        var service2 = await CreateServiceAsync(category.Id, "Limpeza de Jardim", "Manutenção de jardim");

        // Act - Services module would validate service IDs
        var validateRequest = new
        {
            ServiceIds = new[] { service1.Id, service2.Id }
        };

        // Simulate calling the validation endpoint
        var response = await PostJsonAsync("/api/v1/catalogs/services/validate", validateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        
        // Should validate all services as valid
        result.TryGetProperty("data", out var data).Should().BeTrue();
        data.TryGetProperty("validServiceIds", out var validIds).Should().BeTrue();
        validIds.GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task ProvidersModule_Can_Query_Active_Services_Only()
    {
        // Arrange - Create services with different states
        AuthenticateAsAdmin();
        var category = await CreateServiceCategoryAsync("Manutenção", "Serviços de manutenção");
        var activeService = await CreateServiceAsync(category.Id, "Manutenção Elétrica", "Serviços elétricos");
        var inactiveService = await CreateServiceAsync(category.Id, "Manutenção Antiga", "Serviço descontinuado");

        // Deactivate one service
        await PostJsonAsync($"/api/v1/catalogs/services/{inactiveService.Id}/deactivate", new { });

        // Act - Query only active services
        var response = await ApiClient.GetAsync("/api/v1/catalogs/services?activeOnly=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        
        result.TryGetProperty("data", out var data).Should().BeTrue();
        var services = data.Deserialize<ServiceDto[]>(JsonOptions);
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
        var response = await ApiClient.GetAsync($"/api/v1/catalogs/categories/{category1.Id}/services");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        
        result.TryGetProperty("data", out var data).Should().BeTrue();
        var services = data.Deserialize<ServiceDto[]>(JsonOptions);
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
            var response = await ApiClient.GetAsync($"/api/v1/catalogs/categories/{category.Id}");
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
        var response = await ApiClient.GetAsync("/api/v1/catalogs/categories");

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
        var updateResponse = await PutJsonAsync($"/api/v1/catalogs/services/{service.Id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 2. Deactivate service
        var deactivateResponse = await PostJsonAsync($"/api/v1/catalogs/services/{service.Id}/deactivate", new { });
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 3. Verify service is inactive
        var checkResponse = await ApiClient.GetAsync($"/api/v1/catalogs/services/{service.Id}/active");
        var checkContent = await checkResponse.Content.ReadAsStringAsync();
        var checkResult = JsonSerializer.Deserialize<JsonElement>(checkContent, JsonOptions);
        checkResult.TryGetProperty("data", out var isActive).Should().BeTrue();
        isActive.GetBoolean().Should().BeFalse();

        // 4. Delete service (should work now that it's inactive)
        var deleteResponse = await ApiClient.DeleteAsync($"/api/v1/catalogs/services/{service.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
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

        var response = await PostJsonAsync("/api/v1/catalogs/categories", request);
        
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
        var request = new
        {
            CategoryId = categoryId,
            Name = name,
            Description = description,
            DisplayOrder = 1
        };

        var response = await PostJsonAsync("/api/v1/catalogs/services", request);
        
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

    private record ServiceCategoryDto(Guid Id, string Name, string Description, int DisplayOrder, bool IsActive);
    private record ServiceDto(Guid Id, Guid CategoryId, string Name, string Description, int DisplayOrder, bool IsActive);

    #endregion
}
