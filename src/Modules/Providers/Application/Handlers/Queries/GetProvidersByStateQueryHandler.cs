using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Queries;

internal sealed class GetProvidersByStateQueryHandler(
    IProviderRepository providerRepository,
    ILogger<GetProvidersByStateQueryHandler> logger
) : IQueryHandler<GetProvidersByStateQuery, Result<IReadOnlyList<ProviderDto>>>
{
    public async Task<Result<IReadOnlyList<ProviderDto>>> HandleAsync(GetProvidersByStateQuery query, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting providers by state {State}", query.State);
            var providers = await providerRepository.GetByStateAsync(query.State, cancellationToken);
            logger.LogInformation("Found {Count} providers in state {State}", providers.Count, query.State);
            return Result<IReadOnlyList<ProviderDto>>.Success(providers.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting providers by state {State}", query.State);
            return Result<IReadOnlyList<ProviderDto>>.Failure($"Error getting providers: {ex.Message}");
        }
    }
}