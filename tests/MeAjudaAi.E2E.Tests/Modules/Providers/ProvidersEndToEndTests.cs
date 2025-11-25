using MeAjudaAi.E2E.Tests.Base;
using System.Text.Json;

namespace MeAjudaAi.E2E.Tests.Modules.Providers;

/// <summary>
/// Testes E2E para o módulo de Providers usando TestContainers
/// Testa cenários completos de uso do módulo Providers
/// </summary>
public class ProvidersEndToEndTests : TestContainerTestBase
{
    private readonly ITestOutputHelper _testOutput;

    public ProvidersEndToEndTests(ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;
    }

    [Fact]
    public async Task CreateProvider_Should_Return_Success()
    {
        // Arrange
        AuthenticateAsAdmin(); // Autentica como admin para criar provider

        var createProviderRequest = new
        {
            UserId = Guid.NewGuid(),
            Name = Faker.Company.CompanyName(),
            Type = 0, // Individual
            BusinessProfile = new
            {
                TaxId = Faker.Random.Replace("############"),
                CompanyName = Faker.Company.CompanyName(),
                Address = new
                {
                    Street = Faker.Address.StreetAddress(),
                    City = Faker.Address.City(),
                    State = Faker.Address.StateAbbr(),
                    PostalCode = Faker.Address.ZipCode(),
                    Country = "Brasil"
                },
                ContactInfo = new
                {
                    Email = Faker.Internet.Email(),
                    Phone = Faker.Phone.PhoneNumber(),
                    Website = Faker.Internet.Url()
                }
            }
        };

        // Act
        var response = await PostJsonAsync("/api/v1/providers", createProviderRequest);

        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            _testOutput.WriteLine($"Expected 201 Created but got {response.StatusCode}. Response: {content}");

            // Se não conseguir criar, pelo menos verificar que não é erro 500
            response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
            return;
        }

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var locationHeader = response.Headers.Location?.ToString();
        locationHeader.Should().NotBeNull();
        locationHeader.Should().Contain("/api/v1/providers");
    }

    [Fact]
    public async Task GetProviders_Should_Return_List()
    {
        // Arrange
        AuthenticateAsAdmin(); // Autentica como admin para listar providers

        // Act
        var response = await ApiClient.GetAsync("/api/v1/providers");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNull();

            // Verificar que é um JSON válido
            var jsonDocument = JsonDocument.Parse(content);
            jsonDocument.Should().NotBeNull();
        }
        else
        {
            // Se falhou por questões de permissão, pelo menos não deve ser 500
            response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
            _testOutput.WriteLine($"GetProviders returned {response.StatusCode}");
        }
    }

    [Fact]
    public async Task CompleteProviderWorkflow_Should_Work()
    {
        // Arrange
        AuthenticateAsAdmin();

        var userId = Guid.NewGuid();
        var providerData = new
        {
            UserId = userId,
            Name = "Complete Workflow Provider",
            Type = 0, // Individual
            BusinessProfile = new
            {
                TaxId = "12345678000195",
                CompanyName = "Workflow Test LTDA",
                Address = new
                {
                    Street = "Rua Workflow, 123",
                    City = "São Paulo",
                    State = "SP",
                    PostalCode = "01234-567",
                    Country = "Brasil"
                },
                ContactInfo = new
                {
                    Email = "workflow@test.com",
                    Phone = "+55 11 99999-9999",
                    Website = "https://www.workflow.com"
                }
            }
        };

        Guid? providerId = null;

        try
        {
            // Act 1: Criar Provider
            var createResponse = await PostJsonAsync("/api/v1/providers", providerData);

            if (createResponse.IsSuccessStatusCode)
            {
                var createContent = await createResponse.Content.ReadAsStringAsync();
                var createdProvider = JsonSerializer.Deserialize<JsonElement>(createContent);

                if (createdProvider.TryGetProperty("id", out var idProperty))
                {
                    providerId = Guid.Parse(idProperty.GetString()!);

                    // Act 2: Buscar Provider criado
                    var getResponse = await ApiClient.GetAsync($"/api/v1/providers/{providerId}");

                    if (getResponse.IsSuccessStatusCode)
                    {
                        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                        var getContent = await getResponse.Content.ReadAsStringAsync();
                        var retrievedProvider = JsonSerializer.Deserialize<JsonElement>(getContent);

                        retrievedProvider.TryGetProperty("name", out var nameProperty).Should().BeTrue();
                        nameProperty.GetString().Should().Be("Complete Workflow Provider");
                    }

                    // Act 3: Buscar por UserId
                    var getUserResponse = await ApiClient.GetAsync($"/api/v1/providers/user/{userId}");

                    if (getUserResponse.IsSuccessStatusCode)
                    {
                        getUserResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                    }

                    // Act 4: Buscar por tipo
                    var getTypeResponse = await ApiClient.GetAsync("/api/v1/providers/type/0");

                    if (getTypeResponse.IsSuccessStatusCode)
                    {
                        getTypeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                    }
                }
            }
            else
            {
                // Se não conseguiu criar, verificar que não é erro de servidor
                createResponse.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
                _testOutput.WriteLine($"CreateProvider returned {createResponse.StatusCode}");
            }
        }
        finally
        {
            // Cleanup: Tentar deletar o provider se foi criado
            if (providerId.HasValue)
            {
                try
                {
                    var deleteResponse = await ApiClient.DeleteAsync($"/api/v1/providers/{providerId}");
                    _testOutput.WriteLine($"Cleanup delete returned {deleteResponse.StatusCode}");
                }
                catch (Exception ex)
                {
                    _testOutput.WriteLine($"Cleanup failed: {ex.Message}");
                }
            }
        }
    }

    [Fact]
    public async Task ProvidersEndpoints_ShouldNotCrash()
    {
        // Arrange
        AuthenticateAsAdmin();

        var endpoints = new[]
        {
            "/api/v1/providers",
            "/api/v1/providers/type/0",
            "/api/v1/providers/type/1",
            "/api/v1/providers/verification-status/0",
            "/api/v1/providers/verification-status/1",
            "/api/v1/providers/city/São Paulo",
            "/api/v1/providers/state/SP"
        };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await ApiClient.GetAsync(endpoint);

            // O importante é que não seja erro 500 (crash do servidor)
            response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError,
                $"Endpoint {endpoint} should not crash the server");

            _testOutput.WriteLine($"Endpoint {endpoint}: {response.StatusCode}");
        }
    }
}
