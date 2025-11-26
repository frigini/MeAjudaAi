using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração do EF Core para a entidade SearchableProvider.
/// </summary>
internal sealed class SearchableProviderConfiguration : IEntityTypeConfiguration<SearchableProvider>
{
    public void Configure(EntityTypeBuilder<SearchableProvider> builder)
    {
        builder.ToTable("searchable_providers", "meajudaai_searchproviders");

        // Primary key
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => SearchableProviderId.From(value))
            .ValueGeneratedNever();

        // Provider ID (reference to Providers module)
        builder.Property(p => p.ProviderId)
            .IsRequired()
            .HasColumnName("provider_id");

        builder.HasIndex(p => p.ProviderId)
            .IsUnique()
            .HasDatabaseName("ix_searchable_providers_provider_id");

        // Basic information
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("name");

        builder.Property(p => p.Description)
            .HasMaxLength(1000)
            .HasColumnName("description");

        builder.Property(p => p.City)
            .HasMaxLength(100)
            .HasColumnName("city");

        builder.Property(p => p.State)
            .HasMaxLength(2)
            .HasColumnName("state");

        // Geolocation using PostGIS Point type
        builder.Property(p => p.Location)
            .HasConversion(
                location => new Point(location.Longitude, location.Latitude) { SRID = 4326 },
                point => new GeoPoint(point.Y, point.X))
            .HasColumnName("location")
            .HasColumnType("geography(Point, 4326)");

        // Create spatial index for efficient geospatial queries
        builder.HasIndex(p => p.Location)
            .HasMethod("gist")
            .HasDatabaseName("ix_searchable_providers_location");

        // Rating information
        builder.Property(p => p.AverageRating)
            .HasPrecision(3, 2)
            .HasColumnName("average_rating");

        builder.Property(p => p.TotalReviews)
            .HasColumnName("total_reviews");

        // Subscription tier
        builder.Property(p => p.SubscriptionTier)
            .HasConversion<int>()
            .HasColumnName("subscription_tier");

        builder.HasIndex(p => p.SubscriptionTier)
            .HasDatabaseName("ix_searchable_providers_subscription_tier");

        // Service IDs array
        builder.Property(p => p.ServiceIds)
            .HasColumnName("service_ids")
            .HasColumnType("uuid[]");

        // GIN index for efficient array queries
        builder.HasIndex(p => p.ServiceIds)
            .HasMethod("gin")
            .HasDatabaseName("ix_searchable_providers_service_ids");

        // Active status
        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("ix_searchable_providers_is_active");

        // Audit fields (inherited from BaseEntity)
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        // Composite index for search queries (active + subscription tier + rating)
        builder.HasIndex(p => new { p.IsActive, p.SubscriptionTier, p.AverageRating })
            .HasDatabaseName("ix_searchable_providers_search_ranking");

        // Ignore domain events (not persisted)
        builder.Ignore(p => p.DomainEvents);
    }
}
