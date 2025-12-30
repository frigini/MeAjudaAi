using MeAjudaAi.Modules.Locations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for AllowedCity entity
/// </summary>
internal sealed class AllowedCityConfiguration : IEntityTypeConfiguration<AllowedCity>
{
    public void Configure(EntityTypeBuilder<AllowedCity> builder)
    {
        // Tabela no schema locations (snake_case aplicado automaticamente via UseSnakeCaseNamingConvention)
        builder.ToTable("allowed_cities", "locations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CityName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.StateSigla)
            .IsRequired()
            .HasMaxLength(2)
            .IsFixedLength();

        builder.Property(x => x.IbgeCode)
            .IsRequired(false);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.UpdatedBy)
            .IsRequired(false)
            .HasMaxLength(256);

        // Índices para performance
        builder.HasIndex(x => new { x.CityName, x.StateSigla })
            .IsUnique()
            .HasDatabaseName("ix_allowed_cities_city_name_state_sigla");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("ix_allowed_cities_is_active");

        builder.HasIndex(x => x.IbgeCode)
            .IsUnique()
            .HasFilter("ibge_code IS NOT NULL")
            .HasDatabaseName("ix_allowed_cities_ibge_code");
    }
}
