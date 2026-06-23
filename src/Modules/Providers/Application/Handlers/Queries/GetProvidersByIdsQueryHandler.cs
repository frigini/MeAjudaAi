using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar queries de busca de prestadores por lista de IDs.
/// </summary>
public sealed class GetProvidersByIdsQueryHandler(
    IProviderQueries providerQueries,
    ILogger<GetProvidersByIdsQueryHandler> logger
) : IQueryHandler<GetProvidersByIdsQuery, Result<IReadOnlyList<ProviderDto>>>
{
    public async Task<Result<IReadOnlyList<ProviderDto>>> HandleAsync(
        GetProvidersByIdsQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Retrieving providers by IDs: {ProviderIds}",
            string.Join(", ", query.ProviderIds));

        var providers = await providerQueries.GetByIdsAsync(query.ProviderIds, cancellationToken);

        var result = providers.ToDto();

        logger.LogDebug("Found {Count} providers for {RequestedCount} IDs",
            result.Count, query.ProviderIds.Count);

        return Result<IReadOnlyList<ProviderDto>>.Success(result);
    }
}