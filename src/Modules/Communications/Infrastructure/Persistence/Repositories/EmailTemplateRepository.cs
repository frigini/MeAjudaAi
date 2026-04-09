using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Repositories;

internal sealed class EmailTemplateRepository(CommunicationsDbContext context) : IEmailTemplateRepository
{
    public async Task AddAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        await context.EmailTemplates.AddAsync(template, cancellationToken);
    }

    public async Task<EmailTemplate?> GetActiveByKeyAsync(
        string templateKey,
        string language = "pt-BR",
        CancellationToken cancellationToken = default)
    {
        // Tenta buscar primeiro por um override, depois pelo padrão
        var templates = await context.EmailTemplates
            .Where(x => x.TemplateKey == templateKey.ToLowerInvariant() && x.Language == language && x.IsActive)
            .ToListAsync(cancellationToken);

        return templates.OrderByDescending(x => x.OverrideKey != null).FirstOrDefault();
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
}
