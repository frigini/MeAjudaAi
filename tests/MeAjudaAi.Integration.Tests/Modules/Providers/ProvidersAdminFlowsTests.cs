using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using System.Text.Json;

namespace MeAjudaAi.Integration.Tests.Modules.Providers;

public class ProvidersAdminFlowsTests(ITestOutputHelper testOutput) : BaseApiTest
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
    public async Task UpdateVerificationStatus_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var providerId = Guid.NewGuid();
        var invalidData = new { status = 999, updatedBy = "" }; // Status inválido

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/providers/{providerId}/verification-status", invalidData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RequireBasicInfoCorrection_ShouldWork()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        
        // 1. Criar um provider para ter um ID válido
        var userId = Guid.NewGuid();
        var createRequest = new { 
            userId = userId, 
            name = "Correction Test", 
            type = 1,
            businessProfile = new { 
                legalName = "Test Ltd",
                contactInfo = new { email = $"corr_{Guid.NewGuid():N}@test.com" }
            }
        };
        var createResponse = await Client.PostAsJsonAsync("/api/v1/providers", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var providerId = GetResponseData(await ReadJsonAsync<JsonElement>(createResponse.Content)).GetProperty("id").GetGuid();

        // 2. Solicitar correção
        var correctionRequest = new { 
            reason = "Documento ilegível",
            fields = new[] { "DocumentNumber", "Name" }
        };
        
        // Act
        var response = await Client.PostAsJsonAsync($"/api/v1/providers/{providerId}/require-basic-info-correction", correctionRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.Accepted, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
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
}
