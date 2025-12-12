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
            .HasDatabaseName("IX_AllowedCities_CityName_State");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_AllowedCities_IsActive");

        builder.HasIndex(x => x.IbgeCode)
            .IsUnique()
            .HasFilter("\"IbgeCode\" IS NOT NULL")
            .HasDatabaseName("IX_AllowedCities_IbgeCode");
    }
}
