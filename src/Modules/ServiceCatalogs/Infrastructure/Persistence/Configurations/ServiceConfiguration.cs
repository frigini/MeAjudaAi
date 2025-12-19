using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.Configurations;

internal sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(
                id => id.Value,
                value => ServiceId.From(value))
            .HasColumnName("id");

        builder.Property(s => s.CategoryId)
            .HasConversion(
                id => id.Value,
                value => ServiceCategoryId.From(value))
            .IsRequired()
            .HasColumnName("category_id");

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnName("name");

        builder.Property(s => s.Description)
            .HasMaxLength(1000)
            .HasColumnName("description");

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(s => s.DisplayOrder)
            .IsRequired()
            .HasColumnName("display_order");

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        // Relacionamentos
        builder.HasOne(s => s.Category)
            .WithMany()
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_services_category");

        // Índices
        builder.HasIndex(s => s.Name)
            .IsUnique()
            .HasDatabaseName("ix_services_name");

        builder.HasIndex(s => s.CategoryId)
            .HasDatabaseName("ix_services_category_id");

        builder.HasIndex(s => s.IsActive)
            .HasDatabaseName("ix_services_is_active");

        builder.HasIndex(s => new { s.CategoryId, s.DisplayOrder })
            .HasDatabaseName("ix_services_category_display_order");

        // Ignora propriedades de navegação
        builder.Ignore(s => s.DomainEvents);
    }
}
