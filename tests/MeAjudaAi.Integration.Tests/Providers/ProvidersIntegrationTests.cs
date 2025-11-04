using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Tests.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Providers;

/// <summary>
/// 🧪 TESTES DE INTEGRAÇÃO PARA O MÓDULO PROVIDERS
/// 
/// Valida as funcionalidades implementadas do módulo Providers:
/// - Criação de prestadores de serviços
/// - Consulta de prestadores
/// - Soft Delete de prestadores
/// - Gerenciamento de documentos e qualificações
/// </summary>
public class ProvidersIntegrationTests(ITestOutputHelper testOutput) : ApiTestBase
{
    [Fact]
    public async Task CreateProvider_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

        var providerData = new
        {
            userId = Guid.NewGuid(),
            name = "Test Provider Integration",
            type = 0, // Individual
            businessProfile = new
            {
                legalName = "Test Company LTDA",
                fantasyName = (string?)null,
                description = (string?)null,
                contactInfo = new
                {
                    email = "test@provider.com",
                    phoneNumber = "+55 11 99999-9999",
                    website = "https://www.provider.com"
                },
                primaryAddress = new
                {
                    street = "Rua Teste",
                    number = "123",
                    complement = (string?)null,
                    neighborhood = "Centro",
                    city = "São Paulo",
                    state = "SP",
                    zipCode = "01234-567",
                    country = "Brasil"
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/providers", providerData);

        // Assert
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var responseJson = JsonSerializer.Deserialize<JsonElement>(content);

            // Verifica se é uma response estruturada (com data)
            if (responseJson.TryGetProperty("data", out var dataElement))
            {
                Assert.True(dataElement.TryGetProperty("id", out _),
                    $"Response data does not contain 'id' property. Full response: {content}");
                Assert.True(dataElement.TryGetProperty("name", out var nameProperty));
                Assert.Equal("Test Provider Integration", nameProperty.GetString());

                // Cleanup - tentar deletar o provider criado
                if (dataElement.TryGetProperty("id", out var idProperty))
                {
                    var providerId = idProperty.GetString();
                    await Client.DeleteAsync($"/api/v1/providers/{providerId}");
                }
            }
            else
            {
                // Fallback para response direta
                Assert.True(responseJson.TryGetProperty("id", out _),
                    $"Response does not contain 'id' property. Full response: {content}");
                Assert.True(responseJson.TryGetProperty("name", out var nameProperty));
                Assert.Equal("Test Provider Integration", nameProperty.GetString());

                if (responseJson.TryGetProperty("id", out var idProperty))
                {
                    var providerId = idProperty.GetString();
                    await Client.DeleteAsync($"/api/v1/providers/{providerId}");
                }
            }
        }
        else
        {
            // Se falhou, capturar e exibir o erro real
            Assert.True(response.IsSuccessStatusCode,
                $"Expected success status code, but got {response.StatusCode}. Error content: {content}");
        }
    }

    [Fact]
    public async Task GetProviders_ShouldReturnProvidersList()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var providers = JsonSerializer.Deserialize<JsonElement>(content);

            // Verificar que retorna uma lista (mesmo que vazia)
            var isValidFormat = providers.ValueKind == JsonValueKind.Array ||
                               (providers.ValueKind == JsonValueKind.Object && providers.TryGetProperty("items", out _)) ||
                               (providers.ValueKind == JsonValueKind.Object && providers.TryGetProperty("data", out var dataElement) &&
                                (dataElement.ValueKind == JsonValueKind.Array || dataElement.TryGetProperty("items", out _)));

            Assert.True(isValidFormat,
                $"Expected array or object with 'items'/'data' property. Got: {content}");
        }
        else
        {
            // Se falhou, capturar e exibir o erro real
            Assert.True(response.IsSuccessStatusCode,
                $"Expected success status code, but got {response.StatusCode}. Error content: {content}");
        }
    }

    [Fact]
    public async Task GetProviderById_WithRandomId_ShouldNotReturnServerError()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();
        var randomId = Guid.NewGuid(); // Use random ID

        // Act
        var response = await Client.GetAsync($"/api/v1/providers/{randomId}");

        // Assert
        // Should not return server error - can be NotFound, OK, or other client errors
        Assert.NotEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);

        // Should be a valid response (not method not allowed, etc.)
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.OK ||
                   response.StatusCode == System.Net.HttpStatusCode.BadRequest,
                   $"Expected valid response code but got {response.StatusCode}");
    }

    [Fact]
    public async Task GetProvidersByType_ShouldReturnFilteredList()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

        // Act - Testar busca por tipo Individual (0)
        var response = await Client.GetAsync("/api/v1/providers/type/0");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var providers = JsonSerializer.Deserialize<JsonElement>(content);

            // Verificar que retorna uma lista
            Assert.True(providers.ValueKind == JsonValueKind.Array ||
                       (providers.ValueKind == JsonValueKind.Object && providers.TryGetProperty("items", out _)));
        }
        else
        {
            // Se falhou, verificar que não é erro 500
            Assert.NotEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }

    [Fact]
    public async Task GetProvidersByVerificationStatus_ShouldReturnFilteredList()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

        // Act - Testar busca por status Pending (0)
        var response = await Client.GetAsync("/api/v1/providers/verification-status/0");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var providers = JsonSerializer.Deserialize<JsonElement>(content);

            // Verificar que retorna uma lista
            Assert.True(providers.ValueKind == JsonValueKind.Array ||
                       (providers.ValueKind == JsonValueKind.Object && providers.TryGetProperty("items", out _)));
        }
        else
        {
            // Se falhou, verificar que não é erro 500
            Assert.NotEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }

    [Fact]
    public async Task ProvidersEndpoints_ShouldNotReturn500Errors()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

        var endpoints = new[]
        {
            "/api/v1/providers",
            "/api/v1/providers/type/0",
            "/api/v1/providers/verification-status/0",
            "/api/v1/providers/city/São Paulo",
            "/api/v1/providers/state/SP"
        };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await Client.GetAsync(endpoint);

            // O importante é que não seja erro 500 (erro de servidor)
            Assert.NotEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);

            // Log do status para debugging se necessário
            if (!response.IsSuccessStatusCode)
            {
                testOutput.WriteLine($"Endpoint {endpoint} returned {response.StatusCode}");
            }
        }
    }
}
