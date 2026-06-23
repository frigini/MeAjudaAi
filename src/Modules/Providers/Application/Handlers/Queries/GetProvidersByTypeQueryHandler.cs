using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar queries de busca de prestadores por tipo.
/// </summary>
public sealed class GetProvidersByTypeQueryHandler(
    IProviderQueries providerQueries,
    ILogger<GetProvidersByTypeQueryHandler> logger
) : IQueryHandler<GetProvidersByTypeQuery, Result<IReadOnlyList<ProviderDto>>>
{
    public async Task<Result<IReadOnlyList<ProviderDto>>> HandleAsync(GetProvidersByTypeQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting providers by type {Type}", query.Type);
        var providers = await providerQueries.GetByTypeAsync(query.Type, cancellationToken);
        logger.LogInformation("Found {Count} providers of type {Type}", providers.Count, query.Type);
        return Result<IReadOnlyList<ProviderDto>>.Success(providers.ToDto());
    }
}