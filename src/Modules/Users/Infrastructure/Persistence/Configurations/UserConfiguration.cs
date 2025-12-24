using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração do Entity Framework Core para a entidade User.
/// Define mapeamento de tabela, propriedades, value objects e usa PostgreSQL xmin para controle de concorrência.
/// </summary>
/// <remarks>
/// Utiliza a coluna de sistema xmin do PostgreSQL como token de concorrência otimista,
/// eliminando a necessidade de uma coluna RowVersion adicional.
/// </remarks>
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
            .HasMaxLength(ValidationConstants.UserLimits.UsernameMaxLength)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => new Email(value))
            .HasColumnName("email")
            .HasMaxLength(ValidationConstants.UserLimits.EmailMaxLength)
            .IsRequired();

        // PhoneNumber - owned type nullable mapeado para colunas individuais (phone_number, phone_country_code)
        builder.OwnsOne(u => u.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("phone_number")
                .HasMaxLength(20)
                .IsRequired(false);

            phone.Property(p => p.CountryCode)
                .HasColumnName("phone_country_code")
                .HasMaxLength(5)
                .HasDefaultValue("BR")
                .IsRequired(false);
        });
        
        // Make the PhoneNumber navigation optional - EF won't create instance if phone_number is null
        builder.Navigation(u => u.PhoneNumber)
            .IsRequired(false)
            .AutoInclude(false);

        // Primitive value object
        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(ValidationConstants.UserLimits.FirstNameMaxLength)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(ValidationConstants.UserLimits.LastNameMaxLength)
            .IsRequired();

        builder.Property(u => u.KeycloakId)
            .HasColumnName("keycloak_id")
            .HasMaxLength(ValidationConstants.UserLimits.KeycloakIdMaxLength)
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

        // Concurrency token using PostgreSQL xmin system column
        builder.Property(u => u.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

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
