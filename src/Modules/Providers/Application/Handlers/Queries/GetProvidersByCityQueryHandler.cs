using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar queries de busca de prestadores de serviços por cidade.
/// </summary>
/// <param name="providerRepository">Repositório para acesso aos dados</param>
/// <param name="logger">Logger estruturado</param>
internal sealed class GetProvidersByCityQueryHandler(
    IProviderRepository providerRepository,
    ILogger<GetProvidersByCityQueryHandler> logger
) : IQueryHandler<GetProvidersByCityQuery, Result<IReadOnlyList<ProviderDto>>>
{
    /// <summary>
    /// Processa a query de busca por cidade.
    /// </summary>
    public async Task<Result<IReadOnlyList<ProviderDto>>> HandleAsync(GetProvidersByCityQuery query, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting providers by city {City}", query.City);

            var providers = await providerRepository.GetByCityAsync(query.City, cancellationToken);

            logger.LogInformation("Found {Count} providers in city {City}", providers.Count, query.City);
            return Result<IReadOnlyList<ProviderDto>>.Success(providers.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting providers by city {City}", query.City);
            return Result<IReadOnlyList<ProviderDto>>.Failure("An error occurred while retrieving providers");
        }
    }
}