using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Documents.Application.Queries;

/// <summary>
/// Query para consultar o status de um documento específico.
/// Utiliza CorrelationId gerado centralmente via UuidGenerator.
/// </summary>
public record GetDocumentStatusQuery(Guid DocumentId) : Query<DocumentDto?>, ICacheableQuery
{
    public string GetCacheKey()
    {
        return $"document:status:{DocumentId}";
    }

    public TimeSpan GetCacheExpiration()
    {
        // Cache curto (2 minutos) pois status muda durante verificação
        return TimeSpan.FromMinutes(2);
    }

    public IReadOnlyCollection<string>? GetCacheTags()
    {
        return ["documents", $"document:{DocumentId}"];
    }
}
