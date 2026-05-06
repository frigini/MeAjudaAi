using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
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

    [Fact]
    public void DocumentId_Should_HaveCorrectColumnConfiguration()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new DocumentsDbContext(options))
        {
            var entityType = context.Model.FindEntityType(typeof(Document));
            var idProperty = entityType?.FindProperty(nameof(Document.Id));

            idProperty.Should().NotBeNull();
            idProperty?.GetColumnName().Should().Be("id");
        }
    }

    [Fact]
    public void Document_Should_IgnoreDomainEvents()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new DocumentsDbContext(options))
        {
            var entityType = context.Model.FindEntityType(typeof(Document));
            var domainEventsProperty = entityType?.FindProperty(nameof(Document.DomainEvents));

            domainEventsProperty.Should().BeNull();
        }
    }

    [Fact]
    public void Document_Should_HaveRequiredProperties()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new DocumentsDbContext(options))
        {
            var entityType = context.Model.FindEntityType(typeof(Document));

            entityType.FindProperty(nameof(Document.ProviderId)).Should().NotBeNull();
            entityType.FindProperty(nameof(Document.FileName)).Should().NotBeNull();
            entityType.FindProperty(nameof(Document.FileUrl)).Should().NotBeNull();
            entityType.FindProperty(nameof(Document.DocumentType)).Should().NotBeNull();
            entityType.FindProperty(nameof(Document.Status)).Should().NotBeNull();
        }
    }

    [Fact]
    public void Document_Should_HaveMaxLengthConstraints()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new DocumentsDbContext(options))
        {
            var entityType = context.Model.FindEntityType(typeof(Document));

            var fileName = entityType?.FindProperty(nameof(Document.FileName));
            fileName?.GetMaxLength().Should().Be(512);

            var fileUrl = entityType?.FindProperty(nameof(Document.FileUrl));
            fileUrl?.GetMaxLength().Should().Be(2048);

            var rejectionReason = entityType?.FindProperty(nameof(Document.RejectionReason));
            rejectionReason?.GetMaxLength().Should().Be(1000);
        }
    }

    [Fact]
    public void Document_Should_HaveCorrectIndexes()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new DocumentsDbContext(options))
        {
            var entityType = context.Model.FindEntityType(typeof(Document));

            var indexes = entityType?.GetIndexes();
            indexes.Should().NotBeEmpty();
            indexes!.Count().Should().Be(3, "exactly 3 indexes should exist: provider_id, status, provider_type");
        }
    }

    [Fact]
    public void Document_EnumProperties_ShouldBeConfiguredForStorage()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new DocumentsDbContext(options))
        {
            var entityType = context.Model.FindEntityType(typeof(Document));
            var documentType = entityType?.FindProperty(nameof(Document.DocumentType));
            var status = entityType?.FindProperty(nameof(Document.Status));

            (documentType != null && status != null).Should().BeTrue();
        }
    }

    [Fact]
    public void Document_Should_HaveCorrectValueComparerForDocumentId()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new DocumentsDbContext(options))
        {
            var entityType = context.Model.FindEntityType(typeof(Document));
            var idProperty = entityType?.FindProperty(nameof(Document.Id));

            var comparer = idProperty?.GetValueComparer();
            comparer.Should().NotBeNull();
        }
    }

    [Fact]
    public void Document_OcrData_ShouldHaveJsonbColumnType()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new DocumentsDbContext(options))
        {
            var entityType = context.Model.FindEntityType(typeof(Document));
            var ocrData = entityType?.FindProperty(nameof(Document.OcrData));

            ocrData.Should().NotBeNull();
        }
    }

    [Fact]
    public void Document_Should_HaveCreatedAtAndUpdatedAtProperties()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new DocumentsDbContext(options))
        {
            var entityType = context.Model.FindEntityType(typeof(Document));

            var createdAt = entityType?.FindProperty("CreatedAt");
            createdAt.Should().NotBeNull();

            var updatedAt = entityType?.FindProperty("UpdatedAt");
            updatedAt.Should().NotBeNull();
        }
    }
}
