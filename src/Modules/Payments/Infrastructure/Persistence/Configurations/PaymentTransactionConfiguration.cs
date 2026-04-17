using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Persistence.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(t => t.SubscriptionId)
            .IsRequired()
            .HasColumnName("subscription_id");

        builder.Property(t => t.ExternalTransactionId)
            .HasMaxLength(255)
            .HasColumnName("external_transaction_id");

        builder.OwnsOne(t => t.Amount, amount =>
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

        builder.Property(t => t.Status)
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<EPaymentStatus>(value))
            .HasMaxLength(20)
            .IsRequired()
            .HasColumnName("status");

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(t => t.ProcessedAt)
            .HasColumnName("processed_at");

        builder.HasIndex(t => t.SubscriptionId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.ExternalTransactionId)
            .IsUnique()
            .HasFilter("external_transaction_id IS NOT NULL");
    }
}
