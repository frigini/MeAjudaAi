using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Tests.Mocks;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Documents.Tests.Integration;

[Trait("Category", "Integration")]
public class DocumentsInfrastructureIntegrationTests : IDisposable
{
    private readonly DocumentsDbContext _dbContext;
    private readonly IUnitOfWork _uow;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IDocumentIntelligenceService _documentIntelligenceService;

    public DocumentsInfrastructureIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseInMemoryDatabase($"DocumentsTestDb_{UuidGenerator.NewId()}")
            .Options;

        _dbContext = new DocumentsDbContext(options);
        _uow = _dbContext;
        _blobStorageService = new MockBlobStorageService();
        _documentIntelligenceService = new MockDocumentIntelligenceService();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AddDocument_ShouldPersistToDatabase()
    {
        var document = Document.Create(
            UuidGenerator.NewId(),
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");

        _uow.GetRepository<Document, DocumentId>().Add(document);
        await _uow.SaveChangesAsync();

        var count = await _dbContext.Documents.CountAsync();
        count.Should().Be(1);
    }
}
