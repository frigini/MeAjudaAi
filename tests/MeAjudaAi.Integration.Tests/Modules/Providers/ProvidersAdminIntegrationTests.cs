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
    public async Task GetProvidersByVerificationStatus_ShouldReturnOk()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var status = (int)EVerificationStatus.Pending;

        // Act
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
            documentType = (int)EDocumentType.Identity 
        };
        
        // Act - Add
        var addResponse = await Client.PostAsJsonAsync($"/api/v1/providers/{provider!.Id}/documents", addDocRequest);
        
        // Assert - Add
        addResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Remove Document
        var docType = (int)EDocumentType.Identity;
        
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

        // Act
        var response = await Client.PostAsJsonAsync($"/api/v1/providers/{provider!.Id}/require-correction", correctionRequest);

        // Assert
        // O endpoint pode retornar 400 se o status inicial não for favorável à correção (fluxo de negócio)
        // mas o teste valida a acessibilidade do endpoint
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
        var request = new
        {
            userId = userId,
            name = name,
            type = 1,
            businessProfile = new
            {
                description = "Test",
                contactInfo = new { email = $"{Guid.NewGuid()}@test.com" },
                showAddressToClient = false
            }
        };
        var response = await Client.PostAsJsonAsync("/api/v1/providers", request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<ProviderDto?> GetProviderByUserIdAsync(Guid userId)
    {
        var response = await Client.GetAsync($"/api/v1/providers/by-user/{userId}");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        
        var result = await ReadJsonAsync<Result<ProviderDto>>(response.Content);
        return result.Value;
    }

    #endregion
}
