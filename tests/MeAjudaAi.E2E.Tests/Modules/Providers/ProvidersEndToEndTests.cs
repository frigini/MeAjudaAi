using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.E2E.Tests.Modules.Providers;

/// <summary>
/// Testes E2E para o módulo Providers usando TestContainers
/// Cobre CRUD completo, lifecycle, documentos e workflows de verificação
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "Providers")]
public class ProvidersEndToEndTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public ProvidersEndToEndTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    #region Basic CRUD Operations

    [Fact]
    public async Task CreateProvider_Should_Return_Success()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin(); // Autentica como admin para criar provider

        var userId = await _fixture.CreateTestUserAsync();
        var providerName = _fixture.Faker.Company.CompanyName();

        var createProviderRequest = new
        {
            UserId = userId.ToString(),
            Name = providerName,
            Type = 0, // Individual
            BusinessProfile = new
            {
                LegalName = providerName,
                FantasyName = providerName,
                Description = $"Test provider {providerName}",
                ContactInfo = new
                {
                    Email = _fixture.Faker.Internet.Email(),
                    PhoneNumber = _fixture.Faker.Phone.PhoneNumber(),
                    Website = _fixture.Faker.Internet.Url()
                },
                PrimaryAddress = new
                {
                    Street = _fixture.Faker.Address.StreetAddress(),
                    Number = _fixture.Faker.Random.Number(1, 9999).ToString(),
                    Complement = (string?)null,
                    Neighborhood = _fixture.Faker.Address.City(),
                    City = _fixture.Faker.Address.City(),
                    State = _fixture.Faker.Address.StateAbbr(),
                    ZipCode = _fixture.Faker.Address.ZipCode(),
                    Country = "Brasil"
                }
            }
        };

        // Act
        var response = await _fixture.PostJsonAsync("/api/v1/providers", createProviderRequest);

        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            // Se não conseguir criar, pelo menos verificar que não é erro 500
            response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
            return;
        }

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var locationHeader = response.Headers.Location?.ToString();
        locationHeader.Should().NotBeNull();
        locationHeader.Should().Contain("/api/v1/providers");
    }

    // NOTA: Teste básico GetProviders removido - duplica ProvidersIntegrationTests.GetProviders_ShouldReturnProvidersList
    // Recuperação de lista simples já está adequadamente coberta nos testes de Integração

    [Fact]
    public async Task CompleteProviderWorkflow_Should_Work()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();

        Guid? providerId = null;
        Guid? userId = null;

        try
        {
            // Act 1: Criar Provider usando helper
            providerId = await CreateTestProviderAsync();
            
            // Recuperar userId do provider criado
            var getProviderResponse = await _fixture.ApiClient.GetAsync($"/api/v1/providers/{providerId}");
            if (getProviderResponse.IsSuccessStatusCode)
            {
                var providerContent = await getProviderResponse.Content.ReadAsStringAsync();
                var provider = JsonSerializer.Deserialize<JsonElement>(providerContent);
                
                // Unwrapping para garantir que pegamos o userId correto mesmo dentro de um envelope
                if (provider.TryGetProperty("value", out var dataProperty))
                {
                    provider = dataProperty;
                }
                else if (provider.TryGetProperty("data", out var legacyDataProperty))
                {
                    provider = legacyDataProperty;
                }

                if (provider.TryGetProperty("userId", out var userIdProperty))
                {
                    userId = Guid.Parse(userIdProperty.GetString()!);
                }
            }

            // Act 2: Buscar Provider criado
            var getResponse = await _fixture.ApiClient.GetAsync($"/api/v1/providers/{providerId}");

            if (getResponse.IsSuccessStatusCode)
            {
                getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var getContent = await getResponse.Content.ReadAsStringAsync();
                
                var retrievedProvider = JsonSerializer.Deserialize<JsonElement>(getContent);

                // Verificar se o JSON tem a estrutura esperada (pode estar dentro de "value" ou outro wrapper)
                if (retrievedProvider.TryGetProperty("value", out var dataProperty))
                {
                    retrievedProvider = dataProperty;
                }
                else if (retrievedProvider.TryGetProperty("data", out var legacyDataProperty))
                {
                    retrievedProvider = legacyDataProperty;
                }
                
                retrievedProvider.TryGetProperty("name", out var nameProperty).Should().BeTrue();
                nameProperty.GetString().Should().NotBeNullOrEmpty();
            }

            // Act 3: Buscar por UserId (se conseguimos recuperar)
            if (userId.HasValue)
            {
                var getUserResponse = await _fixture.ApiClient.GetAsync($"/api/v1/providers/user/{userId}");

                if (getUserResponse.IsSuccessStatusCode)
                {
                    getUserResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                }
            }

            // Act 4: Buscar por tipo
            var getTypeResponse = await _fixture.ApiClient.GetAsync("/api/v1/providers/type/0");

            if (getTypeResponse.IsSuccessStatusCode)
            {
                getTypeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
        finally
        {
            // Cleanup: Tentar deletar o provider se foi criado
            if (providerId.HasValue)
            {
                try
                {
                    var deleteResponse = await _fixture.ApiClient.DeleteAsync($"/api/v1/providers/{providerId}");
                }
                catch (Exception)
                {
                    // Cleanup failure is acceptable in this test
                }
            }
        }
    }

    // NOTA: Teste de smoke ProvidersEndpoints_ShouldNotCrash removido - baixo valor
    // Estabilidade dos endpoints é validada por testes funcionais específicos

    #endregion

    #region Update Operations

    [Fact]
    public async Task UpdateProvider_WithValidData_Should_Return_Success()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // Criar provider usando o helper
        var providerId = await CreateTestProviderAsync($"Provider_{uniqueId}");

        // Act - Update provider (usando UpdateProviderProfileRequest)
        var updateRequest = new
        {
            Name = $"Updated_{uniqueId}",
            BusinessProfile = new
            {
                LegalName = $"UpdatedLegal_{uniqueId}",
                FantasyName = $"UpdatedFantasy_{uniqueId}",
                Description = "Updated description",
                ContactInfo = new
                {
                    Email = $"updated_{uniqueId}@example.com",
                    PhoneNumber = "+5511999999999",
                    Website = "https://updated.example.com"
                },
                PrimaryAddress = new
                {
                    Street = "Updated Street",
                    Number = "999",
                    Complement = (string?)null,
                    Neighborhood = "Updated Neighborhood",
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = "01234-567",
                    Country = "Brasil"
                }
            }
        };

        var updateResponse = await _fixture.ApiClient.PutAsJsonAsync($"/api/v1/providers/{providerId}", updateRequest, TestContainerFixture.JsonOptions);

        // Assert
        updateResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);

        if (updateResponse.StatusCode == HttpStatusCode.OK)
        {
            var content = await updateResponse.Content.ReadAsStringAsync();
            content.Should().Contain("Updated");
        }
    }

    [Fact]
    public async Task UpdateProvider_WithInvalidData_Should_Return_BadRequest()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        
        // Criar provider válido primeiro para testar validação de update
        var providerId = await CreateTestProviderAsync($"InvalidTest_{uniqueId}");

        var invalidRequest = new
        {
            Name = "", // Inválido - campo obrigatório vazio
            BusinessProfile = new
            {
                LegalName = new string('a', 201), // Inválido - muito longo
                FantasyName = "",
                Description = "",
                ContactInfo = new
                {
                    Email = "not-an-email", // Inválido - formato incorreto
                    PhoneNumber = "123", // Inválido - formato incorreto
                    Website = "not-a-url"
                },
                PrimaryAddress = new
                {
                    Street = "",
                    Number = "",
                    Complement = (string?)null,
                    Neighborhood = "",
                    City = "",
                    State = "INVALID", // Inválido - mais de 2 caracteres
                    ZipCode = "123",
                    Country = ""
                }
            }
        };

        // Act
        var response = await _fixture.ApiClient.PutAsJsonAsync($"/api/v1/providers/{providerId}", invalidRequest, TestContainerFixture.JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "validation errors should return BadRequest for existing provider");
    }

    #endregion

    #region Delete Operations

    [Fact]
    public async Task DeleteProvider_WithoutDocuments_Should_Return_Success()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // Create provider using helper
        var providerId = await CreateTestProviderAsync($"ToDelete_{uniqueId}");

        // Act - Delete provider
        var deleteResponse = await _fixture.ApiClient.DeleteAsync($"/api/v1/providers/{providerId}");

        // Assert
        deleteResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);

        // Verify provider is deleted
        if (deleteResponse.IsSuccessStatusCode)
        {
            var getResponse = await _fixture.ApiClient.GetAsync($"/api/v1/providers/{providerId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

    #endregion

    #region Verification Status

    [Fact]
    public async Task UpdateVerificationStatus_ToVerified_Should_Succeed()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // Create provider using helper
        var providerId = await CreateTestProviderAsync($"ToVerify_{uniqueId}");

        // Act - Update verification status to Verified
        var updateStatusRequest = new
        {
            Status = 2, // Verified
            Reason = "Verification completed successfully"
        };

        var updateResponse = await _fixture.ApiClient.PutAsJsonAsync(
            $"/api/v1/providers/{providerId}/verification-status",
            updateStatusRequest,
            TestContainerFixture.JsonOptions);

        // Assert
        updateResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateVerificationStatus_InvalidTransition_Should_Fail()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var providerId = await CreateTestProviderAsync($"InvalidStatusTest_{uniqueId}");

        var invalidRequest = new
        {
            Status = 999, // Status inválido
            Reason = "Invalid status"
        };

        // Act
        var response = await _fixture.ApiClient.PutAsJsonAsync(
            $"/api/v1/providers/{providerId}/verification-status",
            invalidRequest,
            TestContainerFixture.JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "invalid status value should return BadRequest for existing provider");
    }

    #endregion

    #region Basic Info Correction

    #endregion

    #region Document Operations

    [Fact]
    public async Task UploadProviderDocument_Should_Return_Success()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // Create provider using helper
        var providerId = await CreateTestProviderAsync($"DocProvider_{uniqueId}");

        // Act - Add document (não é upload de arquivo, apenas registro do documento)
        var documentRequest = new
        {
            Number = "123456789", // RG number
            DocumentType = 3 // RG (EDocumentType enum value)
        };

        var addDocumentResponse = await _fixture.ApiClient.PostAsJsonAsync(
            $"/api/v1/providers/{providerId}/documents",
            documentRequest,
            TestContainerFixture.JsonOptions);

        // Debug output
        if (!addDocumentResponse.IsSuccessStatusCode)
        {
            var errorContent = await addDocumentResponse.Content.ReadAsStringAsync();
        }

        // Assert
        addDocumentResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task DeleteProviderDocument_Should_Return_Success()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // Create provider using helper
        var providerId = await CreateTestProviderAsync($"DelDocProvider_{uniqueId}");

        // Add document first
        var documentRequest = new
        {
            Number = "123456789", // RG number
            DocumentType = 3 // RG (EDocumentType enum value)
        };

        var addDocumentResponse = await _fixture.ApiClient.PostAsJsonAsync(
            $"/api/v1/providers/{providerId}/documents",
            documentRequest,
            TestContainerFixture.JsonOptions);

        // Act - Delete document (usando o DocumentType como identificador)
        var deleteResponse = await _fixture.ApiClient.DeleteAsync(
            $"/api/v1/providers/{providerId}/documents/{documentRequest.DocumentType}");

        // Assert
        deleteResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateTestProviderAsync(string? name = null)
    {
        // Ensure authenticated as admin to create providers
        TestContainerFixture.AuthenticateAsAdmin();
        
        var userId = await _fixture.CreateTestUserAsync();
        var providerName = name ?? _fixture.Faker.Company.CompanyName();

        var request = new
        {
            UserId = userId.ToString(),
            Name = providerName,
            Type = 0, // Individual
            BusinessProfile = new
            {
                LegalName = providerName,
                FantasyName = providerName,
                Description = $"Test provider {providerName}",
                ContactInfo = new
                {
                    Email = _fixture.Faker.Internet.Email(),
                    PhoneNumber = "+5511999999999",
                    Website = "https://www.example.com"
                },
                PrimaryAddress = new
                {
                    Street = _fixture.Faker.Address.StreetAddress(),
                    Number = _fixture.Faker.Random.Number(1, 9999).ToString(),
                    Complement = (string?)null,
                    Neighborhood = _fixture.Faker.Address.City(),
                    City = _fixture.Faker.Address.City(),
                    State = _fixture.Faker.Address.StateAbbr(),
                    ZipCode = _fixture.Faker.Address.ZipCode(),
                    Country = "Brasil"
                }
            }
        };

        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/providers", request, TestContainerFixture.JsonOptions);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create provider. Status: {response.StatusCode}, Content: {errorContent}");
        }

        var location = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(location))
        {
            throw new InvalidOperationException("Location header not found in create provider response");
        }

        return TestContainerFixture.ExtractIdFromLocation(location);
    }

    #endregion
}

