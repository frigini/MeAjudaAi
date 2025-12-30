using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.Documents;

/// <summary>
/// Testes de integração para o DocumentsDbContext.
/// Valida configuração do EF Core, migrations e schema PostgreSQL.
/// </summary>
public class DocumentsDbContextTests : BaseApiTest
{
    [Fact]
    public void DocumentsDbContext_ShouldBeRegisteredInDependencyInjection()
    {
        // Arrange & Act
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<DocumentsDbContext>();

        // Assert
        dbContext.Should().NotBeNull("DocumentsDbContext should be registered in DI container");
    }

    [Fact]
    public void DocumentsDbContext_ShouldUseCorrectSchema()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();

        // Act
        var schema = dbContext.Model.FindEntityType(typeof(Document))
            ?.GetSchema();

        // Assert
        schema.Should().Be("documents", "all Documents entities should use 'documents' schema for PostgreSQL isolation");
    }

    [Fact]
    public async Task DocumentsDbContext_ShouldHaveMigrationsHistoryTableInCorrectSchema()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();

        // Act
        var connection = dbContext.Database.GetDbConnection();
        await using (connection)
        {
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT schemaname 
                FROM pg_tables 
                WHERE tablename = '__EFMigrationsHistory' 
                AND schemaname = 'documents'";

            var result = await command.ExecuteScalarAsync();

            // Assert
            result.Should().NotBeNull("__EFMigrationsHistory should exist in 'documents' schema");
            result.Should().Be("documents");
        }
    }

    [Fact]
    public async Task DocumentsDbContext_ShouldHaveDocumentsTable()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();

        // Act
        var connection = dbContext.Database.GetDbConnection();
        await using (connection)
        {
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*) 
                FROM information_schema.tables 
                WHERE table_schema = 'documents' 
                AND table_name = 'documents'";

            var result = await command.ExecuteScalarAsync();
            var count = Convert.ToInt64(result ?? 0);

            // Assert
            count.Should().Be(1, "documents table should exist in documents schema");
        }
    }

    [Fact]
    public async Task DocumentsDbContext_ShouldUseSnakeCaseNamingConvention()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();

        // Act
        var connection = dbContext.Database.GetDbConnection();
        await using (connection)
        {
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT column_name 
                FROM information_schema.columns 
                WHERE table_schema = 'documents' 
                AND table_name = 'documents' 
                AND column_name = 'provider_id'";

            var columnName = await command.ExecuteScalarAsync();

            // Assert
            columnName.Should().NotBeNull("provider_id column should exist (snake_case, not PascalCase)");
            columnName.Should().Be("provider_id");
        }
    }

    [Fact]
    public async Task Documents_ShouldSupportBasicCrudOperations()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();

        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "test-blob.pdf",
            "https://storage.blob.core.windows.net/documents/test-blob.pdf");

        // Act - Create
        dbContext.Documents.Add(document);
        await dbContext.SaveChangesAsync();

        // Act - Read
        var retrievedDocument = await dbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == document.Id);

        // Assert
        retrievedDocument.Should().NotBeNull();
        retrievedDocument!.ProviderId.Should().Be(document.ProviderId);
        retrievedDocument.FileName.Should().Be("test-blob.pdf");

        // Cleanup
        dbContext.Documents.Remove(retrievedDocument);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task DocumentsDbContext_ShouldTrackChanges()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();

        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.ProofOfResidence,
            "test-change-tracking.pdf",
            "https://storage.blob.core.windows.net/documents/test-change-tracking.pdf");

        dbContext.Documents.Add(document);
        await dbContext.SaveChangesAsync();

        // Act
        document.MarkAsPendingVerification();
        document.MarkAsVerified("{\"test\":\"data\"}");

        await dbContext.SaveChangesAsync();

        // Assert
        var updated = await dbContext.Documents.FirstAsync(d => d.Id == document.Id);
        updated.Status.Should().Be(EDocumentStatus.Verified);

        // Cleanup
        dbContext.Documents.Remove(updated);
        await dbContext.SaveChangesAsync();
    }
}
