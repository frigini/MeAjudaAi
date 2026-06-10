using MeAjudaAi.Contracts.Modules.Communications.Queries;
using MeAjudaAi.Modules.Communications.Domain.Entities;

namespace MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;

public interface ICommunicationLogQueries
{
    Task<bool> ExistsByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommunicationLog>> GetByRecipientAsync(string recipient, int maxResults = 50, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<CommunicationLog> Items, int TotalCount)> SearchAsync(
        CommunicationLogQuery query, CancellationToken cancellationToken = default);
}
