using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Queries;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Testcontainers.PostgreSql;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Integration.Persistence;

[Trait("Category", "Integration")]
[Trait("Module", "Documents")]
[Trait("Layer", "Infrastructure")]
public sealed class DocumentRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private DocumentsDbContext? _dbContext;
    private IUnitOfWork? _uow;
    private IDocumentQueries? _queries;

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
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(warnings => 
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        _dbContext = new DocumentsDbContext(options, null!);
        await _dbContext.Database.MigrateAsync();
        
        _uow = _dbContext;
        _queries = new DbContextDocumentQueries(_dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbContext != null) await _dbContext.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task Add_WithValidDocument_ShouldPersistToDatabase()
    {
        var providerId = Guid.NewGuid();
        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "test.pdf", "path.pdf");

        _uow!.GetRepository<Document, Guid>().Add(document);
        await _uow.SaveChangesAsync();

        var retrieved = await _queries!.GetByIdAsync(document.Id.Value);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(EDocumentStatus.Uploaded);
    }
}
