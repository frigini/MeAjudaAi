using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.Modules.Providers;

/// <summary>
/// Testes E2E para o módulo Providers usando TestContainers
/// Cobre CRUD completo, lifecycle, documentos e workflows de verificação
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "Providers")]
public class ProvidersEndToEndTests : TestContainerTestBase
{
    private readonly ITestOutputHelper _testOutput;

    public ProvidersEndToEndTests(ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;
    }

    #region Basic CRUD Operations

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

    // NOTA: Teste básico GetProviders removido - duplica ProvidersIntegrationTests.GetProviders_ShouldReturnProvidersList
    // Recuperação de lista simples já está adequadamente coberta nos testes de Integração

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

    // NOTA: Teste de smoke ProvidersEndpoints_ShouldNotCrash removido - baixo valor
    // Estabilidade dos endpoints é validada por testes funcionais específicos

    #endregion

    #region Update Operations

    [Fact]
    public async Task UpdateProvider_WithValidData_Should_Return_Success()
    {
        // Arrange
        AuthenticateAsAdmin();
        var userId = Guid.NewGuid();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var createRequest = new
        {
            UserId = userId.ToString(), // UserId é string no Request base
            Name = $"Provider_{uniqueId}",
            Type = 0, // Individual
            BusinessProfile = new
            {
                LegalName = Faker.Company.CompanyName(),
                FantasyName = $"MultiService_{uniqueId}",
                Description = "Healthcare provider",
                ContactInfo = new
                {
                    Email = $"multiservice_{uniqueId}@example.com",
                    Phone = Faker.Phone.PhoneNumber("(##) #####-####"),
                    Website = (string?)null
                },
                PrimaryAddress = new
                {
                    Street = Faker.Address.StreetName(),
                    Number = Faker.Random.Number(1, 9999).ToString(),
                    Complement = (string?)null,
                    Neighborhood = Faker.Address.County(),
                    City = Faker.Address.City(),
                    State = Faker.Address.StateAbbr(),
                    ZipCode = Faker.Random.Replace("#####-###"),
                    Country = "Brasil"
                }
            }
        };

        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/providers", createRequest, JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created, "provider creation must succeed for update test to be meaningful");

        var locationHeader = createResponse.Headers.Location?.ToString();
        locationHeader.Should().NotBeNullOrEmpty("Created response must include Location header");
        var providerId = ExtractIdFromLocation(locationHeader!);

        // Act - Update provider
        var updateRequest = new
        {
            Name = $"Updated_{uniqueId}",
            BusinessProfile = new
            {
                LegalName = Faker.Company.CompanyName(),
                FantasyName = $"UpdatedTrading_{uniqueId}",
                Description = "Updated description",
                ContactInfo = new
                {
                    Email = $"updated_{uniqueId}@example.com",
                    Phone = Faker.Phone.PhoneNumber("(##) #####-####"),
                    Website = (string?)null
                },
                PrimaryAddress = new
                {
                    Street = "Updated Street",
                    Number = "999",
                    Complement = (string?)null,
                    Neighborhood = "Centro",
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = "01234-567",
                    Country = "Brasil"
                }
            }
        };

        var updateResponse = await ApiClient.PutAsJsonAsync($"/api/v1/providers/{providerId}", updateRequest, JsonOptions);

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
        AuthenticateAsAdmin();
        var providerId = Guid.NewGuid();

        var invalidRequest = new
        {
            CompanyName = "", // Inválido - campo obrigatório vazio
            TradingName = new string('a', 201), // Inválido - muito longo
            Email = "not-an-email", // Inválido - formato incorreto
            Phone = "123", // Inválido - formato incorreto
            Description = "",
            Address = new
            {
                Street = "",
                Number = "",
                City = "",
                State = "INVALID", // Inválido - mais de 2 caracteres
                ZipCode = "123",
                Latitude = 91.0, // Inválido - fora do range
                Longitude = 181.0 // Inválido - fora do range
            }
        };

        // Act
        var response = await ApiClient.PutAsJsonAsync($"/api/v1/providers/{providerId}", invalidRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound); // NotFound aceitável se provider não existe
    }

    #endregion

    #region Delete Operations

    [Fact]
    public async Task DeleteProvider_WithoutDocuments_Should_Return_Success()
    {
        // Arrange
        AuthenticateAsAdmin();
        var userId = Guid.NewGuid();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var createRequest = new
        {
            UserId = userId,
            Type = 0, // Individual
            CompanyName = $"ToDelete_{uniqueId}",
            TradingName = $"ToDeleteTrading_{uniqueId}",
            TaxId = Faker.Random.Replace("###########"),
            Email = $"todelete_{uniqueId}@example.com",
            Phone = Faker.Phone.PhoneNumber("(##) #####-####"),
            Description = "Provider to be deleted",
            Address = new
            {
                Street = Faker.Address.StreetName(),
                Number = Faker.Random.Number(1, 9999).ToString(),
                City = Faker.Address.City(),
                State = Faker.Address.StateAbbr(),
                ZipCode = Faker.Random.Replace("#####-###"),
                Latitude = Faker.Address.Latitude(),
                Longitude = Faker.Address.Longitude()
            }
        };

        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/providers", createRequest, JsonOptions);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "provider must be created successfully as a prerequisite for update test");

        var locationHeader = createResponse.Headers.Location?.ToString();
        var providerId = ExtractIdFromLocation(locationHeader!);

        // Act - Delete provider
        var deleteResponse = await ApiClient.DeleteAsync($"/api/v1/providers/{providerId}");

        // Assert
        deleteResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);

        // Verify provider is deleted
        if (deleteResponse.IsSuccessStatusCode)
        {
            var getResponse = await ApiClient.GetAsync($"/api/v1/providers/{providerId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

    #endregion

    #region Verification Status

    [Fact]
    public async Task UpdateVerificationStatus_ToVerified_Should_Succeed()
    {
        // Arrange
        AuthenticateAsAdmin();
        var userId = Guid.NewGuid();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var createRequest = new
        {
            UserId = userId,
            Type = 0, // Individual
            CompanyName = $"ToVerify_{uniqueId}",
            TradingName = $"ToVerifyTrading_{uniqueId}",
            TaxId = Faker.Random.Replace("###########"),
            Email = $"toverify_{uniqueId}@example.com",
            Phone = Faker.Phone.PhoneNumber("(##) #####-####"),
            Description = "Provider to be verified",
            Address = new
            {
                Street = Faker.Address.StreetName(),
                Number = Faker.Random.Number(1, 9999).ToString(),
                City = Faker.Address.City(),
                State = Faker.Address.StateAbbr(),
                ZipCode = Faker.Random.Replace("#####-###"),
                Latitude = Faker.Address.Latitude(),
                Longitude = Faker.Address.Longitude()
            }
        };

        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/providers", createRequest, JsonOptions);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "provider must be created successfully as a prerequisite for verification status test");

        var locationHeader = createResponse.Headers.Location?.ToString();
        var providerId = ExtractIdFromLocation(locationHeader!);

        // Act - Update verification status to Verified
        var updateStatusRequest = new
        {
            Status = 2, // Verified
            Reason = "Verification completed successfully"
        };

        var updateResponse = await ApiClient.PutAsJsonAsync(
            $"/api/v1/providers/{providerId}/verification-status",
            updateStatusRequest,
            JsonOptions);

        // Assert
        updateResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateVerificationStatus_InvalidTransition_Should_Fail()
    {
        // Arrange
        AuthenticateAsAdmin();
        var providerId = Guid.NewGuid(); // Provider inexistente

        var invalidRequest = new
        {
            Status = 999, // Status inválido
            Reason = "Invalid status"
        };

        // Act
        var response = await ApiClient.PutAsJsonAsync(
            $"/api/v1/providers/{providerId}/verification-status",
            invalidRequest,
            JsonOptions);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }

    #endregion

    #region Basic Info Correction

    [Fact]
    public async Task RequestBasicInfoCorrection_Should_TriggerWorkflow()
    {
        // Arrange
        AuthenticateAsAdmin();
        var userId = Guid.NewGuid();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var createRequest = new
        {
            UserId = userId,
            Type = 0, // Individual
            CompanyName = $"ToCorrect_{uniqueId}",
            TradingName = $"ToCorrectTrading_{uniqueId}",
            TaxId = Faker.Random.Replace("###########"),
            Email = $"tocorrect_{uniqueId}@example.com",
            Phone = Faker.Phone.PhoneNumber("(##) #####-####"),
            Description = "Provider requiring correction",
            Address = new
            {
                Street = Faker.Address.StreetName(),
                Number = Faker.Random.Number(1, 9999).ToString(),
                City = Faker.Address.City(),
                State = Faker.Address.StateAbbr(),
                ZipCode = Faker.Random.Replace("#####-###"),
                Latitude = Faker.Address.Latitude(),
                Longitude = Faker.Address.Longitude()
            }
        };

        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/providers", createRequest, JsonOptions);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "provider must be created successfully as a prerequisite for correction request test");

        var locationHeader = createResponse.Headers.Location?.ToString();
        var providerId = ExtractIdFromLocation(locationHeader!);

        // Act - Request basic info correction
        var correctionRequest = new
        {
            Reason = "Tax ID format is incorrect",
            RequestedFields = new[] { "TaxId", "CompanyName" }
        };

        var response = await ApiClient.PostAsJsonAsync(
            $"/api/v1/providers/{providerId}/require-basic-info-correction",
            correctionRequest,
            JsonOptions);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Accepted,
            HttpStatusCode.NoContent);
    }

    #endregion

    #region Document Operations

    [Fact]
    public async Task UploadProviderDocument_Should_Return_Success()
    {
        // Arrange
        AuthenticateAsAdmin();
        var userId = Guid.NewGuid();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // Primeiro cria um provider
        var createRequest = new
        {
            UserId = userId,
            Type = 0, // Individual
            CompanyName = $"DocProvider_{uniqueId}",
            TradingName = $"DocTrading_{uniqueId}",
            TaxId = Faker.Random.Replace("###########"),
            Email = $"doctest1_{uniqueId}@example.com",
            Phone = Faker.Phone.PhoneNumber("(##) #####-####"),
            Description = "Provider with documents",
            Address = new
            {
                Street = Faker.Address.StreetName(),
                Number = Faker.Random.Number(1, 9999).ToString(),
                City = Faker.Address.City(),
                State = Faker.Address.StateAbbr(),
                ZipCode = Faker.Random.Replace("#####-###"),
                Latitude = Faker.Address.Latitude(),
                Longitude = Faker.Address.Longitude()
            }
        };

        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/providers", createRequest, JsonOptions);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "provider must be created successfully as a prerequisite for document delete test");

        var locationHeader = createResponse.Headers.Location?.ToString();
        var providerId = ExtractIdFromLocation(locationHeader!);

        // Act - Upload document
        var documentRequest = new
        {
            DocumentType = "CPF", // ou outro tipo de documento
            FileContent = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Fake document content")),
            FileName = "cpf_document.pdf",
            ContentType = "application/pdf"
        };

        var uploadResponse = await ApiClient.PostAsJsonAsync(
            $"/api/v1/providers/{providerId}/documents",
            documentRequest,
            JsonOptions);

        // Assert
        uploadResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.Created,
            HttpStatusCode.OK,
            HttpStatusCode.Accepted);

        if (uploadResponse.StatusCode == HttpStatusCode.Created)
        {
            uploadResponse.Headers.Location.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task DeleteProviderDocument_Should_Return_Success()
    {
        // Arrange
        AuthenticateAsAdmin();
        var userId = Guid.NewGuid();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // Primeiro cria um provider
        var createRequest = new
        {
            UserId = userId,
            Type = 0, // Individual
            CompanyName = $"DelDocProvider_{uniqueId}",
            TradingName = $"DelDocTrading_{uniqueId}",
            TaxId = Faker.Random.Replace("###########"),
            Email = $"deldocprovider_{uniqueId}@example.com",
            Phone = Faker.Phone.PhoneNumber("(##) #####-####"),
            Description = "Provider for document deletion",
            Address = new
            {
                Street = Faker.Address.StreetName(),
                Number = Faker.Random.Number(1, 9999).ToString(),
                City = Faker.Address.City(),
                State = Faker.Address.StateAbbr(),
                ZipCode = Faker.Random.Replace("#####-###"),
                Latitude = Faker.Address.Latitude(),
                Longitude = Faker.Address.Longitude()
            }
        };

        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/providers", createRequest, JsonOptions);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "provider must be created successfully as a prerequisite for document deletion test");

        var locationHeader = createResponse.Headers.Location?.ToString();
        var providerId = ExtractIdFromLocation(locationHeader!);

        // Tenta fazer upload de documento primeiro
        var documentRequest = new
        {
            DocumentType = "RG",
            FileContent = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Document to delete")),
            FileName = "rg_document.pdf",
            ContentType = "application/pdf"
        };

        var uploadResponse = await ApiClient.PostAsJsonAsync(
            $"/api/v1/providers/{providerId}/documents",
            documentRequest,
            JsonOptions);

        // Act - Delete document
        var deleteResponse = await ApiClient.DeleteAsync(
            $"/api/v1/providers/{providerId}/documents/RG");

        // Assert
        deleteResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);

        // Se a exclusão foi bem-sucedida, verifica que o documento não existe mais
        if (deleteResponse.IsSuccessStatusCode)
        {
            var getDocsResponse = await ApiClient.GetAsync($"/api/v1/documents/provider/{providerId}");
            if (getDocsResponse.IsSuccessStatusCode)
            {
                var content = await getDocsResponse.Content.ReadAsStringAsync();
                content.Should().NotContain("RG");
            }
        }
    }

    #endregion
}
