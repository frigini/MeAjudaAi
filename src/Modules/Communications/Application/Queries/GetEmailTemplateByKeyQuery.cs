using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Communications.Application.Queries;

public record GetEmailTemplateByKeyQuery(string TemplateKey, string Language, Guid CorrelationId) 
    : IQuery<Result<EmailTemplate?>>, ICacheableQuery
{
    public string GetCacheKey() => $"email-template:{TemplateKey}:{Language}";
    public TimeSpan GetCacheExpiration() => TimeSpan.FromHours(1);
    public IReadOnlyCollection<string>? GetCacheTags() => 
        [CacheTags.Communications, CacheTags.EmailTemplates];
}
