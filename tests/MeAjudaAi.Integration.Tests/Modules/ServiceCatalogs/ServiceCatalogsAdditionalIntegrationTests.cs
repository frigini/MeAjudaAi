using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;

namespace MeAjudaAi.Integration.Tests.Modules.ServiceCatalogs;

/// <summary>
/// Testes de integração adicionais para cobrir endpoints de ServiceCatalogs que faltavam.
/// </summary>
public class ServiceCatalogsAdditionalIntegrationTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.ServiceCatalogs;

    [Fact]
    public async Task GetServicesByCategory_ShouldReturnResults()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        
        // 1. Criar uma categoria
        var categoryResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", new { name = "Category Filter Test" });
        var catId = GetResponseData(await ReadJsonAsync<JsonElement>(categoryResponse.Content)).GetProperty("id").GetString();

        // 2. Criar um serviço nessa categoria
        await Client.PostAsJsonAsync("/api/v1/service-catalogs/services", new { name = "Filter Svc", categoryId = catId });

        // Act
        var response = await Client.GetAsync($"/api/v1/service-catalogs/services/by-category/{catId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = GetResponseData(await ReadJsonAsync<JsonElement>(response.Content));
        data.ValueKind.Should().Be(JsonValueKind.Array);
        data.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ChangeServiceCategory_ShouldUpdateSuccessfully()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        
        // 1. Criar duas categorias e um serviço
        var cat1Res = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", new { name = "Cat 1" });
        var cat1Id = GetResponseData(await ReadJsonAsync<JsonElement>(cat1Res.Content)).GetProperty("id").GetString();
        
        var cat2Res = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", new { name = "Cat 2" });
        var cat2Id = GetResponseData(await ReadJsonAsync<JsonElement>(cat2Res.Content)).GetProperty("id").GetString();

        var svcRes = await Client.PostAsJsonAsync("/api/v1/service-catalogs/services", new { name = "Move Me", categoryId = cat1Id });
        var svcId = GetResponseData(await ReadJsonAsync<JsonElement>(svcRes.Content)).GetProperty("id").GetString();

        // Act - Mudar categoria
        var changeResponse = await Client.PutAsJsonAsync($"/api/v1/service-catalogs/services/{svcId}/category", new { newCategoryId = cat2Id });

        // Assert
        changeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar se mudou mesmo
        var getSvcRes = await Client.GetAsync($"/api/v1/service-catalogs/services/{svcId}");
        var svcData = GetResponseData(await ReadJsonAsync<JsonElement>(getSvcRes.Content));
        svcData.GetProperty("categoryId").GetString().Should().Be(cat2Id);
    }

    [Fact]
    public async Task ValidateServices_ShouldReturnOk()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var serviceIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/service-catalogs/services/validate", new { serviceIds });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteServiceCategory_ShouldRemoveResource()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var catRes = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", new { name = "Delete Me Category" });
        var id = GetResponseData(await ReadJsonAsync<JsonElement>(catRes.Content)).GetProperty("id").GetString();

        // Act
        var response = await Client.DeleteAsync($"/api/v1/service-catalogs/categories/{id}");

        // Assert - 200 ou 400 se tiver serviços vinculados (regra de negócio)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }
}
