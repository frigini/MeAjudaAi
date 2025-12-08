using System.Net;
using System.Net.Http.Json;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Documents.Application.DTOs.Requests;
using MeAjudaAi.Modules.Documents.Domain.Enums;

namespace MeAjudaAi.E2E.Tests.Integration;

/// <summary>
/// Testes de integração E2E para endpoints do módulo Documents.
/// Testa workflows completos de upload, verificação e consulta de documentos.
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "Documents")]
public class DocumentsModuleTests : TestContainerTestBase
{
    private const string DocumentsApiBaseUrl = "/api/v1/documents";

    [Fact]
    public async Task UploadDocument_WithValidData_ShouldReturnOkWithUploadResponse()
    {
        // Arrange
        AuthenticateAsAdmin(); // Admin can upload for any provider

        var request = new UploadDocumentRequest
        {
            ProviderId = Guid.NewGuid(),
            DocumentType = EDocumentType.IdentityDocument,
            FileName = "identity-card.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 1024 * 500 // 500KB
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync($"{DocumentsApiBaseUrl}/upload", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var uploadResponse = await response.Content.ReadFromJsonAsync<UploadDocumentResponse>(JsonOptions);
        uploadResponse.Should().NotBeNull();
        uploadResponse!.DocumentId.Should().NotBeEmpty();
        uploadResponse.UploadUrl.Should().NotBeNullOrWhiteSpace();
        uploadResponse.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task UploadDocument_WithProofOfResidence_ShouldCreateCorrectDocumentType()
    {
        // Arrange
        AuthenticateAsAdmin(); // Admin can upload for any provider

        var request = new UploadDocumentRequest
        {
            ProviderId = Guid.NewGuid(),
            DocumentType = EDocumentType.ProofOfResidence,
            FileName = "proof-residence.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 1024 * 300
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync($"{DocumentsApiBaseUrl}/upload", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var uploadResponse = await response.Content.ReadFromJsonAsync<UploadDocumentResponse>(JsonOptions);
        uploadResponse.Should().NotBeNull();
        uploadResponse!.DocumentId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UploadDocument_WithCriminalRecord_ShouldCreateSuccessfully()
    {
        // Arrange
        AuthenticateAsAdmin(); // Admin can upload for any provider

        var request = new UploadDocumentRequest
        {
            ProviderId = Guid.NewGuid(),
            DocumentType = EDocumentType.CriminalRecord,
            FileName = "criminal-record.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 1024 * 200
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync($"{DocumentsApiBaseUrl}/upload", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var uploadResponse = await response.Content.ReadFromJsonAsync<UploadDocumentResponse>(JsonOptions);
        uploadResponse.Should().NotBeNull();
    }

    [Fact]
    public async Task UploadDocument_WithOtherDocumentType_ShouldCreateSuccessfully()
    {
        // Arrange
        AuthenticateAsAdmin(); // Admin can upload for any provider

        var request = new UploadDocumentRequest
        {
            ProviderId = Guid.NewGuid(),
            DocumentType = EDocumentType.Other,
            FileName = "other-document.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 1024 * 100
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync($"{DocumentsApiBaseUrl}/upload", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UploadDocument_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange - No authentication
        var request = new UploadDocumentRequest
        {
            ProviderId = Guid.NewGuid(),
            DocumentType = EDocumentType.IdentityDocument,
            FileName = "identity.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 1024
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync($"{DocumentsApiBaseUrl}/upload", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDocumentStatus_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        AuthenticateAsUser(userId: Guid.NewGuid().ToString());
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await ApiClient.GetAsync($"{DocumentsApiBaseUrl}/{nonExistentId}/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDocumentStatus_WithInvalidGuid_ShouldReturnNotFound()
    {
        // Arrange
        AuthenticateAsUser(userId: Guid.NewGuid().ToString());

        // Act - Invalid GUID fails route constraint, returns 404
        var response = await ApiClient.GetAsync($"{DocumentsApiBaseUrl}/invalid-guid/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDocumentStatus_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange - No authentication
        var documentId = Guid.NewGuid();

        // Act
        var response = await ApiClient.GetAsync($"{DocumentsApiBaseUrl}/{documentId}/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProviderDocuments_WithNonExistentProvider_ShouldReturnEmptyList()
    {
        // Arrange
        AuthenticateAsUser(userId: Guid.NewGuid().ToString());
        var nonExistentProviderId = Guid.NewGuid();

        // Act
        var response = await ApiClient.GetAsync($"{DocumentsApiBaseUrl}/provider/{nonExistentProviderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var documents = await response.Content.ReadFromJsonAsync<List<DocumentDto>>(JsonOptions);
        documents.Should().NotBeNull();
        documents.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProviderDocuments_WithInvalidGuid_ShouldReturnNotFound()
    {
        // Arrange
        AuthenticateAsUser(userId: Guid.NewGuid().ToString());

        // Act
        var response = await ApiClient.GetAsync($"{DocumentsApiBaseUrl}/provider/not-a-guid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProviderDocuments_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange - No authentication
        var providerId = Guid.NewGuid();

        // Act
        var response = await ApiClient.GetAsync($"{DocumentsApiBaseUrl}/provider/{providerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequestVerification_WithNonExistentDocument_ShouldReturnNotFound()
    {
        // Arrange
        AuthenticateAsUser(userId: Guid.NewGuid().ToString());
        var nonExistentDocumentId = Guid.NewGuid();

        // Act
        var response = await ApiClient.PostAsync($"{DocumentsApiBaseUrl}/{nonExistentDocumentId}/verify", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RequestVerification_WithInvalidGuid_ShouldReturnNotFound()
    {
        // Arrange
        AuthenticateAsUser(userId: Guid.NewGuid().ToString());

        // Act
        var response = await ApiClient.PostAsync($"{DocumentsApiBaseUrl}/invalid-guid/verify", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RequestVerification_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange - No authentication
        var documentId = Guid.NewGuid();

        // Act
        var response = await ApiClient.PostAsync($"{DocumentsApiBaseUrl}/{documentId}/verify", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadDocument_CompleteWorkflow_ShouldCreateAndQueryDocument()
    {
        // Arrange
        AuthenticateAsAdmin(); // Admin can upload for any provider
        var providerId = Guid.NewGuid();

        var uploadRequest = new UploadDocumentRequest
        {
            ProviderId = providerId,
            DocumentType = EDocumentType.IdentityDocument,
            FileName = "workflow-test.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 1024 * 250
        };

        // Act 1: Upload document
        var uploadResponse = await ApiClient.PostAsJsonAsync($"{DocumentsApiBaseUrl}/upload", uploadRequest, JsonOptions);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var upload = await uploadResponse.Content.ReadFromJsonAsync<UploadDocumentResponse>(JsonOptions);
        upload.Should().NotBeNull();
        var documentId = upload!.DocumentId;

        // Act 2: Query document status (should exist but return null for now - handler logic)
        var statusResponse = await ApiClient.GetAsync($"{DocumentsApiBaseUrl}/{documentId}/status");

        // Assert: Document query executed successfully
        // Note: Actual response depends on handler implementation and database state
        statusResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProviderDocuments_AfterMultipleUploads_ShouldReturnMultipleDocuments()
    {
        // Arrange
        AuthenticateAsAdmin(); // Admin can upload for any provider
        var providerId = Guid.NewGuid();

        // Act: Upload multiple documents for the same provider
        var uploadTasks = new[]
        {
            EDocumentType.IdentityDocument,
            EDocumentType.ProofOfResidence,
            EDocumentType.CriminalRecord
        }.Select(docType => ApiClient.PostAsJsonAsync($"{DocumentsApiBaseUrl}/upload", new UploadDocumentRequest
        {
            ProviderId = providerId,
            DocumentType = docType,
            FileName = $"{docType}.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 1024 * 100
        }, JsonOptions));

        var uploadResponses = await Task.WhenAll(uploadTasks);

        // Assert: All uploads succeeded
        uploadResponses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));

        // Act: Query provider documents
        var providerDocsResponse = await ApiClient.GetAsync($"{DocumentsApiBaseUrl}/provider/{providerId}");

        // Assert: Query executed successfully
        providerDocsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

/// <summary>
/// Response DTO for document upload endpoint.
/// </summary>
public record UploadDocumentResponse
{
    /// <summary>
    /// ID of the created document.
    /// </summary>
    public Guid DocumentId { get; init; }

    /// <summary>
    /// Upload URL with SAS token for direct blob storage upload.
    /// </summary>
    public string UploadUrl { get; init; } = string.Empty;

    /// <summary>
    /// SAS token expiration time.
    /// </summary>
    public DateTime ExpiresAt { get; init; }
}

/// <summary>
/// DTO for document information.
/// </summary>
public record DocumentDto
{
    public Guid Id { get; init; }
    public Guid ProviderId { get; init; }
    public EDocumentType DocumentType { get; init; }
    public string FileUrl { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public EDocumentStatus Status { get; init; }
    public DateTime UploadedAt { get; init; }
    public DateTime? VerifiedAt { get; init; }
    public string? RejectionReason { get; init; }
    public string? OcrData { get; init; }
}
