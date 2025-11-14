using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Documents.Application.Queries;

/// <summary>
/// Query para obter todos os documentos de um prestador.
/// Utiliza CorrelationId gerado centralmente via UuidGenerator.
/// </summary>
public record GetProviderDocumentsQuery(Guid ProviderId) : Query<IEnumerable<DocumentDto>>;
