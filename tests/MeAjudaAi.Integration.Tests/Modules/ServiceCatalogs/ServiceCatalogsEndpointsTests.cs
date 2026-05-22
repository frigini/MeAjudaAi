using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;

namespace MeAjudaAi.Integration.Tests.Modules.ServiceCatalogs;

public class ServiceCatalogsEndpointsTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.ServiceCatalogs;

    [Fact]
    public async Task ServiceCatalogs_GetAllServices_ShouldReturnOk()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/service-catalogs/services");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ServiceCatalogs_GetCategories_ShouldReturnOk()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/service-catalogs/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ServiceCatalogs_FullAdminFlow_ShouldWork()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // 1. Create Category
        var catName = $"Category_{Guid.NewGuid():N}";
        var catResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", new { name = catName, displayOrder = 1 });
        catResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        var catId = GetResponseData(await ReadJsonAsync<JsonElement>(catResponse.Content)).GetProperty("id").GetGuid();

        // 2. Update Category
        var updateCatResponse = await Client.PutAsJsonAsync($"/api/v1/service-catalogs/categories/{catId}", new { name = catName + "_Updated", displayOrder = 2 });
        updateCatResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.OK);

        // 3. Create Service
        var svcName = $"Service_{Guid.NewGuid():N}";
        var svcResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/services", new { name = svcName, categoryId = catId });
        svcResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        var svcId = GetResponseData(await ReadJsonAsync<JsonElement>(svcResponse.Content)).GetProperty("id").GetGuid();

        // 4. Update Service
        var updateSvcResponse = await Client.PutAsJsonAsync($"/api/v1/service-catalogs/services/{svcId}", new { name = svcName + "_Updated", categoryId = catId });
        updateSvcResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.OK);

        // 5. Deactivate & Activate
        await Client.PostAsync($"/api/v1/service-catalogs/services/{svcId}/deactivate", null);
        await Client.PostAsync($"/api/v1/service-catalogs/services/{svcId}/activate", null);

        // 6. Delete
        var delSvcResponse = await Client.DeleteAsync($"/api/v1/service-catalogs/services/{svcId}");
        delSvcResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.BadRequest);

        var delCatResponse = await Client.DeleteAsync($"/api/v1/service-catalogs/categories/{catId}");
        delCatResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetServiceById_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await Client.GetAsync($"/api/v1/service-catalogs/services/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ServiceCatalogs_GetServicesByCategory_ShouldReturnOk()
    {
        // Act
        var response = await Client.GetAsync($"/api/v1/service-catalogs/categories/{Guid.NewGuid()}/services");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ServiceCatalogs_GetCategoriesWithCount_ShouldReturnOk()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/service-catalogs/categories/with-count");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ServiceCatalogs_ValidateServices_ShouldReturnOk()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/service-catalogs/services/validate", new { serviceIds = new[] { Guid.NewGuid() } });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ServiceCatalogs_ChangeServiceCategory_ShouldWork()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        var catName = $"Category_{Guid.NewGuid():N}";
        var catResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", new { name = catName, displayOrder = 1 });
        catResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        var catId = GetResponseData(await ReadJsonAsync<JsonElement>(catResponse.Content)).GetProperty("id").GetGuid();

        var targetCatName = $"TargetCategory_{Guid.NewGuid():N}";
        var targetCatResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", new { name = targetCatName, displayOrder = 2 });
        targetCatResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        var targetCatId = GetResponseData(await ReadJsonAsync<JsonElement>(targetCatResponse.Content)).GetProperty("id").GetGuid();

        var activateResponse = await Client.PostAsync($"/api/v1/service-catalogs/categories/{targetCatId}/activate", null);
        activateResponse.EnsureSuccessStatusCode();

        var svcName = $"Service_{Guid.NewGuid():N}";
        var svcResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/services", new { name = svcName, categoryId = catId });
        svcResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        var svcId = GetResponseData(await ReadJsonAsync<JsonElement>(svcResponse.Content)).GetProperty("id").GetGuid();

        // Act
        var response = await Client.PostAsJsonAsync($"/api/v1/service-catalogs/services/{svcId}/change-category", new { newCategoryId = targetCatId });

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.OK);
    }
}
