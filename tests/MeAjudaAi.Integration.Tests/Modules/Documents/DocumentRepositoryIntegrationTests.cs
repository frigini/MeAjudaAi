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

        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");

        uow.GetRepository<Document, DocumentId>().Add(document);
        await uow.SaveChangesAsync();

        var count = scope.ServiceProvider.GetRequiredService<MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext>()
            .Documents.Count();

        count.Should().Be(1);
    }
}