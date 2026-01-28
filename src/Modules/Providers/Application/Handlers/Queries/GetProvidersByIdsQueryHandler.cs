using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Queries;

public sealed class GetProvidersByIdsQueryHandler(
    IProviderRepository providerRepository,
    ILogger<GetProvidersByIdsQueryHandler> logger
) : IQueryHandler<GetProvidersByIdsQuery, Result<IReadOnlyList<ProviderDto>>>
{
    public async Task<Result<IReadOnlyList<ProviderDto>>> HandleAsync(
        GetProvidersByIdsQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Retrieving providers by IDs: {ProviderIds}",
                string.Join(", ", query.ProviderIds));

            var providers = await providerRepository.GetByIdsAsync(query.ProviderIds, cancellationToken);

            var result = providers.ToDto();

            logger.LogDebug("Found {Count} providers for {RequestedCount} IDs",
                result.Count, query.ProviderIds.Count);

            return Result<IReadOnlyList<ProviderDto>>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving providers by IDs: {ProviderIds}",
                string.Join(", ", query.ProviderIds));
            return Result<IReadOnlyList<ProviderDto>>.Failure(ValidationMessages.Providers.ErrorRetrievingProviders);
        }
    }
}
