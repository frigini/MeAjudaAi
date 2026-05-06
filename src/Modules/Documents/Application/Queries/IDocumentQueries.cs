using MeAjudaAi.Modules.Documents.Domain.Entities;

namespace MeAjudaAi.Modules.Documents.Application.Queries;

public interface IDocumentQueries
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdAndProviderAsync(Guid id, Guid providerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
