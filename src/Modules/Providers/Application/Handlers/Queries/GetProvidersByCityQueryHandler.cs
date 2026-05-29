using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar queries de busca de prestadores de serviços por cidade.
/// </summary>
/// <param name="providerQueries">Serviço de queries para acesso aos dados</param>
/// <param name="logger">Logger estruturado</param>
public sealed class GetProvidersByCityQueryHandler(
    IProviderQueries providerQueries,
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

            if (string.IsNullOrWhiteSpace(query.City)) {
                logger.LogWarning("Query city is null or empty");
                return Result<IReadOnlyList<ProviderDto>>.Failure(Error.BadRequest("City parameter is required"));
            }

            var providers = await providerQueries.GetByCityAsync(query.City, cancellationToken);

            logger.LogInformation("Found {Count} providers in city {City}", providers.Count, query.City);
            return Result<IReadOnlyList<ProviderDto>>.Success(providers.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting providers by city {City}. Message: {Message}", query.City, ex.Message);
            return Result<IReadOnlyList<ProviderDto>>.Failure(Error.Internal(ValidationMessages.Providers.ErrorRetrievingProviders));
        }
    }
}

