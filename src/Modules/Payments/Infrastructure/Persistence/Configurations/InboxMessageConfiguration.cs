using MeAjudaAi.Modules.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Persistence.Configurations;

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(m => m.ExternalEventId)
            .HasMaxLength(255)
            .HasColumnName("external_event_id");

        builder.Property(m => m.Type)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("type");

        builder.Property(m => m.Content)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasColumnName("content");

        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(m => m.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(m => m.Error)
            .HasColumnName("error");

        builder.Property(m => m.RetryCount)
            .HasDefaultValue(0)
            .HasColumnName("retry_count");

        builder.Property(m => m.MaxRetries)
            .HasDefaultValue(5)
            .HasColumnName("max_retries");

        builder.Property(m => m.NextAttemptAt)
            .HasColumnName("next_attempt_at");

        builder.HasIndex(m => m.ExternalEventId)
            .IsUnique()
            .HasFilter("external_event_id IS NOT NULL");

        builder.HasIndex(m => new { m.NextAttemptAt, m.CreatedAt })
            .HasFilter("processed_at IS NULL AND retry_count < max_retries")
            .HasDatabaseName("IX_inbox_messages_pending_processing");
    }
}
