using MeAjudaAi.Modules.Communications.Domain.Entities;

namespace MeAjudaAi.Modules.Communications.Application.Queries;

public interface ICommunicationLogQueries
{
    Task<bool> ExistsByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommunicationLog>> GetByRecipientAsync(string recipient, int maxResults = 50, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<CommunicationLog> Items, int TotalCount)> SearchAsync(
        string? correlationId = null, string? channel = null, string? recipient = null,
        bool? isSuccess = null, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}
