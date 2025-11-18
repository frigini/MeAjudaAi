using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Catalogs.Infrastructure.Persistence.Configurations;

internal sealed class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategory>
{
    public void Configure(EntityTypeBuilder<ServiceCategory> builder)
    {
        builder.ToTable("service_categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => ServiceCategoryId.From(value))
            .HasColumnName("id");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("name");

        builder.Property(c => c.Description)
            .HasMaxLength(500)
            .HasColumnName("description");

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(c => c.DisplayOrder)
            .IsRequired()
            .HasColumnName("display_order");

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("ix_service_categories_name");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("ix_service_categories_is_active");

        builder.HasIndex(c => c.DisplayOrder)
            .HasDatabaseName("ix_service_categories_display_order");

        // Ignore navigation properties
        builder.Ignore(c => c.DomainEvents);
    }
}
