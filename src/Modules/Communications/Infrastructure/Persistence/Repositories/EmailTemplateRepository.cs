using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Repositories;

internal sealed class EmailTemplateRepository(CommunicationsDbContext context) : IEmailTemplateRepository
{
    public async Task AddAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        await context.EmailTemplates.AddAsync(template, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<EmailTemplate?> GetActiveByKeyAsync(
        string templateKey,
        string language = "pt-BR",
        CancellationToken cancellationToken = default)
    {
        var templateKeyLower = templateKey.ToLowerInvariant();

        // 1. Tenta buscar primeiro por um override exato (OverrideKey coincide com o solicitado)
        var overrideTemplate = await context.EmailTemplates
            .FirstOrDefaultAsync(x => x.OverrideKey == templateKeyLower 
                                && x.Language == language 
                                && x.IsActive, cancellationToken);

        if (overrideTemplate != null) return overrideTemplate;

        // 2. Se não houver override, busca pelo template base (TemplateKey coincide e OverrideKey é nulo)
        return await context.EmailTemplates
            .FirstOrDefaultAsync(x => x.TemplateKey == templateKeyLower 
                                && x.OverrideKey == null
                                && x.Language == language 
                                && x.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetAllByKeyAsync(
        string templateKey,
        CancellationToken cancellationToken = default)
    {
        return await context.EmailTemplates
            .Where(x => x.TemplateKey == templateKey.ToLowerInvariant())
            .OrderBy(x => x.Language)
            .ThenByDescending(x => x.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.EmailTemplates
            .OrderBy(x => x.TemplateKey)
            .ThenBy(x => x.Language)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await context.EmailTemplates.FindAsync(new object[] { id }, cancellationToken);
        if (template == null) return;

        if (template.IsSystemTemplate)
        {
            throw new InvalidOperationException($"Cannot delete system template with ID {id}.");
        }

        context.EmailTemplates.Remove(template);
        await context.SaveChangesAsync(cancellationToken);
    }
}
