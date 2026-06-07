using MeAjudaAi.Modules.Providers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Configurations;

internal class ProcessedIntegrationEventConfiguration : IEntityTypeConfiguration<ProcessedIntegrationEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedIntegrationEvent> builder)
    {
        builder.ToTable("processed_integration_events", "providers");
        builder.HasKey(e => e.CorrelationId);
        builder.Property(e => e.CorrelationId).IsRequired().HasMaxLength(255);
        builder.Property(e => e.ProcessedAt).IsRequired();
    }
}
