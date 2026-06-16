using MeAjudaAi.Modules.Communications.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Configurations;

internal sealed class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.ToTable("email_templates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Version)
            .IsRequired();

        builder.Property(x => x.TemplateKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.OverrideKey)
            .HasMaxLength(100);

        builder.Property(x => x.Subject)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.HtmlBody)
            .IsRequired();

        builder.Property(x => x.TextBody)
            .IsRequired();

        builder.Property(x => x.Language)
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue("pt-br");

        // Índice único parcial: apenas uma versão ativa por template_key + language + override_key (quando override_key NÃO é NULL)
        builder.HasIndex(x => new { x.TemplateKey, x.Language, x.OverrideKey })
            .IsUnique()
            .HasFilter("is_active = true AND override_key IS NOT NULL")
            .HasDatabaseName("ix_email_templates_active_per_key_language_override");

        // Índice único parcial para NULL override_key: apenas uma versão ativa por template_key + language
        // Necessário porque PostgreSQL trata NULLs como distintos em índices únicos
        builder.HasIndex(x => new { x.TemplateKey, x.Language })
            .IsUnique()
            .HasFilter("is_active = true AND override_key IS NULL")
            .HasDatabaseName("ix_email_templates_active_per_key_language_no_override");

        // Índice para buscar todas as versões de um template específico
        builder.HasIndex(x => new { x.TemplateKey, x.Language, x.Version });
    }
}
