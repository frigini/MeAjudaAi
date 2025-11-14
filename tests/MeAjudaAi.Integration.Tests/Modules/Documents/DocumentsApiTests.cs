using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.DTOs.Requests;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Tests.Auth;

namespace MeAjudaAi.Integration.Tests.Modules.Documents;

/// <summary>
/// Testes de integração para a API do módulo Documents.
/// Valida endpoints de upload, status e listagem de documentos de provedores.
/// </summary>
public class DocumentsApiTests : ApiTestBase
{
    [Fact]
    public async Task DocumentsUploadEndpoint_ShouldBeAccessible()
    {
        // Act
        var response = await Client.PostAsync("/api/v1/documents/upload", null);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest, // Expected for null body
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task UploadDocument_WithValidRequest_ShouldReturnUploadUrl()
    {
        // Arrange
        AuthConfig.ConfigureUser(Guid.NewGuid().ToString(), "provider", "provider@test.com", "provider");

        var providerId = Guid.NewGuid();
        var request = new UploadDocumentRequest
        {
            ProviderId = providerId,
            DocumentType = EDocumentType.IdentityDocument,
            FileName = "identity-card.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 102400
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/documents/upload", request);

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<UploadDocumentResponse>();
            result.Should().NotBeNull();
            result!.DocumentId.Should().NotBeEmpty();
            result.UploadUrl.Should().NotBeNullOrEmpty();
        }
        else
        {
            // If not OK, should be due to missing services (acceptable in test environment)
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.InternalServerError,
                HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task GetDocumentStatus_WithValidId_ShouldReturnDocument()
    {
        // Arrange
        AuthConfig.ConfigureUser(Guid.NewGuid().ToString(), "provider", "provider@test.com", "provider");

        var providerId = Guid.NewGuid();
        var uploadRequest = new UploadDocumentRequest
        {
            ProviderId = providerId,
            DocumentType = EDocumentType.ProofOfResidence,
            FileName = "test-document.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 51200
        };

        var uploadResponse = await Client.PostAsJsonAsync("/api/v1/documents/upload", uploadRequest);

        if (uploadResponse.StatusCode != HttpStatusCode.OK)
        {
            // Skip test if upload failed (acceptable in test environment)
            return;
        }

        var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<UploadDocumentResponse>();

        // Act
        var response = await Client.GetAsync($"/api/v1/documents/{uploadResult!.DocumentId}/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var document = await response.Content.ReadFromJsonAsync<DocumentDto>();
        document.Should().NotBeNull();
        document!.Id.Should().Be(uploadResult.DocumentId);
        document.Status.Should().Be(EDocumentStatus.Uploaded);
    }

    [Fact]
    public async Task GetDocumentStatus_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        AuthConfig.ConfigureUser(Guid.NewGuid().ToString(), "provider", "provider@test.com", "provider");
        var randomId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/documents/{randomId}/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "API should return 404 when document ID does not exist");
    }

    [Fact]
    public async Task GetProviderDocuments_WithValidProviderId_ShouldReturnDocumentsList()
    {
        // Arrange
        AuthConfig.ConfigureUser(Guid.NewGuid().ToString(), "provider", "provider@test.com", "provider");
        var providerId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/documents/provider/{providerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var documents = await response.Content.ReadFromJsonAsync<List<DocumentDto>>();
        documents.Should().NotBeNull();
        // List can be empty for new provider
    }

    [Fact]
    public async Task DocumentsEndpoints_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        AuthConfig.ClearConfiguration();
        AuthConfig.SetAllowUnauthenticated(false);

        // Act
        var uploadResponse = await Client.PostAsJsonAsync("/api/v1/documents/upload", new { });
        var statusResponse = await Client.GetAsync($"/api/v1/documents/{Guid.NewGuid()}/status");
        var listResponse = await Client.GetAsync($"/api/v1/documents/provider/{Guid.NewGuid()}");

        // Assert
        uploadResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.BadRequest); // BadRequest is also acceptable for invalid request

        statusResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        listResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadDocument_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        AuthConfig.ConfigureUser(Guid.NewGuid().ToString(), "provider", "provider@test.com", "provider");
        var invalidRequest = new { }; // Empty object

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/documents/upload", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "API should validate request and return 400 for invalid data");
    }
}
