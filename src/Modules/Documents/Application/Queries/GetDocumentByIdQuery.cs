using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Documents.Application.Queries;

/// <summary>
/// Query para obter um documento por ID.
/// Utiliza CorrelationId gerado centralmente via UuidGenerator.
/// </summary>
[ExcludeFromCodeCoverage]
public record GetDocumentByIdQuery(Guid DocumentId) : Query<DocumentDto?>, ICacheableQuery
{
    public string GetCacheKey()
    {
        return $"document:{DocumentId}";
    }

    public TimeSpan GetCacheExpiration()
    {
        // Cache curto (2 minutos) pois status muda durante verificação
        return TimeSpan.FromMinutes(2);
    }

    public IReadOnlyCollection<string>? GetCacheTags() => 
        [CacheTags.Documents, CacheTags.DocumentTag(DocumentId)];
}
