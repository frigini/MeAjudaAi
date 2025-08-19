using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Enums;
using MeAjudaAi.Modules.Users.Domain.ValuleObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence.Configurations;

public class ServiceProviderConfiguration : IEntityTypeConfiguration<ServiceProvider>
{
    public void Configure(EntityTypeBuilder<ServiceProvider> builder)
    {
        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Id)
            .HasConversion(id => id.Value, value => new UserId(value))
            .ValueGeneratedNever();

        builder.Property(sp => sp.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .IsRequired();

        builder.Property(sp => sp.CompanyName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(sp => sp.TaxId)
            .HasMaxLength(50);

        builder.Property(sp => sp.Tier)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(sp => sp.SubscriptionStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(sp => sp.SubscriptionId)
            .HasMaxLength(100);

        builder.Property(sp => sp.ServiceCategories)
            .HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
            )
            .HasMaxLength(1000);

        builder.Property(sp => sp.Description)
            .HasMaxLength(2000);

        builder.Property(sp => sp.Rating)
            .HasPrecision(3, 2);

        builder.Property(sp => sp.IsVerified)
            .IsRequired();

        builder.HasIndex(sp => sp.UserId)
            .IsUnique();

        builder.HasIndex(sp => sp.CompanyName);
        builder.HasIndex(sp => sp.Tier);
        builder.HasIndex(sp => sp.IsVerified);

        builder.ToTable("ServiceProviders");
    }
}