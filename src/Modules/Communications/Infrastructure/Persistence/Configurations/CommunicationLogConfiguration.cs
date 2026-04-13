using MeAjudaAi.Modules.Communications.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Configurations;

internal sealed class CommunicationLogConfiguration : IEntityTypeConfiguration<CommunicationLog>
{
    public void Configure(EntityTypeBuilder<CommunicationLog> builder)
    {
        builder.ToTable("communication_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Channel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Recipient)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.TemplateKey)
            .HasMaxLength(100);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.CorrelationId).IsUnique();
        builder.HasIndex(x => x.Recipient);
        builder.HasIndex(x => x.CreatedAt);
    }
}
