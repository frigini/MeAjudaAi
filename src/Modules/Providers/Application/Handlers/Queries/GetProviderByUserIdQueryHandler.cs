using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar queries de busca de prestador de serviços por ID do usuário.
/// </summary>
/// <param name="providerRepository">Repositório para acesso aos dados</param>
/// <param name="logger">Logger estruturado</param>
internal sealed class GetProviderByUserIdQueryHandler(
    IProviderRepository providerRepository,
    ILogger<GetProviderByUserIdQueryHandler> logger
) : IQueryHandler<GetProviderByUserIdQuery, Result<ProviderDto?>>
{
    /// <summary>
    /// Processa a query de busca por ID do usuário.
    /// </summary>
    public async Task<Result<ProviderDto?>> HandleAsync(GetProviderByUserIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting provider by user ID {UserId}", query.UserId);

            var provider = await providerRepository.GetByUserIdAsync(query.UserId, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider for user {UserId} not found", query.UserId);
                return Result<ProviderDto?>.Success(null);
            }

            logger.LogInformation("Provider for user {UserId} found successfully", query.UserId);
            return Result<ProviderDto?>.Success(provider.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting provider by user ID {UserId}", query.UserId);
            return Result<ProviderDto?>.Failure("Error getting provider");
        }
    }
}
