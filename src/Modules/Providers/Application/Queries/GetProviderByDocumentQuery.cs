using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar um prestador de serviços por documento.
/// </summary>
/// <param name="Document">Número do documento do prestador</param>
[ExcludeFromCodeCoverage]
public sealed record GetProviderByDocumentQuery(string Document) : IQuery<Result<ProviderDto?>>
{
    /// <summary>
    /// Identificador único para correlacionar logs e rastreamento da query.
    /// </summary>
    public Guid CorrelationId { get; } = Guid.NewGuid();
}
