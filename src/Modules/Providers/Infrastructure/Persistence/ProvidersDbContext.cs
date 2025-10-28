using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence;

/// <summary>
/// Contexto do Entity Framework para o módulo Providers.
/// </summary>
/// <remarks>
/// Implementa o padrão DbContext do Entity Framework Core para persistência
/// das entidades do domínio de prestadores de serviços.
/// </remarks>
public class ProvidersDbContext : DbContext
{
    /// <summary>
    /// Conjunto de dados para prestadores de serviços.
    /// </summary>
    public DbSet<Provider> Providers { get; set; } = null!;

    /// <summary>
    /// Inicializa uma nova instância do contexto.
    /// </summary>
    /// <param name="options">Opções de configuração do contexto</param>
    public ProvidersDbContext(DbContextOptions<ProvidersDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configura o modelo de dados durante a criação do contexto.
    /// </summary>
    /// <param name="modelBuilder">Construtor do modelo</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("providers");

        // Configuração da entidade Provider
        modelBuilder.Entity<Provider>(entity =>
        {
            entity.HasKey(p => p.Id);
            
            entity.Property(p => p.Id)
                .HasConversion(id => id.Value, value => new ProviderId(value))
                .HasColumnName("id");

            entity.Property(p => p.UserId)
                .HasColumnName("user_id");

            entity.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("name");

            entity.Property(p => p.Type)
                .HasConversion(
                    type => type.ToString(),
                    value => Enum.Parse<EProviderType>(value))
                .HasMaxLength(20)
                .IsRequired()
                .HasColumnName("type");

            entity.Property(p => p.VerificationStatus)
                .HasConversion(
                    status => status.ToString(),
                    value => Enum.Parse<EVerificationStatus>(value))
                .HasMaxLength(20)
                .IsRequired()
                .HasColumnName("verification_status");

            entity.Property(p => p.IsDeleted)
                .IsRequired()
                .HasColumnName("is_deleted");

            entity.Property(p => p.DeletedAt)
                .HasColumnName("deleted_at");

            // Configuração do BusinessProfile como owned entity
            entity.OwnsOne(p => p.BusinessProfile, bp =>
            {
                bp.Property(b => b.LegalName)
                    .HasMaxLength(200)
                    .IsRequired()
                    .HasColumnName("legal_name");

                bp.Property(b => b.FantasyName)
                    .HasMaxLength(200)
                    .HasColumnName("fantasy_name");

                bp.Property(b => b.Description)
                    .HasMaxLength(1000)
                    .HasColumnName("description");

                // Configuração do ContactInfo como owned entity
                bp.OwnsOne(b => b.ContactInfo, ci =>
                {
                    ci.Property(c => c.Email)
                        .HasMaxLength(255)
                        .IsRequired()
                        .HasColumnName("email");

                    ci.Property(c => c.PhoneNumber)
                        .HasMaxLength(20)
                        .HasColumnName("phone_number");

                    ci.Property(c => c.Website)
                        .HasMaxLength(255)
                        .HasColumnName("website");
                });

                // Configuração do Address como owned entity
                bp.OwnsOne(b => b.PrimaryAddress, addr =>
                {
                    addr.Property(a => a.Street)
                        .HasMaxLength(200)
                        .IsRequired()
                        .HasColumnName("street");

                    addr.Property(a => a.Number)
                        .HasMaxLength(20)
                        .IsRequired()
                        .HasColumnName("number");

                    addr.Property(a => a.Complement)
                        .HasMaxLength(100)
                        .HasColumnName("complement");

                    addr.Property(a => a.Neighborhood)
                        .HasMaxLength(100)
                        .IsRequired()
                        .HasColumnName("neighborhood");

                    addr.Property(a => a.City)
                        .HasMaxLength(100)
                        .IsRequired()
                        .HasColumnName("city");

                    addr.Property(a => a.State)
                        .HasMaxLength(50)
                        .IsRequired()
                        .HasColumnName("state");

                    addr.Property(a => a.ZipCode)
                        .HasMaxLength(20)
                        .IsRequired()
                        .HasColumnName("zip_code");

                    addr.Property(a => a.Country)
                        .HasMaxLength(50)
                        .IsRequired()
                        .HasColumnName("country");
                });
            });

            // Configuração dos documentos como owned entities
            entity.OwnsMany(p => p.Documents, doc =>
            {
                doc.Property(d => d.Number)
                    .HasMaxLength(50)
                    .IsRequired()
                    .HasColumnName("number");

                doc.Property(d => d.DocumentType)
                    .HasConversion(
                        type => type.ToString(),
                        value => Enum.Parse<EDocumentType>(value))
                    .HasMaxLength(20)
                    .IsRequired()
                    .HasColumnName("document_type");
            });

            // Configuração das qualificações como owned entities
            entity.OwnsMany(p => p.Qualifications, qual =>
            {
                qual.Property(q => q.Name)
                    .HasMaxLength(200)
                    .IsRequired()
                    .HasColumnName("name");

                qual.Property(q => q.Description)
                    .HasMaxLength(1000)
                    .HasColumnName("description");

                qual.Property(q => q.IssuingOrganization)
                    .HasMaxLength(200)
                    .HasColumnName("issuing_organization");

                qual.Property(q => q.IssueDate)
                    .HasColumnName("issue_date");

                qual.Property(q => q.ExpirationDate)
                    .HasColumnName("expiration_date");

                qual.Property(q => q.DocumentNumber)
                    .HasMaxLength(50)
                    .HasColumnName("document_number");
            });

            // Índices
            entity.HasIndex(p => p.UserId)
                .IsUnique()
                .HasDatabaseName("ix_providers_user_id");

            entity.HasIndex(p => p.Name)
                .HasDatabaseName("ix_providers_name");

            entity.HasIndex(p => p.Type)
                .HasDatabaseName("ix_providers_type");

            entity.HasIndex(p => p.VerificationStatus)
                .HasDatabaseName("ix_providers_verification_status");

            entity.HasIndex(p => p.IsDeleted)
                .HasDatabaseName("ix_providers_is_deleted");
        });

        base.OnModelCreating(modelBuilder);
    }
}
