using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Communications.Application.Queries;

public record GetAllEmailTemplatesQuery(Guid CorrelationId) : IQuery<Result<IReadOnlyList<EmailTemplate>>>, ICacheableQuery
{
    public string GetCacheKey() => "email-templates:all";
    public TimeSpan GetCacheExpiration() => TimeSpan.FromHours(1);
    public IReadOnlyCollection<string>? GetCacheTags() => 
        [CacheTags.Communications, CacheTags.EmailTemplates];
}
