using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(s => s.ProviderId)
            .IsRequired()
            .HasColumnName("provider_id");

        builder.Property(s => s.PlanId)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("plan_id");

        builder.Property(s => s.ExternalSubscriptionId)
            .HasMaxLength(255)
            .HasColumnName("external_subscription_id");

        builder.OwnsOne(s => s.Amount, amount =>
        {
            amount.Property(m => m.Amount)
                .HasPrecision(18, 2)
                .IsRequired()
                .HasColumnName("amount");
            
            amount.Property(m => m.Currency)
                .HasMaxLength(3)
                .IsRequired()
                .HasColumnName("currency");
        });

        builder.Property(s => s.Status)
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<ESubscriptionStatus>(value))
            .HasMaxLength(20)
            .IsRequired()
            .HasColumnName("status");

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(s => s.StartedAt)
            .HasColumnName("started_at");

        builder.Property(s => s.ExpiresAt)
            .HasColumnName("expires_at");

        builder.HasIndex(s => s.ProviderId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.ExternalSubscriptionId).IsUnique();
    }
}
