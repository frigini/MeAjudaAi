using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
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

        builder.HasIndex(m => m.ProcessedAt);
        builder.HasIndex(m => m.CreatedAt);
    }
}
