using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Infrastructure.Persistence;

/// <summary>
/// Unit tests for IDocumentRepository interface contract validation.
/// Note: These tests use mocks to verify interface behavior contracts,
/// not the concrete DocumentRepository implementation.
/// TODO: Convert to integration tests using real DocumentRepository with in-memory/Testcontainers DB
/// or create abstract base test class for contract testing against actual implementations.
/// Current mock-based approach only verifies Moq setup, not real persistence behavior.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Documents")]
[Trait("Layer", "Infrastructure")]
public class DocumentRepositoryTests
{
    private readonly Mock<IDocumentRepository> _mockRepository;

    public DocumentRepositoryTests()
    {
        _mockRepository = new Mock<IDocumentRepository>();
    }

    [Fact]
    public async Task AddAsync_WithValidDocument_ShouldCallRepositoryMethod()
    {
        // Arrange
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "test-file.pdf",
            "https://storage.example.com/test-file.pdf");

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockRepository.Object.AddAsync(document);

        // Assert
        _mockRepository.Verify(x => x.AddAsync(document, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingDocument_ShouldReturnDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.ProofOfResidence,
            "proof.pdf",
            "https://storage.example.com/proof.pdf");

        _mockRepository
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _mockRepository.Object.GetByIdAsync(documentId);

        // Assert
        result.Should().NotBeNull();
        result!.DocumentType.Should().Be(EDocumentType.ProofOfResidence);
        result.FileName.Should().Be("proof.pdf");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentDocument_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _mockRepository.Object.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithExistingProvider_ShouldReturnDocuments()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var doc1 = Document.Create(providerId, EDocumentType.IdentityDocument, "id.pdf", "url1");
        var doc2 = Document.Create(providerId, EDocumentType.CriminalRecord, "cr.pdf", "url2");
        var documents = new List<Document> { doc1, doc2 };

        _mockRepository
            .Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        // Act
        var result = await _mockRepository.Object.GetByProviderIdAsync(providerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.DocumentType == EDocumentType.IdentityDocument);
        result.Should().Contain(d => d.DocumentType == EDocumentType.CriminalRecord);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithNoDocuments_ShouldReturnEmpty()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document>());

        // Act
        var result = await _mockRepository.Object.GetByProviderIdAsync(providerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_WithValidDocument_ShouldCallRepositoryMethod()
    {
        // Arrange
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.Other,
            "updated.pdf",
            "https://storage.example.com/updated.pdf");

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockRepository.Object.UpdateAsync(document);

        // Assert
        _mockRepository.Verify(x => x.UpdateAsync(document, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingDocument_ShouldReturnTrue()
    {
        // Arrange
        var documentId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.ExistsAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mockRepository.Object.ExistsAsync(documentId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentDocument_ShouldReturnFalse()
    {
        // Arrange
        var documentId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.ExistsAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _mockRepository.Object.ExistsAsync(documentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldCallRepositoryMethod()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockRepository.Object.SaveChangesAsync();

        // Assert
        _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_WithDifferentDocumentTypes_ShouldAcceptAll()
    {
        // Arrange & Act & Assert
        var documentTypes = new[]
        {
            EDocumentType.IdentityDocument,
            EDocumentType.ProofOfResidence,
            EDocumentType.CriminalRecord,
            EDocumentType.Other
        };

        foreach (var docType in documentTypes)
        {
            var document = Document.Create(
                Guid.NewGuid(),
                docType,
                $"{docType}.pdf",
                $"https://storage.example.com/{docType}.pdf");

            _mockRepository
                .Setup(x => x.AddAsync(document, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _mockRepository.Object.AddAsync(document);

            _mockRepository.Verify(x => x.AddAsync(document, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Reset();
        }
    }
}
