using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Contracts.Modules.Documents.DTOs;

namespace MeAjudaAi.Integration.Tests.Modules.Documents;

/// <summary>
/// Testes de integração para a API do módulo Documents.
/// Valida endpoints de upload, status e listagem de documentos de provedores.
/// </summary>
public class DocumentsApiTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Documents;

    [Fact]
    public async Task UploadDocument_WithValidRequest_ShouldReturnUploadUrl()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        AuthConfig.ConfigureUser(providerId.ToString(), "provider", "provider@test.com", "provider");

        var request = new UploadDocumentRequest(
            providerId,
            "IdentityDocument",
            "identity-card.pdf",
            "application/pdf",
            102400);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/documents/upload", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "authenticated user uploading their own document should succeed");

        var result = await ReadJsonAsync<UploadDocumentResponse>(response.Content);
        result.Should().NotBeNull();
        result!.DocumentId.Should().NotBeEmpty();
        result.UploadUrl.Should().NotBeNullOrEmpty();
        result.BlobName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UploadDocument_WithMismatchedProviderId_ShouldReturnForbidden()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();
        var differentProviderId = Guid.NewGuid();
        AuthConfig.ConfigureUser(authenticatedUserId.ToString(), "provider", "provider@test.com", "provider");

        var request = new UploadDocumentRequest(
            differentProviderId,
            "IdentityDocument",
            "identity-card.pdf",
            "application/pdf",
            102400);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/documents/upload", request);

        // Assert
        response.StatusCode.Should().Match(code =>
            code == HttpStatusCode.Unauthorized ||
            code == HttpStatusCode.Forbidden,
            "user should not be able to upload documents for a different provider");
    }

    [Fact]
    public async Task DocumentsEndpoints_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        AuthConfig.ClearConfiguration();
        AuthConfig.SetAllowUnauthenticated(false);

        // Act
        var uploadResponse = await Client.PostAsJsonAsync("/api/v1/documents/upload", new { });
        var statusResponse = await Client.GetAsync($"/api/v1/documents/{Guid.NewGuid()}");
        var listResponse = await Client.GetAsync($"/api/v1/documents/provider/{Guid.NewGuid()}");

        // Assert
        uploadResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.BadRequest);

        statusResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        listResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadDocument_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        AuthConfig.ConfigureUser(Guid.NewGuid().ToString(), "provider", "provider@test.com", "provider");
        var invalidRequest = new { };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/documents/upload", invalidRequest);

        // Assert
        response.StatusCode.Should().Match(code =>
            code == HttpStatusCode.BadRequest ||
            code == HttpStatusCode.Unauthorized,
            "API should reject invalid request with 400 or 401");
    }

    [Theory]
    [InlineData("IdentityDocument")]
    [InlineData("ProofOfResidence")]
    [InlineData("CriminalRecord")]
    [InlineData("Other")]
    public async Task UploadDocument_WithDifferentDocumentTypes_ShouldAcceptAll(string docType)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        AuthConfig.ConfigureUser(providerId.ToString(), "provider", "provider@test.com", "provider");

        var request = new UploadDocumentRequest(
            providerId,
            docType,
            $"{docType}.pdf",
            "application/pdf",
            50000);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/documents/upload", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Document type {docType} should be accepted");
        var result = await ReadJsonAsync<UploadDocumentResponse>(response.Content);
        result.Should().NotBeNull();
        result!.DocumentId.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("application/pdf", "document.pdf")]
    [InlineData("image/jpeg", "document.jpeg")]
    [InlineData("image/png", "document.png")]
    public async Task UploadDocument_WithDifferentContentTypes_ShouldAcceptCommonFormats(
        string contentType, string fileName)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        AuthConfig.ConfigureUser(providerId.ToString(), "provider", "provider@test.com", "provider");

        var request = new UploadDocumentRequest(
            providerId,
            "IdentityDocument",
            fileName,
            contentType,
            50000);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/documents/upload", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Content type {contentType} should be accepted");
        var result = await ReadJsonAsync<UploadDocumentResponse>(response.Content);
        result.Should().NotBeNull();
        result!.DocumentId.Should().NotBeEmpty();
    }

    #region Document Lifecycle Endpoints

    [Fact]
    public async Task DocumentLifecycle_UploadRequestVerificationAndVerify_ShouldWork()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        AuthConfig.ConfigureUser(providerId.ToString(), "provider", "provider@test.com", "provider");

        // 1. Upload
        var uploadRequest = new UploadDocumentRequest(
            providerId,
            "IdentityDocument",
            "identity.pdf",
            "application/pdf",
            1024);
        var uploadResponse = await Client.PostAsJsonAsync("/api/v1/documents/upload", uploadRequest);
        var uploadData = await ReadJsonAsync<UploadDocumentResponse>(uploadResponse.Content);
        var documentId = uploadData!.DocumentId;

        // 2. Verify upload succeeded via response
        uploadData.Should().NotBeNull();
        uploadData!.DocumentId.Should().NotBeEmpty();

        // 3. Request Verification
        var requestVerificationResponse = await Client.PostAsJsonAsync($"/api/v1/documents/{documentId}/request-verification", new { });
        requestVerificationResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        // 4. Verify (Approve) - Needs Admin
        AuthConfig.ConfigureAdmin();
        var verifyData = new { IsVerified = true, VerificationNotes = "Looks good" };
        var verifyResponse = await Client.PostAsJsonAsync($"/api/v1/documents/{documentId}/verify", verifyData);
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. List Provider Documents
        var listResponse = await Client.GetAsync($"/api/v1/documents/provider/{providerId}");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var documents = GetResponseData(await ReadJsonAsync<JsonElement>(listResponse.Content));

        // Handle both array and object responses
        JsonElement itemsToCheck = documents;
        if (documents.ValueKind == JsonValueKind.Object && documents.TryGetProperty("items", out var items))
        {
            itemsToCheck = items;
        }

        if (itemsToCheck.ValueKind == JsonValueKind.Array)
        {
            var found = false;
            foreach (var item in itemsToCheck.EnumerateArray())
            {
                if (item.TryGetProperty("id", out var idProp) && idProp.GetString() == documentId.ToString())
                {
                    found = true;
                    break;
                }
            }
            found.Should().BeTrue();
        }
    }

    [Fact]
    public async Task VerifyDocument_Reject_ShouldUpdateStatus()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        AuthConfig.ConfigureUser(providerId.ToString(), "provider", "provider@test.com", "provider");

        var uploadRequest = new UploadDocumentRequest(providerId, "IdentityDocument", "id.pdf", "application/pdf", 100);
        var uploadResponse = await Client.PostAsJsonAsync("/api/v1/documents/upload", uploadRequest);
        var documentId = (await ReadJsonAsync<UploadDocumentResponse>(uploadResponse.Content))!.DocumentId;

        await Client.PostAsJsonAsync($"/api/v1/documents/{documentId}/request-verification", new { });

        // Act - Reject (Admin)
        AuthConfig.ConfigureAdmin();
        var rejectData = new { IsVerified = false, VerificationNotes = "Illegible" };
        var response = await Client.PostAsJsonAsync($"/api/v1/documents/{documentId}/verify", rejectData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
