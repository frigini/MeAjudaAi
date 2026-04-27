using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Infrastructure.Persistence;

public class DocumentMappingTests
{
    [Fact]
    public void Document_Should_HaveCorrectMapping()
    {
        // Arrange
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new DocumentsDbContext(options))
        {
            // Act
            var entityType = context.Model.FindEntityType(typeof(Document));

            // Assert
            entityType.Should().NotBeNull();
            if (entityType != null)
            {
                entityType.GetSchema().Should().Be("documents");
                entityType.GetTableName().Should().Be("documents");

                var idProperty = entityType.FindProperty(nameof(Document.Id));
                idProperty.Should().NotBeNull();
                idProperty?.IsPrimaryKey().Should().BeTrue();

                var providerIdProperty = entityType.FindProperty(nameof(Document.ProviderId));
                providerIdProperty.Should().NotBeNull();
                providerIdProperty?.GetColumnName().Should().Be("provider_id");

                var statusProperty = entityType.FindProperty(nameof(Document.Status));
                statusProperty.Should().NotBeNull();
                statusProperty?.GetColumnName().Should().Be("status");
            }
        }
    }
}
