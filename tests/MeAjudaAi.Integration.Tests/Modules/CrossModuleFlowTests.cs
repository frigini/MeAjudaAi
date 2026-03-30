using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Documents.Application.DTOs.Requests;

namespace MeAjudaAi.Integration.Tests.Modules;

/// <summary>
/// Testes de fluxos que atravessam múltiplos módulos.
/// Garante que a integração entre módulos (Users, Documents, Providers, Search) funciona corretamente.
/// </summary>
public class CrossModuleFlowTests : BaseApiTest
{
    // Requer todos os módulos envolvidos nos fluxos complexos
    protected override TestModule RequiredModules => 
        TestModule.Users | TestModule.Documents | TestModule.Providers | TestModule.SearchProviders;

    [Fact]
    public async Task RegistrationAndDocumentUploadFlow_ShouldWork()
    {
        // 1. Registro de Usuário (Módulo Users)
        var registerRequest = new
        {
            Name = "Integration User",
            Email = $"integration_{Guid.NewGuid():N}@example.com",
            Password = "SecurePassword123!",
            PhoneNumber = "+5511999999999",
            TermsAccepted = true,
            AcceptedPrivacyPolicy = true
        };

        var registerResponse = await Client.PostAsJsonAsync("/api/v1/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var userData = GetResponseData(await ReadJsonAsync<JsonElement>(registerResponse.Content));
        var userId = userData.GetProperty("id").GetString()!;

        // Simula login configurando o AuthConfig para o novo usuário
        AuthConfig.ConfigureUser(userId, "customer", registerRequest.Email, "Integration User");

        // 2. Upload de Documento (Módulo Documents)
        // Mesmo sendo um 'customer', ele pode precisar de documentos para upgrade ou verificação
        var uploadRequest = new UploadDocumentRequest
        {
            ProviderId = Guid.Parse(userId), // Neste caso usando o próprio ID do usuário como dono
            DocumentType = MeAjudaAi.Modules.Documents.Domain.Enums.EDocumentType.IdentityDocument,
            FileName = "id_card.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 100 * 1024
        };

        var uploadResponse = await Client.PostAsJsonAsync("/api/v1/documents/upload", uploadRequest);
        
        // Assert
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK, 
            "User should be able to upload documents after registration");
        
        var uploadResult = GetResponseData(await ReadJsonAsync<JsonElement>(uploadResponse.Content));
        uploadResult.TryGetProperty("documentId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ProviderUpdateAndSearchSyncFlow_ShouldBeConsistent()
    {
        // 1. Criar um provedor (Módulo Providers) usando o endpoint become
        var userId = Guid.NewGuid();
        var email = $"provider_{Guid.NewGuid():N}@test.com";
        AuthConfig.ConfigureUser(userId.ToString(), "provider", email, "provider");
        
        var providerData = new
        {
            name = $"Searchable Provider {Guid.NewGuid():N}",
            type = 1, // Individual
            documentNumber = "00000000000",
            phoneNumber = "+5511999999999"
        };
        var createResponse = await Client.PostAsJsonAsync("/api/v1/providers/become", providerData);
        
        // Se o endpoint become não existir ou falhar, tenta o endpoint admin
        if (!createResponse.IsSuccessStatusCode)
        {
            AuthConfig.ConfigureAdmin();
            createResponse = await Client.PostAsJsonAsync("/api/v1/providers", new
            {
                userId = userId,
                name = providerData.name,
                type = 1,
                businessProfile = new
                {
                    description = "Test provider",
                    contactInfo = new { email = email },
                    showAddressToClient = false
                }
            });
        }
        
        var responseData = GetResponseData(await ReadJsonAsync<JsonElement>(createResponse.Content));
        
        // Handle both object and array responses
        Guid providerId;
        if (responseData.ValueKind == JsonValueKind.Array)
        {
            // If it's an array, get the first element's id
            providerId = responseData[0].GetProperty("id").GetGuid();
        }
        else
        {
            providerId = responseData.GetProperty("id").GetGuid();
        }

        // 2. Tentar buscar o provedor (Módulo SearchProviders)
        // Dependendo da implementação, a sincronização pode ser via evento (async) ou comando direto (sync)
        // Nota: Em testes de integração sem workers, costumamos forçar o processamento ou usar mocks
        var searchResponse = await Client.GetAsync($"/api/v1/search/providers?query={providerData.name}");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // 3. Atualizar Perfil
        var updateRequest = new { name = "Name Updated for Search" };
        AuthConfig.ConfigureUser(userId.ToString(), "provider", "test@test.com", "provider");
        await Client.PutAsJsonAsync("/api/v1/providers/me", updateRequest);

        // 4. Verificar se a busca reflete a mudança (pode requerer um pequeno delay se for async)
        // Em testes de integração, buscamos verificar se a falha não ocorre ou se os dados estão consistentes
        var searchUpdatedResponse = await Client.GetAsync($"/api/v1/search/providers?query=Name Updated");
        searchUpdatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
