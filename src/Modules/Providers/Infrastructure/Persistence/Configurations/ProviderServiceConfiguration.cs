using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração do EF Core para a entidade ProviderService (many-to-many).
/// </summary>
public class ProviderServiceConfiguration : IEntityTypeConfiguration<ProviderService>
{
    public void Configure(EntityTypeBuilder<ProviderService> builder)
    {
        builder.ToTable("provider_services", "providers");

        // Chave composta: ProviderId + ServiceId
        builder.HasKey(ps => new { ps.ProviderId, ps.ServiceId });

        builder.Property(ps => ps.ProviderId)
            .HasConversion(id => id.Value, value => new ProviderId(value))
            .HasColumnName("provider_id")
            .IsRequired();

        builder.Property(ps => ps.ServiceId)
            .HasColumnName("service_id")
            .IsRequired();

        builder.Property(ps => ps.AddedAt)
            .HasColumnName("added_at")
            .IsRequired();

        // Índice para busca eficiente por ServiceId
        builder.HasIndex(ps => ps.ServiceId)
            .HasDatabaseName("ix_provider_services_service_id");

        // Índice composto para queries comuns
        builder.HasIndex(ps => new { ps.ProviderId, ps.ServiceId })
            .IsUnique()
            .HasDatabaseName("ix_provider_services_provider_service");
    }
}
