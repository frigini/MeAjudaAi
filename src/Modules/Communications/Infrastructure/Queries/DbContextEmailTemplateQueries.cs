using MeAjudaAi.Modules.Communications.Application.Queries;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Queries;

public class DbContextEmailTemplateQueries(CommunicationsDbContext dbContext) : IEmailTemplateQueries
{
    public async Task<EmailTemplate?> GetActiveByKeyAsync(
        string templateKey, string language = "pt-BR", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(templateKey))
            throw new ArgumentException("Template key cannot be null or whitespace.", nameof(templateKey));

        var templateKeyLower = templateKey.ToLowerInvariant();
        var languageLower = (language ?? "pt-BR").ToLowerInvariant();

        var overrideTemplate = await dbContext.EmailTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OverrideKey == templateKeyLower
                                && x.Language == languageLower
                                && x.IsActive, cancellationToken);
        if (overrideTemplate != null) return overrideTemplate;

        return await dbContext.EmailTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TemplateKey == templateKeyLower
                                && x.OverrideKey == null
                                && x.Language == languageLower
                                && x.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetAllByKeyAsync(
        string templateKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(templateKey))
            throw new ArgumentException("Template key cannot be null or whitespace.", nameof(templateKey));

        return await dbContext.EmailTemplates
            .AsNoTracking()
            .Where(x => x.TemplateKey == templateKey.ToLowerInvariant())
            .OrderBy(x => x.Language)
            .ThenByDescending(x => x.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.EmailTemplates
            .AsNoTracking()
            .OrderBy(x => x.TemplateKey)
            .ThenBy(x => x.Language)
            .ToListAsync(cancellationToken);
    }
}
