using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.Documents;

/// <summary>
/// Testes de integração completos para o módulo Documents.
/// Valida todo o fluxo desde repository até services.
/// </summary>
public class DocumentsIntegrationTests : ApiTestBase
{
    [Fact]
    public void DocumentRepository_ShouldBeRegisteredInDI()
    {
        // Arrange & Act
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetService<IDocumentRepository>();

        // Assert
        repository.Should().NotBeNull("IDocumentRepository should be registered");
    }

    [Fact]
    public void BlobStorageService_ShouldBeRegisteredInDI()
    {
        // Arrange & Act
        using var scope = Services.CreateScope();
        var blobService = scope.ServiceProvider.GetService<IBlobStorageService>();

        // Assert
        blobService.Should().NotBeNull("IBlobStorageService should be registered");
    }

    [Fact]
    public void DocumentIntelligenceService_ShouldBeRegisteredInDI()
    {
        // Arrange & Act
        using var scope = Services.CreateScope();
        var intelligenceService = scope.ServiceProvider.GetService<IDocumentIntelligenceService>();

        // Assert
        intelligenceService.Should().NotBeNull("IDocumentIntelligenceService should be registered");
    }

    [Fact]
    public async Task DocumentRepository_AddAndRetrieve_ShouldWorkCorrectly()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();

        var providerId = Guid.NewGuid();
        var document = MeAjudaAi.Modules.Documents.Domain.Entities.Document.Create(
            providerId,
            MeAjudaAi.Modules.Documents.Domain.Enums.EDocumentType.IdentityDocument,
            "integration-test.pdf",
            "https://storage.local/integration-test.pdf");

        // Act - Add
        await repository.AddAsync(document);

        // Act - Retrieve
        var retrieved = await repository.GetByIdAsync(document.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(document.Id);
        retrieved.ProviderId.Should().Be(providerId);
        retrieved.FileName.Should().Be("integration-test.pdf");

        // Cleanup
        using var dbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        dbContext.Documents.Remove(retrieved);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task DocumentRepository_GetByProviderIdAsync_ShouldReturnOnlyProviderDocuments()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();

        var provider1Id = Guid.NewGuid();
        var provider2Id = Guid.NewGuid();

        var doc1 = MeAjudaAi.Modules.Documents.Domain.Entities.Document.Create(
            provider1Id,
            MeAjudaAi.Modules.Documents.Domain.Enums.EDocumentType.IdentityDocument,
            "provider1-doc.pdf",
            "https://storage.local/provider1-doc.pdf");

        var doc2 = MeAjudaAi.Modules.Documents.Domain.Entities.Document.Create(
            provider2Id,
            MeAjudaAi.Modules.Documents.Domain.Enums.EDocumentType.ProofOfResidence,
            "provider2-doc.pdf",
            "https://storage.local/provider2-doc.pdf");

        await repository.AddAsync(doc1);
        await repository.AddAsync(doc2);

        // Act
        var provider1Docs = await repository.GetByProviderIdAsync(provider1Id);

        // Assert
        provider1Docs.Should().NotBeEmpty();
        provider1Docs.Should().AllSatisfy(d => d.ProviderId.Should().Be(provider1Id));
        provider1Docs.Should().NotContain(d => d.Id == doc2.Id);

        // Cleanup
        using var dbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        dbContext.Documents.RemoveRange(new[] { doc1, doc2 });
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task DocumentRepository_UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();

        var document = MeAjudaAi.Modules.Documents.Domain.Entities.Document.Create(
            Guid.NewGuid(),
            MeAjudaAi.Modules.Documents.Domain.Enums.EDocumentType.CriminalRecord,
            "update-test.pdf",
            "https://storage.local/update-test.pdf");

        await repository.AddAsync(document);

        // Act
        document.MarkAsVerified("{\"verified\":true}");
        await repository.UpdateAsync(document);

        // Assert
        var updated = await repository.GetByIdAsync(document.Id);
        updated.Should().NotBeNull();
        updated!.Status.Should().Be(MeAjudaAi.Modules.Documents.Domain.Enums.EDocumentStatus.Verified);

        // Cleanup
        using var dbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        dbContext.Documents.Remove(updated);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task BlobStorageService_GenerateUploadUrl_ShouldReturnValidUrlAndExpiry()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var blobService = scope.ServiceProvider.GetRequiredService<IBlobStorageService>();

        var blobName = $"test-{Guid.NewGuid()}.pdf";
        var contentType = "application/pdf";

        // Act
        var (uploadUrl, expiresAt) = await blobService.GenerateUploadUrlAsync(blobName, contentType);

        // Assert
        uploadUrl.Should().NotBeNullOrEmpty();
        expiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task BlobStorageService_GenerateDownloadUrl_ShouldReturnValidUrl()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var blobService = scope.ServiceProvider.GetRequiredService<IBlobStorageService>();

        var blobName = $"test-download-{Guid.NewGuid()}.pdf";

        // Act
        var (downloadUrl, expiresAt) = await blobService.GenerateDownloadUrlAsync(blobName);

        // Assert
        downloadUrl.Should().NotBeNullOrEmpty();
        expiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task DocumentsModule_ShouldSupportCompleteWorkflow()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        var blobService = scope.ServiceProvider.GetRequiredService<IBlobStorageService>();

        var providerId = Guid.NewGuid();
        var blobName = $"workflow-test-{Guid.NewGuid()}.pdf";

        // Step 1: Generate upload URL
        var (uploadUrl, _) = await blobService.GenerateUploadUrlAsync(blobName, "application/pdf");
        uploadUrl.Should().NotBeNullOrEmpty();

        // Step 2: Create document record
        var document = MeAjudaAi.Modules.Documents.Domain.Entities.Document.Create(
            providerId,
            MeAjudaAi.Modules.Documents.Domain.Enums.EDocumentType.IdentityDocument,
            "workflow-test.pdf",
            uploadUrl);

        await repository.AddAsync(document);

        // Step 3: Mark as verified (simulating OCR completion)
        document.MarkAsVerified("{\"ocr\":\"completed\"}");
        await repository.UpdateAsync(document);

        // Step 4: Retrieve and verify
        var retrieved = await repository.GetByIdAsync(document.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(MeAjudaAi.Modules.Documents.Domain.Enums.EDocumentStatus.Verified);

        // Cleanup
        using var dbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        dbContext.Documents.Remove(retrieved);
        await dbContext.SaveChangesAsync();
    }
}
