using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Queries;

public sealed class GetProvidersByTypeQueryHandler(
    IProviderRepository providerRepository,
    ILogger<GetProvidersByTypeQueryHandler> logger
) : IQueryHandler<GetProvidersByTypeQuery, Result<IReadOnlyList<ProviderDto>>>
{
    public async Task<Result<IReadOnlyList<ProviderDto>>> HandleAsync(GetProvidersByTypeQuery query, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting providers by type {Type}", query.Type);
            var providers = await providerRepository.GetByTypeAsync(query.Type, cancellationToken);
            logger.LogInformation("Found {Count} providers of type {Type}", providers.Count, query.Type);
            return Result<IReadOnlyList<ProviderDto>>.Success(providers.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting providers by type {Type}", query.Type);
            return Result<IReadOnlyList<ProviderDto>>.Failure(ValidationMessages.Providers.ErrorRetrievingProviders);
        }
    }
}

