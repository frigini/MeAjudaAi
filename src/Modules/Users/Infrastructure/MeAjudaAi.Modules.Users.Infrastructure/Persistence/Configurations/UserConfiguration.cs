using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value, 
                value => new UserId(value))
            .HasColumnName("id")
            .ValueGeneratedNever();

        // Value objects
        builder.Property(u => u.Username)
            .HasConversion(
                username => username.Value,
                value => new Username(value))
            .HasColumnName("username")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value, 
                value => new Email(value))
            .HasColumnName("email")
            .HasMaxLength(254)
            .IsRequired();

        // Primitive value object
        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.KeycloakId)
            .HasColumnName("keycloak_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.Property(u => u.LastUsernameChangeAt)
            .HasColumnName("last_username_change_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        // Índices únicos para campos de busca primários
        builder.HasIndex(u => u.Email)
            .HasDatabaseName("ix_users_email")
            .IsUnique();
        builder.HasIndex(u => u.Username)
            .HasDatabaseName("ix_users_username")
            .IsUnique();
        builder.HasIndex(u => u.KeycloakId)
            .HasDatabaseName("ix_users_keycloak_id")
            .IsUnique();

        // Índice para ordenação temporal (usado em GetPagedAsync)
        builder.HasIndex(u => u.CreatedAt)
            .HasDatabaseName("ix_users_created_at");

        // Índice composto para soft delete + ordenação (otimiza queries principais)
        builder.HasIndex(u => new { u.IsDeleted, u.CreatedAt })
            .HasDatabaseName("ix_users_deleted_created")
            .HasFilter("is_deleted = false"); // Partial index para performance

        // Índice composto para busca com filtro de exclusão
        builder.HasIndex(u => new { u.IsDeleted, u.Email })
            .HasDatabaseName("ix_users_deleted_email")
            .HasFilter("is_deleted = false");

        // Índice composto para busca por username com filtro de exclusão
        builder.HasIndex(u => new { u.IsDeleted, u.Username })
            .HasDatabaseName("ix_users_deleted_username")
            .HasFilter("is_deleted = false");

        // Soft Delete Filter
        builder.HasQueryFilter(u => !u.IsDeleted);

        // Ignore Domain Events (they're not persisted)
        builder.Ignore(u => u.DomainEvents);
    }
}