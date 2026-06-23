using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar queries de busca de prestadores por estado.
/// </summary>
public sealed class GetProvidersByStateQueryHandler(
    IProviderQueries providerQueries,
    ILogger<GetProvidersByStateQueryHandler> logger
) : IQueryHandler<GetProvidersByStateQuery, Result<IReadOnlyList<ProviderDto>>>
{
    public async Task<Result<IReadOnlyList<ProviderDto>>> HandleAsync(GetProvidersByStateQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.State))
        {
            logger.LogWarning("Invalid state parameter: state cannot be null or empty");
            return Result<IReadOnlyList<ProviderDto>>.Failure(ValidationMessages.Providers.StateParameterRequired);
        }

        logger.LogInformation("Getting providers by state {State}", query.State);
        var providers = await providerQueries.GetByStateAsync(query.State, cancellationToken);
        logger.LogInformation("Found {Count} providers in state {State}", providers.Count, query.State);
        return Result<IReadOnlyList<ProviderDto>>.Success(providers.ToDto());
    }
}