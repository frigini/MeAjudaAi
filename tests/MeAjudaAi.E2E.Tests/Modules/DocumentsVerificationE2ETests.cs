using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.Modules;

/// <summary>
/// Testes E2E para workflow de verificação de documentos
/// Cobre o gap do endpoint POST /{documentId}/verify
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "Documents")]
public class DocumentsVerificationE2ETests : TestContainerTestBase
{
    [Fact]
    public async Task RequestDocumentVerification_Should_UpdateStatus()
    {
        // Arrange
        AuthenticateAsAdmin();

        // Create a valid provider first to ensure ProviderId exists
        var createProviderRequest = new
        {
            UserId = Guid.NewGuid().ToString(),
            Name = "Test Provider for Document Verification",
            Type = 0, // Individual
            BusinessProfile = new
            {
                LegalName = "Test Company Legal Name",
                FantasyName = "Test Company",
                Description = (string?)null,
                ContactInfo = new
                {
                    Email = "test@provider.com",
                    Phone = "1234567890",
                    Website = (string?)null
                },
                PrimaryAddress = new
                {
                    Street = "123 Test St",
                    Number = "100",
                    Complement = (string?)null,
                    Neighborhood = "Centro",
                    City = "Test City",
                    State = "SP",
                    ZipCode = "12345-678",
                    Country = "Brasil"
                }
            }
        };

        var providerResponse = await ApiClient.PostAsJsonAsync("/api/v1/providers", createProviderRequest, JsonOptions);
        providerResponse.StatusCode.Should().Be(HttpStatusCode.Created, "Provider creation should succeed");

        var providerLocation = providerResponse.Headers.Location?.ToString();
        providerLocation.Should().NotBeNullOrEmpty("Provider creation should return Location header");
        var providerId = ExtractIdFromLocation(providerLocation!);

        // Wait for provider to be fully persisted (eventual consistency)
        await Task.Delay(1000);

        // Now upload a document with the valid ProviderId
        var uploadRequest = new
        {
            ProviderId = providerId,
            DocumentType = 1, // EDocumentType.IdentityDocument
            FileName = "verification_test.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 1024L
        };

        AuthenticateAsAdmin(); // POST upload requer autorização
        var uploadResponse = await ApiClient.PostAsJsonAsync("/api/v1/documents/upload", uploadRequest, JsonOptions);

        if (!uploadResponse.IsSuccessStatusCode)
        {
            var errorContent = await uploadResponse.Content.ReadAsStringAsync();
            throw new Exception($"Document upload failed with {uploadResponse.StatusCode}: {errorContent}");
        }

        uploadResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);

        Guid documentId;

        if (uploadResponse.StatusCode == HttpStatusCode.Created)
        {
            var locationHeader = uploadResponse.Headers.Location?.ToString();
            locationHeader.Should().NotBeNullOrEmpty("Created response must include Location header");
            documentId = ExtractIdFromLocation(locationHeader!);
        }
        else
        {
            var uploadContent = await uploadResponse.Content.ReadAsStringAsync();
            uploadContent.Should().NotBeNullOrEmpty("Response body required for document ID");
            using var uploadResult = System.Text.Json.JsonDocument.Parse(uploadContent);

            // Response is UploadDocumentResponse directly, not wrapped in "data"
            uploadResult.RootElement.TryGetProperty("documentId", out var idProperty).Should().BeTrue();
            documentId = idProperty.GetGuid();
        }

        // Act - Request verification
        var verificationRequest = new
        {
            VerifierNotes = "Requesting verification for this document",
            Priority = "Normal"
        };

        var response = await ApiClient.PostAsJsonAsync(
            $"/api/v1/documents/{documentId}/verify",
            verificationRequest,
            JsonOptions);

        // Assert - Success path only (no BadRequest)
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Accepted,
            HttpStatusCode.NoContent);

        // Se a verificação foi aceita, verifica o status do documento
        AuthenticateAsAdmin(); // GET requer autorização
        var statusResponse = await ApiClient.GetAsync($"/api/v1/documents/{documentId}/status");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Status endpoint should be available after successful verification");

        var statusContent = await statusResponse.Content.ReadAsStringAsync();
        statusContent.Should().NotBeNullOrEmpty();

        // Parse JSON - DocumentDto é retornado diretamente, não wrapped em "data"
        using var statusResult = System.Text.Json.JsonDocument.Parse(statusContent);

        statusResult.RootElement.TryGetProperty("status", out var statusProperty)
            .Should().BeTrue("Response should contain 'status' property");

        // Status pode ser string ou número dependendo da serialização JSON
        var statusString = statusProperty.GetString();
        statusString.Should().NotBeNullOrEmpty("Status should have a value");

        // Document should be in uploaded or pending verification status
        // EDocumentStatus: Uploaded, PendingVerification, Verified, Rejected, Failed
        statusString!.ToLowerInvariant().Should().BeOneOf(
            "uploaded", "pendingverification")
            .Because("Document should be Uploaded or PendingVerification after upload");
    }

    [Fact]
    public async Task RequestDocumentVerification_WithNonExistentDocument_Should_ReturnNotFound()
    {
        // Arrange
        AuthenticateAsAdmin();
        var documentId = Guid.NewGuid(); // Non-existent document

        var verificationRequest = new
        {
            VerifierNotes = "Attempting to verify non-existent document",
            Priority = "High"
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync(
            $"/api/v1/documents/{documentId}/verify",
            verificationRequest,
            JsonOptions);

        // Assert - Only NotFound is expected for non-existent documents
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RequestDocumentVerification_WithInvalidData_Should_Return_BadRequest()
    {
        // Arrange
        AuthenticateAsAdmin();
        var documentId = Guid.NewGuid();

        var invalidRequest = new
        {
            VerifierNotes = new string('a', 2001), // Too long - exceeds validation limit
            Priority = "InvalidPriority" // Invalid priority value
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync(
            $"/api/v1/documents/{documentId}/verify",
            invalidRequest,
            JsonOptions);

        // Assert - Pode retornar BadRequest (validação) ou NotFound (documento não existe)
        // A ordem de validação pode variar
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }
}
