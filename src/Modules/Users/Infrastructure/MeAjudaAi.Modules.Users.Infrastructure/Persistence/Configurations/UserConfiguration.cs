using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value, 
                value => new UserId(value))
            .ValueGeneratedNever();

        // Value objects
        builder.Property(u => u.Username)
            .HasConversion(
                username => username.Value,
                value => new Username(value))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value, 
                value => new Email(value))
            .HasMaxLength(254)
            .IsRequired();

        // Primitive value object
        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.KeycloakId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(u => u.DeletedAt)
            .IsRequired(false);

        builder.Property(u => u.CreatedAt)
            .IsRequired(false);

        builder.Property(u => u.UpdatedAt)
            .IsRequired(false);

        //Indexes
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.KeycloakId).IsUnique();

        // Soft Delete Filter
        builder.HasQueryFilter(u => !u.IsDeleted);

        // Ignore Domain Events (they're not persisted)
        builder.Ignore(u => u.DomainEvents);
    }
}