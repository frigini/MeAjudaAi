using Bogus;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.Documents;

/// <summary>
/// Integration tests for DocumentRepository with real database (TestContainers).
/// Tests actual persistence logic, EF mappings, and database constraints.
/// </summary>
public class DocumentRepositoryIntegrationTests : ApiTestBase
{
    private readonly Faker _faker = new("pt_BR");

    [Fact]
    public async Task AddAsync_WithValidDocument_ShouldPersistToDatabase()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        var document = CreateValidDocument();

        // Act
        await repository.AddAsync(document);
        await repository.SaveChangesAsync();

        // Assert
        var retrieved = await repository.GetByIdAsync(document.Id.Value);
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(document.Id);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithMultipleDocuments_ShouldReturnAllDocumentsForProvider()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        var providerId = Guid.NewGuid();
        var document1 = CreateValidDocument(providerId, EDocumentType.IdentityDocument);
        var document2 = CreateValidDocument(providerId, EDocumentType.ProofOfResidence);
        var document3 = CreateValidDocument(providerId, EDocumentType.CriminalRecord);

        await repository.AddAsync(document1);
        await repository.AddAsync(document2);
        await repository.AddAsync(document3);
        await repository.SaveChangesAsync();

        // Act
        var results = await repository.GetByProviderIdAsync(providerId);

        // Assert
        results.Should().HaveCount(3);
        results.Should().AllSatisfy(d => d.ProviderId.Should().Be(providerId));
    }

    [Fact]
    public async Task UpdateAsync_WithModifiedDocument_ShouldPersistChanges()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        var document = CreateValidDocument();
        await repository.AddAsync(document);
        await repository.SaveChangesAsync();

        // Modify document status
        document.MarkAsPendingVerification();

        // Act
        await repository.UpdateAsync(document);
        await repository.SaveChangesAsync();

        // Assert
        var retrieved = await repository.GetByIdAsync(document.Id.Value);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(EDocumentStatus.PendingVerification);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingDocument_ShouldReturnTrue()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        var document = CreateValidDocument();
        await repository.AddAsync(document);
        await repository.SaveChangesAsync();

        // Act
        var exists = await repository.ExistsAsync(document.Id.Value);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Document_WithDifferentStatuses_ShouldPersistCorrectly()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        var providerId = Guid.NewGuid();
        var uploadedDocument = CreateValidDocument(providerId);
        
        var pendingDocument = CreateValidDocument(providerId);
        pendingDocument.MarkAsPendingVerification();
        
        var verifiedDocument = CreateValidDocument(providerId);
        verifiedDocument.MarkAsPendingVerification();
        verifiedDocument.MarkAsVerified();

        await repository.AddAsync(uploadedDocument);
        await repository.AddAsync(pendingDocument);
        await repository.AddAsync(verifiedDocument);
        await repository.SaveChangesAsync();

        // Act
        var allDocuments = await repository.GetByProviderIdAsync(providerId);

        // Assert
        allDocuments.Should().HaveCount(3);
        allDocuments.Should().ContainSingle(d => d.Status == EDocumentStatus.Uploaded);
        allDocuments.Should().ContainSingle(d => d.Status == EDocumentStatus.PendingVerification);
        allDocuments.Should().ContainSingle(d => d.Status == EDocumentStatus.Verified);
    }

    #region Helper Methods

    private Document CreateValidDocument(Guid? providerId = null, EDocumentType? documentType = null)
    {
        return Document.Create(
            providerId: providerId ?? Guid.NewGuid(),
            documentType: documentType ?? EDocumentType.IdentityDocument,
            fileName: $"{_faker.Random.AlphaNumeric(10)}.pdf",
            fileUrl: $"documents/{Guid.NewGuid()}.pdf");
    }

    #endregion
}
