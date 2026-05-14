using Bogus;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.Documents;

public class DocumentRepositoryIntegrationTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Documents;

    private readonly Faker _faker = new("pt_BR");

    [Fact]
    public async Task AddAsync_WithValidDocument_ShouldPersistToDatabase()
    {
        using var scope = Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext>();

        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");
        var id = document.Id;

        uow.GetRepository<Document, DocumentId>().Add(document);
        await uow.SaveChangesAsync();

        var savedDocument = await dbContext.Documents.FindAsync(id);
        
        savedDocument.Should().NotBeNull();
        savedDocument!.ProviderId.Should().Be(providerId);
        savedDocument.DocumentType.Should().Be(EDocumentType.IdentityDocument);
        savedDocument.FileName.Should().Be("test.pdf");
        savedDocument.Status.Should().Be(EDocumentStatus.Uploaded);
    }
}