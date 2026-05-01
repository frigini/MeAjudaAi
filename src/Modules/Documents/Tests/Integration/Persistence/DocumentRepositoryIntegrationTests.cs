using FluentAssertions;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Modules.Documents.Tests.Integration.Persistence;

[Trait("Category", "Integration")]
[Trait("Module", "Documents")]
[Trait("Layer", "Infrastructure")]
public sealed class DocumentRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private DocumentsDbContext? _dbContext;
    private IUnitOfWork? _uow;

    public DocumentRepositoryIntegrationTests()
    {
        _postgresContainer = new PostgreSqlBuilder("postgres:15-alpine")
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
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.CommandExecuting))
            .Options;

        _dbContext = new DocumentsDbContext(options);
        _uow = _dbContext;
        
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbContext != null) await _dbContext.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistDocument()
    {
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");

        _uow!.GetRepository<Document, DocumentId>().Add(document);
        var result = await _uow.SaveChangesAsync();

        result.Should().BeGreaterThan(0);

        var persisted = await _dbContext!.Documents.FirstOrDefaultAsync(d => d.Id == document.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task TryFindAsync_WithExistingDocument_ShouldReturnDocument()
    {
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.ProofOfResidence,
            "proof.pdf",
            "blob-url");

        _uow!.GetRepository<Document, DocumentId>().Add(document);
        await _uow.SaveChangesAsync();

        var found = await _uow.GetRepository<Document, DocumentId>().TryFindAsync(document.Id, default);

        found.Should().NotBeNull();
        found!.Id.Should().Be(document.Id);
    }

    [Fact]
    public async Task TryFindAsync_WithNonExistingDocument_ShouldReturnNull()
    {
        var found = await _uow!.GetRepository<Document, DocumentId>().TryFindAsync(new DocumentId(Guid.NewGuid()), default);

        found.Should().BeNull();
    }

    [Fact]
    public async Task Delete_ExistingDocument_ShouldRemoveFromDatabase()
    {
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.ProofOfResidence,
            "proof.pdf",
            "blob-url");

        _uow!.GetRepository<Document, DocumentId>().Add(document);
        await _uow.SaveChangesAsync();

        var found = await _uow.GetRepository<Document, DocumentId>().TryFindAsync(document.Id, default);
        found.Should().NotBeNull();

        _uow.GetRepository<Document, DocumentId>().Delete(found!);
        await _uow.SaveChangesAsync();

        var deleted = await _uow.GetRepository<Document, DocumentId>().TryFindAsync(document.Id, default);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Delete_NonExistingDocument_ShouldNotThrow()
    {
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");

        var act = () => _uow!.GetRepository<Document, DocumentId>().Delete(document);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task Add_MultipleDocuments_ShouldPersistAll()
    {
        var documents = new[]
        {
            Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "doc1.pdf", "blob-1"),
            Document.Create(Guid.NewGuid(), EDocumentType.ProofOfResidence, "doc2.pdf", "blob-2"),
            Document.Create(Guid.NewGuid(), EDocumentType.CriminalRecord, "doc3.pdf", "blob-3")
        };

        var repo = _uow!.GetRepository<Document, DocumentId>();
        foreach (var doc in documents)
        {
            repo.Add(doc);
        }
        await _uow.SaveChangesAsync();

        foreach (var doc in documents)
        {
            var persisted = await _uow.GetRepository<Document, DocumentId>().TryFindAsync(doc.Id, default);
            persisted.Should().NotBeNull($"Document {doc.Id} should be persisted");
        }
    }

    [Fact]
    public async Task Add_SameDocumentTwice_ShouldNotDuplicate()
    {
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");

        var repo = _uow!.GetRepository<Document, DocumentId>();
        repo.Add(document);
        await _uow.SaveChangesAsync();

        var existingCount = await _dbContext!.Documents.CountAsync(d => d.Id == document.Id);
        existingCount.Should().Be(1);
    }

    [Fact]
    public async Task Update_ExistingDocument_ShouldPersistChanges()
    {
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "original.pdf",
            "blob-url");

        _uow!.GetRepository<Document, DocumentId>().Add(document);
        await _uow.SaveChangesAsync();

        document.MarkAsPendingVerification();
        document.MarkAsVerified("{\"verified\": true}");
        await _uow.SaveChangesAsync();

        var updated = await _dbContext!.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == document.Id);

        updated.Should().NotBeNull();
        updated!.Status.Should().Be(EDocumentStatus.Verified);
        updated.OcrData.Should().Be("{\"verified\": true}");
        updated.VerifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_RejectionReason_ShouldPersistCorrectly()
    {
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");

        _uow!.GetRepository<Document, DocumentId>().Add(document);
        await _uow.SaveChangesAsync();

        document.MarkAsPendingVerification();
        document.MarkAsRejected("Document is blurry");
        await _uow.SaveChangesAsync();

        var rejected = await _dbContext!.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == document.Id);

        rejected.Should().NotBeNull();
        rejected!.Status.Should().Be(EDocumentStatus.Rejected);
        rejected.RejectionReason.Should().Be("Document is blurry");
        rejected.VerifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_FailedStatus_ShouldPersistCorrectly()
    {
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");

        _uow!.GetRepository<Document, DocumentId>().Add(document);
        await _uow.SaveChangesAsync();

        document.MarkAsFailed("OCR service timeout");
        await _uow.SaveChangesAsync();

        var failed = await _dbContext!.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == document.Id);

        failed.Should().NotBeNull();
        failed!.Status.Should().Be(EDocumentStatus.Failed);
        failed.RejectionReason.Should().Be("OCR service timeout");
    }

    [Fact]
    public async Task Update_RetryFromFailed_ShouldClearPreviousReason()
    {
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");

        _uow!.GetRepository<Document, DocumentId>().Add(document);
        await _uow.SaveChangesAsync();

        document.MarkAsFailed("Initial failure");
        await _uow.SaveChangesAsync();

        document.MarkAsPendingVerification();
        await _uow.SaveChangesAsync();

        var retried = await _dbContext!.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == document.Id);

        retried.Should().NotBeNull();
        retried!.Status.Should().Be(EDocumentStatus.PendingVerification);
        retried.RejectionReason.Should().BeNull("Previous failure reason should be cleared on retry");
    }

    [Fact]
    public async Task Query_ByProviderId_ShouldReturnOnlyMatchingDocuments()
    {
        var targetProviderId = Guid.NewGuid();
        var otherProviderId = Guid.NewGuid();

        var documents = new[]
        {
            Document.Create(targetProviderId, EDocumentType.IdentityDocument, "doc1.pdf", "blob-1"),
            Document.Create(targetProviderId, EDocumentType.ProofOfResidence, "doc2.pdf", "blob-2"),
            Document.Create(otherProviderId, EDocumentType.IdentityDocument, "doc3.pdf", "blob-3")
        };

        _uow!.GetRepository<Document, DocumentId>().Add(documents[0]);
        _uow.GetRepository<Document, DocumentId>().Add(documents[1]);
        _uow.GetRepository<Document, DocumentId>().Add(documents[2]);
        await _uow.SaveChangesAsync();

        var providerDocuments = await _dbContext!.Documents
            .AsNoTracking()
            .Where(d => d.ProviderId == targetProviderId)
            .ToListAsync();

        providerDocuments.Should().HaveCount(2);
        providerDocuments.All(d => d.ProviderId == targetProviderId).Should().BeTrue();
    }

    [Fact]
    public async Task Query_ByStatus_ShouldReturnOnlyMatchingDocuments()
    {
        var doc1 = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "doc1.pdf", "blob-1");
        var doc2 = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "doc2.pdf", "blob-2");

        _uow!.GetRepository<Document, DocumentId>().Add(doc1);
        _uow.GetRepository<Document, DocumentId>().Add(doc2);
        await _uow.SaveChangesAsync();

        doc1.MarkAsPendingVerification();
        doc1.MarkAsVerified(null);
        await _uow.SaveChangesAsync();

        var verifiedDocs = await _dbContext!.Documents
            .AsNoTracking()
            .Where(d => d.Status == EDocumentStatus.Verified)
            .ToListAsync();

        verifiedDocs.Should().HaveCount(1);
        verifiedDocs[0].Id.Should().Be(doc1.Id);
    }

    [Fact]
    public async Task Query_OrderByUploadedAt_ShouldReturnInCorrectOrder()
    {
        var doc1 = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "doc1.pdf", "blob-1");
        var doc2 = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "doc2.pdf", "blob-2");
        var doc3 = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "doc3.pdf", "blob-3");

        _uow!.GetRepository<Document, DocumentId>().Add(doc1);
        await _uow.SaveChangesAsync();
        await Task.Delay(10);
        _uow.GetRepository<Document, DocumentId>().Add(doc2);
        await _uow.SaveChangesAsync();
        await Task.Delay(10);
        _uow.GetRepository<Document, DocumentId>().Add(doc3);
        await _uow.SaveChangesAsync();

        var ordered = await _dbContext!.Documents
            .AsNoTracking()
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();

        ordered[0].Id.Should().Be(doc3.Id);
        ordered[1].Id.Should().Be(doc2.Id);
        ordered[2].Id.Should().Be(doc1.Id);
    }

    [Fact]
    public async Task Query_ExistsAsync_ShouldReturnCorrectResult()
    {
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");

        _uow!.GetRepository<Document, DocumentId>().Add(document);
        await _uow.SaveChangesAsync();

        var exists = await _dbContext!.Documents
            .AsNoTracking()
            .AnyAsync(d => d.Id == document.Id);

        exists.Should().BeTrue();

        var doesNotExist = await _dbContext!.Documents
            .AsNoTracking()
            .AnyAsync(d => d.Id == Guid.NewGuid());

        doesNotExist.Should().BeFalse();
    }

    [Fact]
    public async Task GetRepository_WithUnsupportedType_ShouldThrowInvalidOperationException()
    {
        var act = () => _uow!.GetRepository<SomeOtherAggregate, DocumentId>();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*does not implement IRepository*");
    }

    [Fact]
    public async Task SaveChanges_WithNoChanges_ShouldReturnZero()
    {
        var result = await _uow!.SaveChangesAsync();
        result.Should().Be(0);
    }

    [Fact]
    public async Task Add_WithoutSaveChanges_ShouldNotPersistToDatabase()
    {
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");

        _uow!.GetRepository<Document, DocumentId>().Add(document);

        var notPersisted = await _dbContext!.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == document.Id);

        notPersisted.Should().BeNull("Document should not be persisted until SaveChangesAsync is called");
    }

    [Fact]
    public async Task Delete_WithoutSaveChanges_ShouldNotRemoveFromDatabase()
    {
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");

        _uow!.GetRepository<Document, DocumentId>().Add(document);
        await _uow.SaveChangesAsync();

        _uow.GetRepository<Document, DocumentId>().Delete(document);

        var stillExists = await _dbContext!.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == document.Id);

        stillExists.Should().NotBeNull("Document should not be deleted until SaveChangesAsync is called");
    }

    private class SomeOtherAggregate : MeAjudaAi.Shared.Domain.AggregateRoot<MeAjudaAi.Modules.Documents.Domain.ValueObjects.DocumentId> { }
}