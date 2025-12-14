using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Documents.Tests.Integration.Mocks;
using MeAjudaAi.Shared.Time;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Documents.Tests.Integration;

/// <summary>
/// Integration tests for Documents infrastructure components including persistence, repositories, and blob storage.
/// </summary>
[Trait("Category", "Integration")]
public class DocumentsInfrastructureIntegrationTests : IDisposable
{
    private readonly DocumentsDbContext _dbContext;
    private readonly IDocumentRepository _repository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IDocumentIntelligenceService _documentIntelligenceService;

    public DocumentsInfrastructureIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseInMemoryDatabase($"DocumentsTestDb_{UuidGenerator.NewId()}")
            .Options;

        _dbContext = new DocumentsDbContext(options);
        _repository = new DocumentRepository(_dbContext);
        _blobStorageService = new MockBlobStorageService();
        _documentIntelligenceService = new MockDocumentIntelligenceService();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Repository Integration Tests

    [Fact]
    public async Task Repository_AddDocument_ShouldPersistToDatabase()
    {
        // Arrange
        var document = Document.Create(
            UuidGenerator.NewId(),
            EDocumentType.IdentityDocument,
            "test-document.pdf",
            "test-blob-path.pdf"
        );

        // Act
        await _repository.AddAsync(document);
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(document.Id);
        retrieved.Should().NotBeNull();
        retrieved!.ProviderId.Should().Be(document.ProviderId);
        retrieved.DocumentType.Should().Be(document.DocumentType);
        retrieved.FileUrl.Should().Be(document.FileUrl);
        retrieved.FileName.Should().Be(document.FileName);
    }

    [Fact]
    public async Task Repository_GetByProviderId_ShouldReturnAllDocuments()
    {
        // Arrange
        var providerId = UuidGenerator.NewId();
        var doc1 = Document.Create(providerId, EDocumentType.IdentityDocument, "doc1.pdf", "path1.pdf");
        var doc2 = Document.Create(providerId, EDocumentType.ProofOfResidence, "doc2.pdf", "path2.pdf");
        var doc3 = Document.Create(UuidGenerator.NewId(), EDocumentType.CriminalRecord, "doc3.pdf", "path3.pdf");

        await _repository.AddAsync(doc1);
        await _repository.AddAsync(doc2);
        await _repository.AddAsync(doc3);
        await _dbContext.SaveChangesAsync();

        // Act
        var documents = await _repository.GetByProviderIdAsync(providerId);

        // Assert
        documents.Should().HaveCount(2);
        documents.Should().Contain(d => d.Id == doc1.Id);
        documents.Should().Contain(d => d.Id == doc2.Id);
        documents.Should().NotContain(d => d.Id == doc3.Id);
    }

    [Fact]
    public async Task Repository_UpdateDocument_ShouldPersistChanges()
    {
        // Arrange
        var document = Document.Create(UuidGenerator.NewId(), EDocumentType.IdentityDocument, "test.pdf", "path.pdf");
        await _repository.AddAsync(document);
        await _dbContext.SaveChangesAsync();

        // Act - Mark as pending verification first
        document.MarkAsPendingVerification();
        document.MarkAsVerified("{\"documentNumber\":\"12345\"}");
        await _repository.UpdateAsync(document);
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(document.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(EDocumentStatus.Verified);
        retrieved.OcrData.Should().NotBeNullOrEmpty();
        retrieved.VerifiedAt.Should().NotBeNull();
    }

    #endregion

    #region Blob Storage Service Integration Tests

    [Fact]
    public async Task BlobStorage_GenerateUploadUrl_ShouldReturnValidUrl()
    {
        // Arrange
        var blobName = "test-document.pdf";
        var contentType = "application/pdf";

        // Act
        var (uploadUrl, expiresAt) = await _blobStorageService.GenerateUploadUrlAsync(blobName, contentType);

        // Assert
        uploadUrl.Should().NotBeNullOrEmpty();
        uploadUrl.Should().Contain(blobName);
        expiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task BlobStorage_GenerateDownloadUrl_ShouldReturnValidUrl()
    {
        // Arrange
        var blobName = "test-document.pdf";

        // Act
        var (downloadUrl, expiresAt) = await _blobStorageService.GenerateDownloadUrlAsync(blobName);

        // Assert
        downloadUrl.Should().NotBeNullOrEmpty();
        downloadUrl.Should().Contain(blobName);
        expiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task BlobStorage_ExistsAsync_AfterUpload_ShouldReturnTrue()
    {
        // Arrange
        var blobName = "existing-document.pdf";
        await _blobStorageService.GenerateUploadUrlAsync(blobName, "application/pdf");

        // Act
        var exists = await _blobStorageService.ExistsAsync(blobName);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task BlobStorage_DeleteAsync_ShouldRemoveBlob()
    {
        // Arrange
        var blobName = "to-delete.pdf";
        await _blobStorageService.GenerateUploadUrlAsync(blobName, "application/pdf");

        // Act
        await _blobStorageService.DeleteAsync(blobName);

        // Assert
        var exists = await _blobStorageService.ExistsAsync(blobName);
        exists.Should().BeFalse();
    }

    #endregion

    #region Document Intelligence Service Integration Tests

    [Fact]
    public async Task DocumentIntelligence_AnalyzeDocument_ShouldReturnSuccessResult()
    {
        // Arrange
        var blobUrl = "https://test.blob.core.windows.net/documents/test.pdf";
        var documentType = "identity";

        // Act
        var result = await _documentIntelligenceService.AnalyzeDocumentAsync(blobUrl, documentType);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ExtractedData.Should().NotBeNullOrEmpty();
        result.Fields.Should().NotBeNull();
        result.Confidence.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DocumentIntelligence_AnalyzeDocument_ShouldExtractFields()
    {
        // Arrange
        var blobUrl = "https://test.blob.core.windows.net/documents/identity.pdf";
        var documentType = "identity";

        // Act
        var result = await _documentIntelligenceService.AnalyzeDocumentAsync(blobUrl, documentType);

        // Assert
        result.Fields.Should().NotBeNull();
        result.Fields.Should().ContainKey("documentNumber");
        result.Fields.Should().ContainKey("name");
    }

    #endregion

    #region Complete Workflow Integration Tests

    [Fact]
    public async Task CompleteWorkflow_UploadToVerification_ShouldWork()
    {
        // Arrange
        var providerId = UuidGenerator.NewId();
        var blobName = $"{providerId}/identity-{UuidGenerator.NewId()}.pdf";

        // Act 1: Generate upload URL
        var (uploadUrl, _) = await _blobStorageService.GenerateUploadUrlAsync(blobName, "application/pdf");
        uploadUrl.Should().NotBeNullOrEmpty();

        // Act 2: Create document entity
        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "identity.pdf", blobName);
        await _repository.AddAsync(document);
        await _dbContext.SaveChangesAsync();

        // Act 3: Verify blob exists
        var blobExists = await _blobStorageService.ExistsAsync(blobName);
        blobExists.Should().BeTrue();

        // Act 4: Mark as pending verification
        document.MarkAsPendingVerification();
        await _repository.UpdateAsync(document);
        await _dbContext.SaveChangesAsync();

        // Act 5: Analyze document
        var downloadUrl = (await _blobStorageService.GenerateDownloadUrlAsync(blobName)).DownloadUrl;
        var ocrResult = await _documentIntelligenceService.AnalyzeDocumentAsync(downloadUrl, "identity");

        // Act 6: Mark as verified
        document.MarkAsVerified(ocrResult.ExtractedData);
        await _repository.UpdateAsync(document);
        await _dbContext.SaveChangesAsync();

        // Assert - Verify complete workflow
        var finalDocument = await _repository.GetByIdAsync(document.Id);
        finalDocument.Should().NotBeNull();
        finalDocument!.Status.Should().Be(EDocumentStatus.Verified);
        finalDocument.OcrData.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task MultipleDocuments_ForSameProvider_ShouldBeManageable()
    {
        // Arrange
        var providerId = UuidGenerator.NewId();
        var documentTypes = new[]
        {
            EDocumentType.IdentityDocument,
            EDocumentType.ProofOfResidence,
            EDocumentType.CriminalRecord
        };

        // Act - Create multiple documents
        var documentIds = new List<Guid>();
        foreach (var docType in documentTypes)
        {
            var blobName = $"{providerId}/{docType}-{UuidGenerator.NewId()}.pdf";
            await _blobStorageService.GenerateUploadUrlAsync(blobName, "application/pdf");

            var document = Document.Create(providerId, docType, $"{docType}.pdf", blobName);
            await _repository.AddAsync(document);
            documentIds.Add(document.Id);
        }
        await _dbContext.SaveChangesAsync();

        // Assert - All documents retrievable
        var allDocs = await _repository.GetByProviderIdAsync(providerId);
        allDocs.Should().HaveCount(3);
        foreach (var docId in documentIds)
        {
            allDocs.Should().Contain(d => d.Id == docId);
        }
    }

    [Fact]
    public async Task DocumentVerificationFlow_WithRejection_ShouldWork()
    {
        // Arrange
        var document = Document.Create(UuidGenerator.NewId(), EDocumentType.IdentityDocument, "test.pdf", "path.pdf");
        await _repository.AddAsync(document);
        await _dbContext.SaveChangesAsync();

        // Act - Mark as pending then rejected
        document.MarkAsPendingVerification();
        document.MarkAsRejected("Document is blurry");
        await _repository.UpdateAsync(document);
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(document.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(EDocumentStatus.Rejected);
        retrieved.RejectionReason.Should().Be("Document is blurry");
    }

    [Fact]
    public async Task Repository_QueryByStatus_ShouldFilterCorrectly()
    {
        // Arrange
        var uploaded = Document.Create(UuidGenerator.NewId(), EDocumentType.IdentityDocument, "uploaded.pdf", "path1");

        var verified = Document.Create(UuidGenerator.NewId(), EDocumentType.ProofOfResidence, "verified.pdf", "path2");
        verified.MarkAsPendingVerification();
        verified.MarkAsVerified("{\"data\":\"test\"}");

        var rejected = Document.Create(UuidGenerator.NewId(), EDocumentType.CriminalRecord, "rejected.pdf", "path3");
        rejected.MarkAsPendingVerification();
        rejected.MarkAsRejected("Invalid");

        await _repository.AddAsync(uploaded);
        await _repository.AddAsync(verified);
        await _repository.AddAsync(rejected);
        await _dbContext.SaveChangesAsync();

        // Act
        var allDocs = await _dbContext.Documents.ToListAsync();

        // Assert
        allDocs.Should().HaveCount(3);
        allDocs.Count(d => d.Status == EDocumentStatus.Uploaded).Should().Be(1);
        allDocs.Count(d => d.Status == EDocumentStatus.Verified).Should().Be(1);
        allDocs.Count(d => d.Status == EDocumentStatus.Rejected).Should().Be(1);
    }

    [Fact]
    public async Task DbContext_MultipleSaves_ShouldPersistBoth()
    {
        // Arrange
        var doc1 = Document.Create(UuidGenerator.NewId(), EDocumentType.IdentityDocument, "doc1.pdf", "path1");
        var doc2 = Document.Create(UuidGenerator.NewId(), EDocumentType.ProofOfResidence, "doc2.pdf", "path2");

        // Act
        await _repository.AddAsync(doc1);
        await _repository.AddAsync(doc2);
        await _dbContext.SaveChangesAsync();

        // Assert
        var count = await _dbContext.Documents.CountAsync();
        count.Should().Be(2);
    }

    #endregion
}
