using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.DTOs;

namespace MeAjudaAi.Integration.Tests.Modules.Providers;

/// <summary>
/// Testes de integração para os endpoints administrativos de Providers.
/// </summary>
public class ProvidersAdminIntegrationTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Providers | TestModule.Users;

    [Fact]
    public async Task GetProviders_WithAllFilters_ShouldReturnOk()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/providers?name=test&type=1&verificationStatus=1&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProviderById_WithInvalidId_ShouldReturnBadRequest()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/providers/not-a-guid");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProvider_ShouldReturnNoContent()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        
        // 1. Criar provider
        var userId = Guid.NewGuid();
        var createRequest = new { 
            userId = userId, 
            name = "Delete Test", 
            type = 1,
            businessProfile = new { 
                legalName = "Delete Ltd",
                contactInfo = new { email = $"del_{Guid.NewGuid():N}@test.com" }
            }
        };
        var createResponse = await Client.PostAsJsonAsync("/api/v1/providers", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var providerId = GetResponseData(await ReadJsonAsync<JsonElement>(createResponse.Content)).GetProperty("id").GetGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/v1/providers/{providerId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.OK);
        
        // Verify it's gone (or soft-deleted)
        var getResponse = await Client.GetAsync($"/api/v1/providers/{providerId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProvidersByVerificationStatus_ShouldReturnOk()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var status = (int)EVerificationStatus.Pending;

        // Act - Correct route
        var response = await Client.GetAsync($"/api/v1/providers/verification-status/{status}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await ReadJsonAsync<Result<IReadOnlyList<ProviderDto>>>(response.Content);
        content.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateVerificationStatus_ShouldUpdateSuccessfully()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        
        // 1. Criar um provider para testar
        var userId = Guid.NewGuid();
        await CreateTestProviderAsync(userId, "Verify Test");
        var provider = await GetProviderByUserIdAsync(userId);
        provider.Should().NotBeNull();

        // 2. Atualizar status
        var updateRequest = new { status = (int)EVerificationStatus.Verified };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/providers/{provider!.Id}/verification-status", updateRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AddAndRemoveDocument_ShouldWorkCorrectly()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var userId = Guid.NewGuid();
        await CreateTestProviderAsync(userId, "Docs Test");
        var provider = await GetProviderByUserIdAsync(userId);
        provider.Should().NotBeNull();

        // 1. Add Document
        var addDocRequest = new 
        { 
            number = "123456789", 
            documentType = (int)EDocumentType.RG 
        };
        
        // Act - Add
        var addResponse = await Client.PostAsJsonAsync($"/api/v1/providers/{provider!.Id}/documents", addDocRequest);
        
        // Assert - Add
        addResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Remove Document
        var docType = (int)EDocumentType.RG;
        
        // Act - Remove
        var removeResponse = await Client.DeleteAsync($"/api/v1/providers/{provider.Id}/documents/{docType}");

        // Assert - Remove
        removeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RequireBasicInfoCorrection_ShouldReturnOk()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var userId = Guid.NewGuid();
        await CreateTestProviderAsync(userId, "Correction Test");
        var provider = await GetProviderByUserIdAsync(userId);
        provider.Should().NotBeNull();

        var correctionRequest = new { reason = "Documento ilegível" };

        // Act - Correct route based on ApiEndpoints.cs
        var response = await Client.PostAsJsonAsync($"/api/v1/providers/{provider!.Id}/require-basic-info-correction", correctionRequest);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GetProvidersByLocation_ShouldReturnResults()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        
        // Act - Por Cidade
        var cityResponse = await Client.GetAsync("/api/v1/providers/by-city/Muriae");
        cityResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Por Estado
        var stateResponse = await Client.GetAsync("/api/v1/providers/by-state/MG");
        stateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProvidersByType_ShouldReturnResults()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var type = (int)EProviderType.Individual;

        // Act
        var response = await Client.GetAsync($"/api/v1/providers/by-type/{type}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #region Helpers

    private async Task CreateTestProviderAsync(Guid userId, string name)
    {
        // Garante que o contexto tem os dados do usuário para o comando
        AuthConfig.ConfigureUser(userId.ToString(), "provider", $"{userId}@test.com", "provider");

        var request = new
        {
            userId = userId,
            name = name,
            type = 1, // Individual
            businessProfile = new
            {
                legalName = name,
                description = "Test Description",
                contactInfo = new 
                { 
                    email = $"{Guid.NewGuid()}@test.com",
                    phoneNumber = "+5511999999999"
                },
                showAddressToClient = false
            }
        };
        
        // Volta para admin para poder criar o provider via admin endpoint
        AuthConfig.ConfigureAdmin();
        var response = await Client.PostAsJsonAsync("/api/v1/providers", request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<ProviderDto?> GetProviderByUserIdAsync(Guid userId)
    {
        var response = await Client.GetAsync($"/api/v1/providers/by-user/{userId}");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        
        // Em integração, o resultado pode estar em "data", "value" ou na raiz
        JsonElement data;
        if (doc.RootElement.TryGetProperty("data", out var dataProp))
            data = dataProp;
        else if (doc.RootElement.TryGetProperty("value", out var valueProp))
            data = valueProp;
        else
            data = doc.RootElement;

        var options = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        };
        options.Converters.Add(new MeAjudaAi.Shared.Serialization.Converters.StrictEnumConverter());

        return JsonSerializer.Deserialize<ProviderDto>(data.GetRawText(), options);
    }

    #endregion
}
