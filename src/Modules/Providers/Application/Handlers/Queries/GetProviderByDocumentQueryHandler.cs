using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Queries;

/// <summary>
/// Handler para processar consultas de prestadores de serviços por documento.
/// </summary>
/// <remarks>
/// Implementa a lógica de negócio para buscar prestadores utilizando número de documento,
/// integrando com o repositório de dados e aplicando as regras de mapeamento necessárias.
/// </remarks>
internal sealed class GetProviderByDocumentQueryHandler(
    IProviderRepository providerRepository,
    ILogger<GetProviderByDocumentQueryHandler> logger)
    : IQueryHandler<GetProviderByDocumentQuery, Result<ProviderDto?>>
{
    /// <summary>
    /// Processa uma consulta para buscar um prestador de serviços por documento.
    /// </summary>
    /// <param name="query">Query contendo o documento a ser buscado</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Result contendo o ProviderDto encontrado ou null se não existir</returns>
    public async Task<Result<ProviderDto?>> HandleAsync(
        GetProviderByDocumentQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Searching for provider by document request");

        try
        {
            var normalizedDocument = query.Document?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedDocument))
            {
                logger.LogWarning("Invalid document provided for provider search");
                return Result<ProviderDto?>.Failure(Error.BadRequest("Document cannot be empty"));
            }

            var provider = await providerRepository.GetByDocumentAsync(normalizedDocument, cancellationToken);

            if (provider == null)
            {
                logger.LogInformation("Provider not found for document request");
                return Result<ProviderDto?>.Success(null);
            }

            var providerDto = provider.ToDto();
            logger.LogInformation("Provider found for document request, ID: {ProviderId}", provider.Id.Value);

            return Result<ProviderDto?>.Success(providerDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while searching for provider by document");
            return Result<ProviderDto?>.Failure(Error.Internal("An error occurred while searching for the provider"));
        }
    }
}
