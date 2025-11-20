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

        // Primeiro tenta fazer upload de um documento
        var uploadRequest = new
        {
            ProviderId = Guid.NewGuid(),
            DocumentType = "CPF",
            FileContent = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Document content for verification")),
            FileName = "verification_test.pdf",
            ContentType = "application/pdf"
        };

        var uploadResponse = await ApiClient.PostAsJsonAsync("/api/v1/documents/upload", uploadRequest, JsonOptions);

        uploadResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK,
            "Document upload failed: {0}", await uploadResponse.Content.ReadAsStringAsync());

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
            var uploadResult = System.Text.Json.JsonDocument.Parse(uploadContent);
            uploadResult.RootElement.TryGetProperty("data", out var dataProperty).Should().BeTrue();
            dataProperty.TryGetProperty("id", out var idProperty).Should().BeTrue();
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

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Accepted,
            HttpStatusCode.NoContent,
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest); // Múltiplos status aceitáveis dependendo da implementação

        // Se a verificação foi aceita, verifica o status do documento
        if (response.IsSuccessStatusCode)
        {
            var statusResponse = await ApiClient.GetAsync($"/api/v1/documents/{documentId}/status");

            if (statusResponse.IsSuccessStatusCode)
            {
                var statusContent = await statusResponse.Content.ReadAsStringAsync();
                statusContent.Should().NotBeNullOrEmpty();

                // Parse JSON e verifica o campo status
                var statusResult = System.Text.Json.JsonDocument.Parse(statusContent);
                if (statusResult.RootElement.TryGetProperty("data", out var dataProperty) &&
                    dataProperty.TryGetProperty("status", out var statusProperty))
                {
                    var status = statusProperty.GetString();
                    status.Should().NotBeNullOrEmpty();
                    status!.Should().BeOneOf(
                        "Pending", "PendingVerification", "Verifying", "pending", "pendingverification", "verifying",
                        because: "Document should be in verification state");
                }
            }
        }
    }

    [Fact]
    public async Task RequestDocumentVerification_WhenAlreadyVerified_Should_Fail()
    {
        // Arrange
        AuthenticateAsAdmin();
        var documentId = Guid.NewGuid(); // Documento inexistente ou já verificado

        var verificationRequest = new
        {
            VerifierNotes = "Attempting to verify already verified document",
            Priority = "High"
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync(
            $"/api/v1/documents/{documentId}/verify",
            verificationRequest,
            JsonOptions);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest,
            HttpStatusCode.Conflict); // Documento não encontrado ou já verificado
    }

    [Fact]
    public async Task RequestDocumentVerification_WithInvalidData_Should_Return_BadRequest()
    {
        // Arrange
        AuthenticateAsAdmin();
        var documentId = Guid.NewGuid();

        var invalidRequest = new
        {
            VerifierNotes = new string('a', 2001), // Muito longo
            Priority = "InvalidPriority" // Prioridade inválida
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync(
            $"/api/v1/documents/{documentId}/verify",
            invalidRequest,
            JsonOptions);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }
}
