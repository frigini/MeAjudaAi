using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Documents.Application.Queries;

/// <summary>
/// Query para obter todos os documentos de um prestador.
/// Utiliza CorrelationId gerado centralmente via UuidGenerator.
/// </summary>
public record GetProviderDocumentsQuery(Guid ProviderId) : Query<IEnumerable<DocumentDto>>, ICacheableQuery
{
    public string GetCacheKey()
    {
        return $"provider:documents:{ProviderId}";
    }

    public TimeSpan GetCacheExpiration()
    {
        // Cache por 5 minutos para lista de documentos
        return TimeSpan.FromMinutes(5);
    }

    public IReadOnlyCollection<string>? GetCacheTags()
    {
        return ["documents", "provider-documents", $"provider:{ProviderId}"];
    }
}
