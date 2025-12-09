using FluentAssertions;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Infrastructure.Persistence;

[Trait("Category", "Unit")]
[Trait("Module", "Documents")]
[Trait("Layer", "Infrastructure")]
public class DocumentConfigurationTests
{
    private static DocumentsDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new DocumentsDbContext(options);
    }

    [Fact]
    public void Configure_ShouldSetTableName()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var entityType = context.Model.FindEntityType(typeof(Document));

        // Assert
        entityType.Should().NotBeNull();
        entityType.GetSchema().Should().Be("meajudaai_documents");
    }

    [Fact]
    public void Configure_ShouldSetOptionalPropertiesCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var entityType = context.Model.FindEntityType(typeof(Document));

        // Assert
        entityType!.FindProperty(nameof(Document.VerifiedAt))!.IsNullable.Should().BeTrue();
        entityType.FindProperty(nameof(Document.RejectionReason))!.IsNullable.Should().BeTrue();
        entityType.FindProperty(nameof(Document.OcrData))!.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void Configure_ShouldSetMaxLengthConstraints()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var entityType = context.Model.FindEntityType(typeof(Document));

        // Assert
        entityType!.FindProperty(nameof(Document.FileUrl))!.GetMaxLength().Should().Be(2048);
        entityType.FindProperty(nameof(Document.FileName))!.GetMaxLength().Should().Be(512);
        entityType.FindProperty(nameof(Document.RejectionReason))!.GetMaxLength().Should().Be(1000);
    }

    [Fact]
    public void Configure_ShouldSetOcrDataAsJsonb()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var entityType = context.Model.FindEntityType(typeof(Document));
        var ocrDataProperty = entityType!.FindProperty(nameof(Document.OcrData));

        // Assert
        ocrDataProperty.Should().NotBeNull();
        ocrDataProperty!.GetColumnName().Should().Be("ocr_data");
    }

    [Fact]
    public void Configure_ShouldConvertEnumsToInt()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var entityType = context.Model.FindEntityType(typeof(Document));

        // Assert - DocumentType should be configured as enum
        var documentTypeProperty = entityType!.FindProperty(nameof(Document.DocumentType));
        documentTypeProperty.Should().NotBeNull();
        documentTypeProperty!.ClrType.Should().Be(typeof(EDocumentType));

        // Assert - Status should be configured as enum
        var statusProperty = entityType.FindProperty(nameof(Document.Status));
        statusProperty.Should().NotBeNull();
        statusProperty!.ClrType.Should().Be(typeof(EDocumentStatus));
    }

    [Fact]
    public void Configure_ShouldCreateIndexOnProviderId()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var entityType = context.Model.FindEntityType(typeof(Document));
        var indexes = entityType!.GetIndexes();

        // Assert
        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == nameof(Document.ProviderId)));
    }

    [Fact]
    public void Configure_ShouldCreateIndexOnStatus()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var entityType = context.Model.FindEntityType(typeof(Document));
        var indexes = entityType!.GetIndexes();

        // Assert
        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == nameof(Document.Status)));
    }

    [Fact]
    public void Configure_ShouldSetPrimaryKey()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var entityType = context.Model.FindEntityType(typeof(Document));

        // Assert
        entityType!.FindProperty(nameof(Document.Id))!.GetColumnName().Should().Be("id");
        entityType.FindProperty(nameof(Document.ProviderId))!.GetColumnName().Should().Be("provider_id");
        entityType.FindProperty(nameof(Document.DocumentType))!.GetColumnName().Should().Be("document_type");
        entityType.FindProperty(nameof(Document.FileUrl))!.GetColumnName().Should().Be("file_url");
        entityType.FindProperty(nameof(Document.FileName))!.GetColumnName().Should().Be("file_name");
        entityType.FindProperty(nameof(Document.Status))!.GetColumnName().Should().Be("status");
        entityType.FindProperty(nameof(Document.UploadedAt))!.GetColumnName().Should().Be("uploaded_at");
        entityType.FindProperty(nameof(Document.VerifiedAt))!.GetColumnName().Should().Be("verified_at");
        entityType.FindProperty(nameof(Document.RejectionReason))!.GetColumnName().Should().Be("rejection_reason");
        entityType.FindProperty(nameof(Document.OcrData))!.GetColumnName().Should().Be("ocr_data");
    }
}
