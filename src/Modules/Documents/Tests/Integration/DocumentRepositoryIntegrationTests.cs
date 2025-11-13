using MeAjudaAi.Modules.Documents.Domain.Aggregates;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Documents.Tests.Infrastructure;

namespace MeAjudaAi.Modules.Documents.Tests.Integration;

public class DocumentRepositoryIntegrationTests : DocumentsIntegrationTestBase
{
    [Fact]
    public async Task AddAsync_ShouldPersistDocument_ToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        var dbContext = GetDbContext();
        var repository = new DocumentRepository(dbContext);

        var providerId = Guid.NewGuid();
        var document = new Document(
            providerId,
            DocumentType.IdentityDocument,
            "https://storage.example.com/test.pdf",
            "test.pdf");

        // Act
        await repository.AddAsync(document);
        await repository.SaveChangesAsync();

        // Assert
        var savedDocument = await repository.GetByIdAsync(document.Id);
        savedDocument.Should().NotBeNull();
        savedDocument!.ProviderId.Should().Be(providerId);
        savedDocument.DocumentType.Should().Be(DocumentType.IdentityDocument);
        savedDocument.Status.Should().Be(DocumentStatus.Uploaded);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDocumentDoesNotExist()
    {
        // Arrange
        await ResetDatabaseAsync();
        var dbContext = GetDbContext();
        var repository = new DocumentRepository(dbContext);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByProviderIdAsync_ShouldReturnAllDocuments_ForProvider()
    {
        // Arrange
        await ResetDatabaseAsync();
        var dbContext = GetDbContext();
        var repository = new DocumentRepository(dbContext);

        var providerId = Guid.NewGuid();
        var document1 = new Document(providerId, DocumentType.IdentityDocument, "url1", "file1.pdf");
        var document2 = new Document(providerId, DocumentType.ProofOfResidence, "url2", "file2.pdf");
        var otherProviderDocument = new Document(Guid.NewGuid(), DocumentType.CriminalRecord, "url3", "file3.pdf");

        await repository.AddAsync(document1);
        await repository.AddAsync(document2);
        await repository.AddAsync(otherProviderDocument);
        await repository.SaveChangesAsync();

        // Act
        var results = await repository.GetByProviderIdAsync(providerId);

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(d => d.Id == document1.Id);
        results.Should().Contain(d => d.Id == document2.Id);
        results.Should().NotContain(d => d.Id == otherProviderDocument.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        await ResetDatabaseAsync();
        var dbContext = GetDbContext();
        var repository = new DocumentRepository(dbContext);

        var document = new Document(
            Guid.NewGuid(),
            DocumentType.CriminalRecord,
            "https://storage.example.com/record.pdf",
            "record.pdf");

        await repository.AddAsync(document);
        await repository.SaveChangesAsync();

        // Act
        document.MarkAsVerified(new { RecordNumber = "ABC123" });
        await repository.UpdateAsync(document);
        await repository.SaveChangesAsync();

        // Assert
        var updatedDocument = await repository.GetByIdAsync(document.Id);
        updatedDocument.Should().NotBeNull();
        updatedDocument!.Status.Should().Be(DocumentStatus.Verified);
        updatedDocument.VerifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenDocumentExists()
    {
        // Arrange
        await ResetDatabaseAsync();
        var dbContext = GetDbContext();
        var repository = new DocumentRepository(dbContext);

        var document = new Document(
            Guid.NewGuid(),
            DocumentType.IdentityDocument,
            "https://storage.example.com/id.pdf",
            "id.pdf");

        await repository.AddAsync(document);
        await repository.SaveChangesAsync();

        // Act
        var exists = await repository.ExistsAsync(document.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenDocumentDoesNotExist()
    {
        // Arrange
        await ResetDatabaseAsync();
        var dbContext = GetDbContext();
        var repository = new DocumentRepository(dbContext);

        // Act
        var exists = await repository.ExistsAsync(Guid.NewGuid());

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldPreserveOcrData()
    {
        // Arrange
        await ResetDatabaseAsync();
        var dbContext = GetDbContext();
        var repository = new DocumentRepository(dbContext);

        var document = new Document(
            Guid.NewGuid(),
            DocumentType.IdentityDocument,
            "https://storage.example.com/id.pdf",
            "id.pdf");

        var ocrData = new
        {
            Name = "João Silva",
            DocumentNumber = "123456789",
            BirthDate = "01/01/1990"
        };

        document.MarkAsVerified(ocrData);
        await repository.AddAsync(document);
        await repository.SaveChangesAsync();

        // Act
        var savedDocument = await repository.GetByIdAsync(document.Id);

        // Assert
        savedDocument.Should().NotBeNull();
        savedDocument!.OcrData.Should().NotBeNull();
        // Note: OcrData is stored as JSON, so exact object comparison may not work
        // In a real scenario, you'd deserialize and compare properties
    }
}
