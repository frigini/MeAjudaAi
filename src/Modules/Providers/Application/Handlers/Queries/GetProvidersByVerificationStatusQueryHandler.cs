using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Queries;

public sealed class GetProvidersByVerificationStatusQueryHandler(
    IProviderRepository providerRepository,
    ILogger<GetProvidersByVerificationStatusQueryHandler> logger
) : IQueryHandler<GetProvidersByVerificationStatusQuery, Result<IReadOnlyList<ProviderDto>>>
{
    public async Task<Result<IReadOnlyList<ProviderDto>>> HandleAsync(GetProvidersByVerificationStatusQuery query, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting providers by verification status {Status}", query.Status);
            var providers = await providerRepository.GetByVerificationStatusAsync(query.Status, cancellationToken);
            logger.LogInformation("Found {Count} providers with status {Status}", providers.Count, query.Status);
            return Result<IReadOnlyList<ProviderDto>>.Success(providers.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting providers by verification status {Status}", query.Status);
            return Result<IReadOnlyList<ProviderDto>>.Failure("Ocorreu um erro ao recuperar os prestadores");
        }
    }
}

