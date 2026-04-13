using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => new ReviewId(value))
            .HasColumnName("id");

        builder.Property(r => r.ProviderId)
            .IsRequired()
            .HasColumnName("provider_id");

        builder.Property(r => r.CustomerId)
            .IsRequired()
            .HasColumnName("customer_id");

        builder.Property(r => r.Rating)
            .IsRequired()
            .HasColumnName("rating");

        builder.Property(r => r.Comment)
            .HasMaxLength(1000)
            .HasColumnName("comment");

        builder.Property(r => r.Status)
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<EReviewStatus>(value))
            .HasMaxLength(20)
            .IsRequired()
            .HasColumnName("status");

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(500)
            .HasColumnName("rejection_reason");

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");
            
        // Índices para performance e integridade
        builder.HasIndex(r => r.ProviderId);
        builder.HasIndex(r => r.CustomerId);
        builder.HasIndex(r => r.Status);
        
        // Unicidade: Um cliente só pode avaliar um prestador uma vez
        builder.HasIndex(r => new { r.ProviderId, r.CustomerId }).IsUnique();
    }
}
