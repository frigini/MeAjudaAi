using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Queries;
using MeAjudaAi.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Documents.Tests.Integration.Modules.Documents;

public class DocumentRepositoryIntegrationTests : IDisposable
{
    private readonly DocumentsDbContext _dbContext;
    private readonly IUnitOfWork _uow;
    private readonly IDocumentQueries _queries;

    public DocumentRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseInMemoryDatabase($"DocumentsIntegrationTestDb_{UuidGenerator.NewId()}")
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
        var providerId = Guid.NewGuid();
        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "test.pdf", "path.pdf");
        _uow.GetRepository<Document, Guid>().Add(document);
        await _uow.SaveChangesAsync();

        var retrieved = await _queries.GetByIdAsync(document.Id.Value);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(EDocumentStatus.Uploaded);
    }
}


