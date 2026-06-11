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
            .HasDefaultValue("pt-BR");

        // Índice único: apenas uma versão ativa por combinação de template_key + language + override_key
        // Permite múltiplas versões inativas para histórico/auditoria
        builder.HasIndex(x => new { x.TemplateKey, x.Language, x.OverrideKey })
            .IsUnique()
            .HasFilter("is_active = true");

        // Índice para buscar todas as versões de um template específico
        builder.HasIndex(x => new { x.TemplateKey, x.Language, x.Version });
    }
}
