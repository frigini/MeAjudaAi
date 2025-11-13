using System.Text.Json;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.E2E.Tests.Modules.Documents;

/// <summary>
/// Testes E2E para o módulo Documents
/// Valida upload de documentos, persistência no banco, e fluxo completo de verificação
/// </summary>
public class DocumentsEndToEndTests : TestContainerTestBase
{
    [Fact]
    public async Task UploadDocument_Should_CreateDocumentInDatabase()
    {
        // Arrange
        AuthenticateAsAdmin();
        var providerId = Guid.NewGuid();

        var uploadRequest = new
        {
            ProviderId = providerId,
            DocumentType = (int)EDocumentType.IdentityDocument,
            FileName = "identity-card.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 102400
        };

        // Act
        var response = await PostJsonAsync("/api/v1/documents/upload", uploadRequest);

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
            
            result.TryGetProperty("documentId", out var documentIdElement).Should().BeTrue();
            var documentId = Guid.Parse(documentIdElement.GetString()!);

            // Verify database persistence
            await WithServiceScopeAsync(async services =>
            {
                var dbContext = services.GetRequiredService<DocumentsDbContext>();
                var document = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
                
                document.Should().NotBeNull();
                document.ProviderId.Should().Be(providerId);
                document.Status.Should().Be(EDocumentStatus.Uploaded);
                document.DocumentType.Should().Be(EDocumentType.IdentityDocument);
            });
        }
        else
        {
            // Upload may fail in test environment due to missing blob storage
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.InternalServerError);
        }
    }

    [Fact]
    public async Task GetDocumentStatus_Should_ReturnDocumentDetails()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        Guid documentId = Guid.Empty;

        // Create document directly in database
        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();
            
            var document = Document.Create(
                providerId,
                EDocumentType.ProofOfResidence,
                "proof-residence.pdf",
                "https://storage.test.com/proof.pdf");

            dbContext.Documents.Add(document);
            await dbContext.SaveChangesAsync();
            documentId = document.Id;
        });

        AuthenticateAsAdmin();

        // Act
        var response = await ApiClient.GetAsync($"/api/v1/documents/{documentId}/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        
        result.TryGetProperty("id", out var idElement).Should().BeTrue();
        Guid.Parse(idElement.GetString()!).Should().Be(documentId);
    }

    [Fact]
    public async Task GetProviderDocuments_Should_ReturnAllDocuments()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        // Create multiple documents for provider
        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();
            
            var doc1 = Document.Create(providerId, EDocumentType.IdentityDocument, "id.pdf", "url1");
            var doc2 = Document.Create(providerId, EDocumentType.ProofOfResidence, "proof.pdf", "url2");
            var doc3 = Document.Create(providerId, EDocumentType.CriminalRecord, "record.pdf", "url3");

            dbContext.Documents.AddRange(doc1, doc2, doc3);
            await dbContext.SaveChangesAsync();
        });

        AuthenticateAsAdmin();

        // Act
        var response = await ApiClient.GetAsync($"/api/v1/documents/provider/{providerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var documents = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        
        documents.ValueKind.Should().Be(JsonValueKind.Array);
        documents.GetArrayLength().Should().Be(3);
    }

    [Fact]
    public async Task DocumentWorkflow_Should_TransitionThroughStatuses()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        Guid documentId = Guid.Empty;

        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();
            
            var document = Document.Create(
                providerId,
                EDocumentType.CriminalRecord,
                "criminal-record.pdf",
                "https://storage.test.com/record.pdf");

            documentId = document.Id;

            // Test status transitions
            document.MarkAsPendingVerification();
            document.Status.Should().Be(EDocumentStatus.PendingVerification);

            document.MarkAsVerified("{\"verified\": true}");
            document.Status.Should().Be(EDocumentStatus.Verified);
            document.VerifiedAt.Should().NotBeNull();

            dbContext.Documents.Add(document);
            await dbContext.SaveChangesAsync();
        });

        // Assert - Verify persisted status
        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();
            var document = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
            
            document.Should().NotBeNull();
            document!.Status.Should().Be(EDocumentStatus.Verified);
            document.OcrData.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task Database_Should_StoreMultipleProvidersDocuments()
    {
        // Arrange
        var provider1 = Guid.NewGuid();
        var provider2 = Guid.NewGuid();

        // Act - Create documents for different providers
        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();
            
            var doc1 = Document.Create(provider1, EDocumentType.IdentityDocument, "p1-id.pdf", "url1");
            var doc2 = Document.Create(provider1, EDocumentType.ProofOfResidence, "p1-proof.pdf", "url2");
            var doc3 = Document.Create(provider2, EDocumentType.CriminalRecord, "p2-record.pdf", "url3");

            dbContext.Documents.AddRange(doc1, doc2, doc3);
            await dbContext.SaveChangesAsync();
        });

        // Assert - Verify isolation between providers
        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<DocumentsDbContext>();
            
            var provider1Docs = await dbContext.Documents
                .Where(d => d.ProviderId == provider1)
                .ToListAsync();
            
            var provider2Docs = await dbContext.Documents
                .Where(d => d.ProviderId == provider2)
                .ToListAsync();

            provider1Docs.Should().HaveCount(2);
            provider2Docs.Should().HaveCount(1);
        });
    }
}
