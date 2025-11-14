using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        // Explicit schema to ensure correct table location even if default schema changes
        builder.ToTable("documents", "documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasConversion(
                id => id.Value,
                value => new DocumentId(value))
            .HasColumnName("id")
            .ValueGeneratedNever()
            .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<DocumentId>(
                (l, r) => l != null && r != null && l.Value == r.Value,
                v => v.Value.GetHashCode(),
                v => new DocumentId(v.Value)));

        builder.Property(d => d.ProviderId)
            .HasColumnName("provider_id")
            .IsRequired();

        builder.Property(d => d.DocumentType)
            .HasColumnName("document_type")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.FileUrl)
            .HasColumnName("file_url")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(d => d.FileName)
            .HasColumnName("file_name")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(d => d.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.UploadedAt)
            .HasColumnName("uploaded_at")
            .IsRequired();

        builder.Property(d => d.VerifiedAt)
            .HasColumnName("verified_at");

        builder.Property(d => d.RejectionReason)
            .HasColumnName("rejection_reason")
            .HasMaxLength(1000);

        builder.Property(d => d.OcrData)
            .HasColumnName("ocr_data")
            .HasColumnType("jsonb");

        // Propriedades herdadas de BaseEntity - usando expressões type-safe
        builder.Property<DateTime>(nameof(Document.CreatedAt))
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property<DateTime?>(nameof(Document.UpdatedAt))
            .HasColumnName("updated_at");

        // Índices
        builder.HasIndex(d => d.ProviderId)
            .HasDatabaseName("ix_documents_provider_id");

        builder.HasIndex(d => d.Status)
            .HasDatabaseName("ix_documents_status");

        builder.HasIndex(d => new { d.ProviderId, d.DocumentType })
            .HasDatabaseName("ix_documents_provider_type");

        // Ignora eventos de domínio (armazenados em memória)
        builder.Ignore(d => d.DomainEvents);
    }
}
