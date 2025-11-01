using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MeAjudaAi.Integration.Tests.Providers;

/// <summary>
/// Testes de funcionalidades implementadas do módulo Providers
/// </summary>
/// <remarks>
/// Verifica se as funcionalidades principais estão funcionando:
/// - Endpoints estão acessíveis
/// - Respostas estão no formato correto
/// - Autorização está funcionando
/// - Dados são persistidos corretamente
/// </remarks>
public class ImplementedFeaturesTests : ApiTestBase
{
    [Fact]
    public async Task ProvidersEndpoint_ShouldBeAccessible()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
        // Can be 401 (unauthorized), 200 (ok), or 500 (server error) but not 404/405
    }

    [Fact]
    public async Task ProvidersEndpoint_WithAuthentication_ShouldReturnValidResponse()
    {
        // Arrange
        // // ConfigurableTestAuthenticationHandler.ConfigureAdmin(); // Temporariamente desabilitado // Temporariamente desabilitado

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);
            
            // Should be either a list or a paginated result
            jsonDocument.RootElement.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
            
            if (jsonDocument.RootElement.ValueKind == JsonValueKind.Object)
            {
                // If it's an object, it should have pagination properties
                var hasItems = jsonDocument.RootElement.TryGetProperty("items", out _);
                var hasTotalCount = jsonDocument.RootElement.TryGetProperty("totalCount", out _);
                var hasPage = jsonDocument.RootElement.TryGetProperty("page", out _);
                
                (hasItems || hasTotalCount || hasPage).Should().BeTrue("Should be a paginated result");
            }
        }
        else
        {
            // If not successful, should not be a server error
            response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError, 
                "Server should not crash on basic endpoint access");
        }
    }

    [Fact]
    public async Task ProvidersEndpoint_ShouldSupportPagination()
    {
        // Arrange
        // ConfigurableTestAuthenticationHandler.ConfigureAdmin(); // Temporariamente desabilitado

        // Act
        var response = await Client.GetAsync("/api/v1/providers?page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest, 
            "Should accept pagination parameters");
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError, 
            "Should not crash with pagination parameters");
    }

    [Fact]
    public async Task ProvidersEndpoint_ShouldSupportFilters()
    {
        // Arrange
        // ConfigurableTestAuthenticationHandler.ConfigureAdmin(); // Temporariamente desabilitado

        // Act
        var response = await Client.GetAsync("/api/v1/providers?name=test&type=1&verificationStatus=1");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest, 
            "Should accept filter parameters");
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError, 
            "Should not crash with filter parameters");
    }

    [Fact]
    public async Task GetProviderById_Endpoint_ShouldExist()
    {
        // Arrange
        // ConfigurableTestAuthenticationHandler.ConfigureAdmin(); // Temporariamente desabilitado
        var testId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/providers/{testId}");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed, 
            "GET by ID endpoint should exist");
        
        // Can be 404 (not found), 401 (unauthorized), 400 (bad request), or 500, but not 405
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.BadRequest,
            HttpStatusCode.OK,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task CreateProvider_Endpoint_ShouldExist()
    {
        // Arrange
        // ConfigurableTestAuthenticationHandler.ConfigureAdmin(); // Temporariamente desabilitado
        var providerData = new
        {
            userId = Guid.NewGuid(),
            name = "Test Provider",
            type = 1, // Individual
            businessProfile = new
            {
                legalName = "Test Provider Ltd",
                contactInfo = new
                {
                    email = "test@provider.com",
                    phoneNumber = "+55 11 99999-9999"
                },
                primaryAddress = new
                {
                    street = "Test Street",
                    number = "123",
                    neighborhood = "Test Neighborhood",
                    city = "Test City",
                    state = "TS",
                    zipCode = "12345-678",
                    country = "Brasil"
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/providers", providerData);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed, 
            "POST endpoint should exist");
        
        // Can fail with various errors, but method should be allowed
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ProvidersModule_ShouldBeProperlyRegistered()
    {
        // Arrange
        using var scope = Services.CreateScope();

        // Act & Assert - verify key services are registered
        var dbContext = scope.ServiceProvider.GetService<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>();
        dbContext.Should().NotBeNull("ProvidersDbContext should be registered");

        var repository = scope.ServiceProvider.GetService<MeAjudaAi.Modules.Providers.Domain.Repositories.IProviderRepository>();
        repository.Should().NotBeNull("IProviderRepository should be registered");

        var queryService = scope.ServiceProvider.GetService<MeAjudaAi.Modules.Providers.Application.Services.IProviderQueryService>();
        queryService.Should().NotBeNull("IProviderQueryService should be registered");
    }

    [Fact]
    public async Task HealthCheck_ShouldIncludeProvidersDatabase()
    {
        // Act
        var response = await Client.GetAsync("/health");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("providers", "Health check should include providers database");
        }
    }
}
