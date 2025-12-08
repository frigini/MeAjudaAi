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

    [Theory]
    [InlineData(EDocumentType.IdentityDocument, 512000)]
    [InlineData(EDocumentType.ProofOfResidence, 307200)]
    [InlineData(EDocumentType.CriminalRecord, 204800)]
    [InlineData(EDocumentType.Other, 102400)]
    public async Task UploadDocument_WithValidDocumentType_ShouldReturnOkWithUploadResponse(
        EDocumentType documentType,
        int fileSizeBytes)
    {
        // Arrange
        AuthenticateAsAdmin(); // Admin can upload for any provider

        var request = new UploadDocumentRequest
        {
            ProviderId = Guid.NewGuid(),
            DocumentType = documentType,
            FileName = $"{documentType}.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = fileSizeBytes
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

        // Act 2: Query document status
        var statusResponse = await ApiClient.GetAsync($"{DocumentsApiBaseUrl}/{documentId}/status");

        // Assert: Created document must be retrievable
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK, "uploaded document should be queryable immediately");

        var documentDto = await statusResponse.Content.ReadFromJsonAsync<DocumentDto>(JsonOptions);
        documentDto.Should().NotBeNull();
        documentDto!.Id.Should().Be(documentId);
        documentDto.ProviderId.Should().Be(providerId);
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

        var documents = await providerDocsResponse.Content.ReadFromJsonAsync<List<DocumentDto>>(JsonOptions);
        documents.Should().NotBeNull();
        documents.Should().HaveCount(3, "three documents were uploaded for this provider");
        documents.Should().OnlyContain(d => d.ProviderId == providerId);
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
/// DTO for document information returned by document query endpoints.
/// </summary>
public record DocumentDto
{
    /// <summary>
    /// Unique identifier of the document.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Identifier of the provider who uploaded the document.
    /// </summary>
    public Guid ProviderId { get; init; }

    /// <summary>
    /// Type of the document (e.g., IdentityDocument, ProofOfResidence, CriminalRecord).
    /// </summary>
    public EDocumentType DocumentType { get; init; }

    /// <summary>
    /// Blob storage identifier (blob name/key) for accessing the document file.
    /// </summary>
    public string FileUrl { get; init; } = string.Empty;

    /// <summary>
    /// Original filename provided during upload.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Current verification status of the document.
    /// </summary>
    public EDocumentStatus Status { get; init; }

    /// <summary>
    /// UTC timestamp when the document was uploaded.
    /// </summary>
    public DateTime UploadedAt { get; init; }

    /// <summary>
    /// UTC timestamp when the document was verified (null if not yet verified).
    /// </summary>
    public DateTime? VerifiedAt { get; init; }

    /// <summary>
    /// Reason for rejection if Status is Rejected or Failed; otherwise null.
    /// </summary>
    public string? RejectionReason { get; init; }

    /// <summary>
    /// OCR-extracted data in JSON format if verification included OCR; otherwise null.
    /// </summary>
    public string? OcrData { get; init; }
}
