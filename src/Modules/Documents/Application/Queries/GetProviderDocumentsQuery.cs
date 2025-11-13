using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Documents.Application.Queries;

public record GetProviderDocumentsQuery(Guid ProviderId) : IQuery<IEnumerable<DocumentDto>>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
