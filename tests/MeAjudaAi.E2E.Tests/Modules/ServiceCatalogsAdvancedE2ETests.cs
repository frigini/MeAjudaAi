using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.Modules;

/// <summary>
/// Testes E2E para operações avançadas de Service Catalogs
/// Cobre os gaps de Validate e ChangeCategory
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "ServiceCatalogs")]
public class ServiceCatalogsAdvancedE2ETests : TestContainerTestBase
{
    /// <summary>
    /// Validates that a service can be successfully validated when it meets all business rules.
    /// </summary>
    [Fact]
    public async Task ValidateService_WithBusinessRules_Should_Succeed()
    {
        // Arrange
        AuthenticateAsAdmin();

        // Primeiro cria uma categoria
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var categoryRequest = new
        {
            Name = $"ValidateTestCategory_{uniqueId}",
            Description = "Category for validation tests",
            IsActive = true
        };

        var categoryResponse = await ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", categoryRequest, JsonOptions);

        categoryResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "Category creation is a precondition for this test. Response: {0}",
            await categoryResponse.Content.ReadAsStringAsync());

        var categoryLocation = categoryResponse.Headers.Location?.ToString();
        var categoryId = ExtractIdFromLocation(categoryLocation!);

        // Cria um serviço
        var serviceRequest = new
        {
            Name = $"ValidateTestService_{uniqueId}",
            Description = "Service for validation tests",
            CategoryId = categoryId,
            IsActive = true,
            Price = 100.00m,
            Duration = 60
        };

        var serviceResponse = await ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/services", serviceRequest, JsonOptions);

        serviceResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "Service creation is a precondition for this test. Response: {0}",
            await serviceResponse.Content.ReadAsStringAsync());

        var serviceLocation = serviceResponse.Headers.Location?.ToString();
        var serviceId = ExtractIdFromLocation(serviceLocation!);

        // Act - Validate service (API valida múltiplos serviços via array)
        var validateRequest = new
        {
            ServiceIds = new[] { serviceId }
        };

        var response = await ApiClient.PostAsJsonAsync(
            "/api/v1/service-catalogs/services/validate",
            validateRequest,
            JsonOptions);

        // Assert - Apenas status de sucesso (happy path)
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Validates that validating a non-existent service returns an appropriate error or validation result.
    /// </summary>
    [Fact]
    public async Task ValidateService_WithInvalidService_Should_ReturnErrorOrValidationResult()
    {
        // Arrange
        AuthenticateAsAdmin();
        var serviceId = Guid.NewGuid(); // Serviço inexistente

        var invalidRequest = new
        {
            ServiceIds = new[] { serviceId }
        };

        // Act - API deve retornar que o serviço é inválido ou OK com indicação de serviços inválidos
        var response = await ApiClient.PostAsJsonAsync(
            "/api/v1/service-catalogs/services/validate",
            invalidRequest,
            JsonOptions);

        // Assert - Pode retornar BadRequest, NotFound, ou OK com AllValid=false
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.OK);
    }

    /// <summary>
    /// Validates that a service category can be successfully changed and the relationship is updated.
    /// </summary>
    [Fact]
    public async Task ChangeServiceCategory_Should_UpdateRelationship()
    {
        // Arrange
        AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // Cria primeira categoria
        var category1Request = new
        {
            Name = $"OriginalCategory_{uniqueId}",
            Description = "Original category",
            IsActive = true
        };

        var category1Response = await ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", category1Request, JsonOptions);

        category1Response.StatusCode.Should().Be(HttpStatusCode.Created,
            "Category creation is a precondition for this test. Response: {0}",
            await category1Response.Content.ReadAsStringAsync());

        var category1Id = ExtractIdFromLocation(category1Response.Headers.Location!.ToString());

        // Cria segunda categoria
        var category2Request = new
        {
            Name = $"NewCategory_{uniqueId}",
            Description = "New category for service",
            IsActive = true
        };

        var category2Response = await ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", category2Request, JsonOptions);

        category2Response.StatusCode.Should().Be(HttpStatusCode.Created,
            "Category creation is a precondition for this test. Response: {0}",
            await category2Response.Content.ReadAsStringAsync());

        var category2Id = ExtractIdFromLocation(category2Response.Headers.Location!.ToString());

        // Cria um serviço na primeira categoria
        var serviceRequest = new
        {
            Name = $"MoveTestService_{uniqueId}",
            Description = "Service to be moved",
            CategoryId = category1Id,
            IsActive = true,
            Price = 150.00m,
            Duration = 90
        };

        var serviceResponse = await ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/services", serviceRequest, JsonOptions);

        serviceResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "Service creation is a precondition for this test. Response: {0}",
            await serviceResponse.Content.ReadAsStringAsync());

        var serviceId = ExtractIdFromLocation(serviceResponse.Headers.Location!.ToString());

        // Act - Change service category
        var changeCategoryRequest = new
        {
            NewCategoryId = category2Id
        };

        var response = await ApiClient.PostAsJsonAsync(
            $"/api/v1/service-catalogs/services/{serviceId}/change-category",
            changeCategoryRequest,
            JsonOptions);

        // Assert - Change is expected to succeed in this scenario
        response.StatusCode.Should().BeOneOf(
            [HttpStatusCode.OK, HttpStatusCode.NoContent],
            "the service category change should succeed in this scenario");

        // Verifica que o serviço está na nova categoria
        AuthenticateAsAdmin(); // Re-autenticar antes do GET
        var getServiceResponse = await ApiClient.GetAsync($"/api/v1/service-catalogs/services/{serviceId}");
        getServiceResponse.IsSuccessStatusCode.Should().BeTrue(
            "the updated service should be retrievable after changing category");

        var content = await getServiceResponse.Content.ReadAsStringAsync();
        content.Should().Contain(category2Id.ToString(),
            "the service should now be associated with the new category");
    }

    /// <summary>
    /// Validates that attempting to change a service to an inactive category returns a failure.
    /// </summary>
    [Fact]
    public async Task ChangeServiceCategory_ToInactiveCategory_Should_Fail()
    {
        // Arrange
        AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // Cria categoria ativa
        var activeCategoryRequest = new
        {
            Name = $"ActiveCategory_{uniqueId}",
            Description = "Active category",
            IsActive = true
        };

        var activeCategoryResponse = await ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", activeCategoryRequest, JsonOptions);

        activeCategoryResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "Active category creation is a precondition for this test. Response: {0}",
            await activeCategoryResponse.Content.ReadAsStringAsync());

        var activeCategoryId = ExtractIdFromLocation(activeCategoryResponse.Headers.Location!.ToString());

        // Cria categoria inativa
        var inactiveCategoryRequest = new
        {
            Name = $"InactiveCategory_{uniqueId}",
            Description = "Inactive category",
            IsActive = false
        };

        var inactiveCategoryResponse = await ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", inactiveCategoryRequest, JsonOptions);

        inactiveCategoryResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "Inactive category creation is a precondition for this test. Response: {0}",
            await inactiveCategoryResponse.Content.ReadAsStringAsync());

        var inactiveCategoryId = ExtractIdFromLocation(inactiveCategoryResponse.Headers.Location!.ToString());

        // Cria serviço na categoria ativa
        var serviceRequest = new
        {
            Name = $"TestService_{uniqueId}",
            Description = "Service for category change test",
            CategoryId = activeCategoryId,
            IsActive = true,
            Price = 200.00m,
            Duration = 120
        };

        var serviceResponse = await ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/services", serviceRequest, JsonOptions);

        serviceResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "Service creation is a precondition for this test. Response: {0}",
            await serviceResponse.Content.ReadAsStringAsync());

        var serviceId = ExtractIdFromLocation(serviceResponse.Headers.Location!.ToString());

        // Act - Tenta mudar para categoria inativa
        var changeCategoryRequest = new
        {
            NewCategoryId = inactiveCategoryId
        };

        var response = await ApiClient.PostAsJsonAsync(
            $"/api/v1/service-catalogs/services/{serviceId}/change-category",
            changeCategoryRequest,
            JsonOptions);

        // Assert - Must fail when trying to move to inactive category
        response.StatusCode.Should().BeOneOf(
            [HttpStatusCode.BadRequest, HttpStatusCode.Conflict, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound],
            "changing to an inactive category should be rejected");
    }

    /// <summary>
    /// Validates that attempting to change a service to a non-existent category returns NotFound.
    /// </summary>
    [Fact]
    public async Task ChangeServiceCategory_ToNonExistentCategory_Should_Return_NotFound()
    {
        // Arrange
        AuthenticateAsAdmin();
        var serviceId = Guid.NewGuid();
        var nonExistentCategoryId = Guid.NewGuid();

        var changeCategoryRequest = new
        {
            NewCategoryId = nonExistentCategoryId
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync(
            $"/api/v1/service-catalogs/services/{serviceId}/change-category",
            changeCategoryRequest,
            JsonOptions);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest);
    }
}
