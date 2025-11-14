using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Documents.Application.Queries;

/// <summary>
/// Query para consultar o status de um documento específico.
/// Utiliza CorrelationId gerado centralmente via UuidGenerator.
/// </summary>
public record GetDocumentStatusQuery(Guid DocumentId) : Query<DocumentDto?>;
