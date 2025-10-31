using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar queries de busca de prestador de serviços por ID.
/// </summary>
/// <param name="providerRepository">Repositório para acesso aos dados</param>
/// <param name="logger">Logger estruturado</param>
internal sealed class GetProviderByIdQueryHandler(
    IProviderRepository providerRepository,
    ILogger<GetProviderByIdQueryHandler> logger
) : IQueryHandler<GetProviderByIdQuery, Result<ProviderDto?>>
{
    /// <summary>
    /// Processa a query de busca por ID.
    /// </summary>
    public async Task<Result<ProviderDto?>> HandleAsync(GetProviderByIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting provider by ID {ProviderId}", query.ProviderId);

            var providerId = new ProviderId(query.ProviderId);
            var provider = await providerRepository.GetByIdAsync(providerId, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", query.ProviderId);
                return Result<ProviderDto?>.Success(null);
            }

            logger.LogInformation("Provider {ProviderId} found successfully", query.ProviderId);
            return Result<ProviderDto?>.Success(provider.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting provider by ID {ProviderId}", query.ProviderId);
            return Result<ProviderDto?>.Failure($"Error getting provider: {ex.Message}");
        }
    }
}