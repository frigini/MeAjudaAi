namespace MeAjudaAi.Contracts.Modules.Communications.Queries;

/// <summary>
/// Query para busca paginada de logs de comunicação.
/// </summary>
public sealed record CommunicationLogQuery(
    string? CorrelationId = null,
    string? Channel = null,
    string? Recipient = null,
    bool? IsSuccess = null,
    int PageNumber = 1,
    int PageSize = 20
);
