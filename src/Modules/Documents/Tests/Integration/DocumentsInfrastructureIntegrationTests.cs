using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Integration;

[Trait("Category", "Integration")]
public class DocumentsInfrastructureIntegrationTests : IDisposable
{
    private readonly DocumentsDbContext _dbContext;
    private readonly IUnitOfWork _uow;
    private readonly IDocumentQueries _queries;

    public DocumentsInfrastructureIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseInMemoryDatabase($"DocumentsTestDb_{UuidGenerator.NewId()}")
            .Options;

        _dbContext = new DocumentsDbContext(options, null!);
        _uow = _dbContext;
        _queries = new DbContextDocumentQueries(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Add_ShouldPersistToDatabase()
    {
        var document = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "test.pdf", "path.pdf");
        _uow.GetRepository<Document, Guid>().Add(document);
        await _uow.SaveChangesAsync();

        var retrieved = await _queries.GetByIdAsync(document.Id.Value);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(EDocumentStatus.Uploaded);
    }
}
