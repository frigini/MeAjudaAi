using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Queries;
using MeAjudaAi.Shared.Database.Abstractions;

namespace MeAjudaAi.Integration.Tests.Modules.Documents.Database;

/// <summary>
/// Testes de integração para persistência de Document com banco de dados real (TestContainers).
/// </summary>
public class DocumentRepositoryIntegrationTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Documents;

    [Fact]
    public async Task Add_ShouldPersistToDatabase()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        var queries = scope.ServiceProvider.GetRequiredService<IDocumentQueries>();
        var repository = context.GetRepository<Document, Guid>();
        var providerId = Guid.NewGuid();
        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "test.pdf", "path.pdf");

        // Act
        repository.Add(document);
        await context.SaveChangesAsync();

        // Assert
        var retrieved = await queries.GetByIdAsync(document.Id.Value);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(EDocumentStatus.Uploaded);
        retrieved.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public async Task Add_MultipleDocuments_ShouldPersistAll()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        var queries = scope.ServiceProvider.GetRequiredService<IDocumentQueries>();
        var repository = context.GetRepository<Document, Guid>();
        var providerId = Guid.NewGuid();
        var doc1 = Document.Create(providerId, EDocumentType.IdentityDocument, "doc1.pdf", "path1.pdf");
        var doc2 = Document.Create(providerId, EDocumentType.ProofOfResidence, "doc2.pdf", "path2.pdf");

        // Act
        repository.Add(doc1);
        repository.Add(doc2);
        await context.SaveChangesAsync();

        // Assert
        var retrieved1 = await queries.GetByIdAsync(doc1.Id.Value);
        var retrieved2 = await queries.GetByIdAsync(doc2.Id.Value);
        retrieved1.Should().NotBeNull();
        retrieved2.Should().NotBeNull();
        retrieved1!.DocumentType.Should().Be(EDocumentType.IdentityDocument);
        retrieved2!.DocumentType.Should().Be(EDocumentType.ProofOfResidence);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithExistingDocuments_ShouldReturnDocuments()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        var queries = scope.ServiceProvider.GetRequiredService<IDocumentQueries>();
        var repository = context.GetRepository<Document, Guid>();
        var providerId = Guid.NewGuid();
        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "doc.pdf", "path.pdf");
        repository.Add(document);
        await context.SaveChangesAsync();

        // Act
        var results = await queries.GetByProviderIdAsync(providerId);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(d => d.Id.Value == document.Id.Value);
    }
}
