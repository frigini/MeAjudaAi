using FluentAssertions;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Modules.Documents.Tests.Integration.Persistence;

/// <summary>
/// Testes de integração para DocumentRepository usando Testcontainers PostgreSQL.
/// Testes validam operações reais de banco de dados e comportamento do EF Core.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Module", "Documents")]
[Trait("Layer", "Infrastructure")]
public sealed class DocumentRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private DocumentsDbContext? _dbContext;
    private IDocumentRepository? _repository;

    public DocumentRepositoryIntegrationTests()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("documents_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(warnings => 
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        _dbContext = new DocumentsDbContext(options);
        await _dbContext.Database.MigrateAsync();
        
        _repository = new DocumentRepository(_dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }
        
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_WithValidDocument_ShouldPersistToDatabase()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test-file.pdf",
            "documents/test-file.pdf");

        // Act
        await _repository!.AddAsync(document);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(document.Id.Value);
        retrieved.Should().NotBeNull();
        retrieved!.ProviderId.Should().Be(providerId);
        retrieved.DocumentType.Should().Be(EDocumentType.IdentityDocument);
        retrieved.FileName.Should().Be("test-file.pdf");
        retrieved.Status.Should().Be(EDocumentStatus.Uploaded);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentDocument_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository!.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithExistingProvider_ShouldReturnAllDocuments()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var doc1 = Document.Create(providerId, EDocumentType.IdentityDocument, "id.pdf", "docs/id.pdf");
        var doc2 = Document.Create(providerId, EDocumentType.CriminalRecord, "cr.pdf", "docs/cr.pdf");

        await _repository!.AddAsync(doc1);
        await _repository.AddAsync(doc2);
        await _repository.SaveChangesAsync();

        // Act
        var documents = await _repository.GetByProviderIdAsync(providerId);

        // Assert
        documents.Should().HaveCount(2);
        documents.Should().Contain(d => d.DocumentType == EDocumentType.IdentityDocument);
        documents.Should().Contain(d => d.DocumentType == EDocumentType.CriminalRecord);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithNonExistentProvider_ShouldReturnEmpty()
    {
        // Arrange
        var nonExistentProviderId = Guid.NewGuid();

        // Act
        var documents = await _repository!.GetByProviderIdAsync(nonExistentProviderId);

        // Assert
        documents.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.ProofOfResidence,
            "residence.pdf",
            "docs/residence.pdf");

        await _repository!.AddAsync(document);
        await _repository.SaveChangesAsync();

        // Act - Mark as pending verification
        document.MarkAsPendingVerification();
        await _repository.UpdateAsync(document);
        await _repository.SaveChangesAsync();

        // Assert
        var updated = await _repository.GetByIdAsync(document.Id.Value);
        updated.Should().NotBeNull();
        updated!.Status.Should().Be(EDocumentStatus.PendingVerification);
    }

    [Fact]
    public async Task UpdateAsync_WithStatusChange_ShouldReflectInDatabase()
    {
        // Arrange
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "id-doc.pdf",
            "docs/id-doc.pdf");

        await _repository!.AddAsync(document);
        await _repository.SaveChangesAsync();

        document.MarkAsPendingVerification();
        await _repository.UpdateAsync(document);
        await _repository.SaveChangesAsync();

        // Act - Verify the document
        document.MarkAsVerified("{\"name\": \"Test User\"}");
        await _repository.UpdateAsync(document);
        await _repository.SaveChangesAsync();

        // Assert
        var verified = await _repository.GetByIdAsync(document.Id.Value);
        verified.Should().NotBeNull();
        verified!.Status.Should().Be(EDocumentStatus.Verified);
        verified.VerifiedAt.Should().NotBeNull();
        verified.OcrData.Should().Contain("Test User");
    }

    [Fact]
    public async Task QueryByStatus_ShouldFilterCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var doc1 = Document.Create(providerId, EDocumentType.IdentityDocument, "id.pdf", "docs/id.pdf");
        var doc2 = Document.Create(providerId, EDocumentType.ProofOfResidence, "proof.pdf", "docs/proof.pdf");
        var doc3 = Document.Create(providerId, EDocumentType.CriminalRecord, "cr.pdf", "docs/cr.pdf");

        doc1.MarkAsPendingVerification();
        doc2.MarkAsPendingVerification();
        doc2.MarkAsVerified();

        await _repository!.AddAsync(doc1);
        await _repository.AddAsync(doc2);
        await _repository.AddAsync(doc3);
        await _repository.SaveChangesAsync();

        // Act
        var pendingDocs = await _dbContext!.Documents
            .Where(d => d.Status == EDocumentStatus.PendingVerification)
            .ToListAsync();

        var uploadedDocs = await _dbContext.Documents
            .Where(d => d.Status == EDocumentStatus.Uploaded)
            .ToListAsync();

        var verifiedDocs = await _dbContext.Documents
            .Where(d => d.Status == EDocumentStatus.Verified)
            .ToListAsync();

        // Assert
        pendingDocs.Should().HaveCount(1);
        uploadedDocs.Should().HaveCount(1);
        verifiedDocs.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddAsync_WithMultipleDocuments_ShouldMaintainSeparateStates()
    {
        // Arrange
        var provider1 = Guid.NewGuid();
        var provider2 = Guid.NewGuid();

        var doc1 = Document.Create(provider1, EDocumentType.IdentityDocument, "id1.pdf", "docs/id1.pdf");
        var doc2 = Document.Create(provider2, EDocumentType.IdentityDocument, "id2.pdf", "docs/id2.pdf");

        // Act
        await _repository!.AddAsync(doc1);
        await _repository.AddAsync(doc2);
        await _repository.SaveChangesAsync();

        // Assert
        var provider1Docs = await _repository.GetByProviderIdAsync(provider1);
        var provider2Docs = await _repository.GetByProviderIdAsync(provider2);

        provider1Docs.Should().HaveCount(1);
        provider2Docs.Should().HaveCount(1);
        provider1Docs.First().ProviderId.Should().Be(provider1);
        provider2Docs.First().ProviderId.Should().Be(provider2);
    }

    [Fact]
    public async Task UpdateAsync_WithRejection_ShouldPersistReason()
    {
        // Arrange
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "id.pdf",
            "docs/id.pdf");

        await _repository!.AddAsync(document);
        await _repository.SaveChangesAsync();

        document.MarkAsPendingVerification();
        await _repository.UpdateAsync(document);
        await _repository.SaveChangesAsync();

        // Act
        document.MarkAsRejected("Invalid document format");
        await _repository.UpdateAsync(document);
        await _repository.SaveChangesAsync();

        // Assert
        var rejected = await _repository.GetByIdAsync(document.Id.Value);
        rejected.Should().NotBeNull();
        rejected!.Status.Should().Be(EDocumentStatus.Rejected);
        rejected.RejectionReason.Should().Be("Invalid document format");
        rejected.VerifiedAt.Should().NotBeNull();
    }
}
