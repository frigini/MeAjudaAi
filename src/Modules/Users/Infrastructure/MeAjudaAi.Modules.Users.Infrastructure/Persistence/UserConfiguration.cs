using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValuleObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, value => new UserId(value))
            .ValueGeneratedNever();

        // Email value object
        builder.Property(u => u.Email)
            .HasConversion(email => email.Value, value => new Email(value))
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        // UserProfile value object
        builder.OwnsOne(u => u.Profile, profileBuilder =>
        {
            profileBuilder.Property(p => p.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();

            profileBuilder.Property(p => p.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();

            profileBuilder.OwnsOne(p => p.PhoneNumber, phoneBuilder =>
            {
                phoneBuilder.Property(pn => pn.Value)
                    .HasColumnName("PhoneNumber")
                    .HasMaxLength(20)
                    .IsRequired(false);

                phoneBuilder.Property(pn => pn.CountryCode)
                    .HasColumnName("CountryCode")
                    .HasMaxLength(5)
                    .IsRequired(false);
            });
        });

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.KeycloakId)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(u => u.KeycloakId)
            .IsUnique();

        // Roles as JSON
        builder.Property(u => u.Roles)
            .HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
            )
            .HasMaxLength(500);

        // ServiceProvider relationship
        builder.HasOne(u => u.ServiceProvider)
            .WithOne()
            .HasForeignKey<ServiceProvider>(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // Ignore domain events
        builder.Ignore(u => u.DomainEvents);

        builder.ToTable("Users");
    }
}