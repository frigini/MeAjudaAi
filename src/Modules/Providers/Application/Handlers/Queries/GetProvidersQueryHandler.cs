using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar queries de listagem paginada de prestadores de serviços.
/// </summary>
/// <remarks>
/// Implementa busca paginada com filtros opcionais aplicando regras de negócio:
/// - Exclui prestadores com soft delete (IsDeleted = true)
/// - Aplica filtros de busca por nome (case-insensitive)
/// - Filtra por tipo e status de verificação quando especificados
/// - Ordena resultados por data de criação (mais recentes primeiro)
/// </remarks>
public class GetProvidersQueryHandler(
    IProviderQueries providerQueries,
    ILogger<GetProvidersQueryHandler> logger) : IQueryHandler<GetProvidersQuery, Result<PagedResult<ProviderDto>>>
{

    /// <summary>
    /// Processa a query de listagem de prestadores aplicando filtros e paginação.
    /// </summary>
    /// <param name="query">Query com parâmetros de busca</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado paginado com lista de prestadores</returns>
    public async Task<Result<PagedResult<ProviderDto>>> HandleAsync(
        GetProvidersQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing provider search - Page: {Page}, Size: {PageSize}, Filters: Name='{Name}', Type={Type}, Status={Status}",
            query.Page, query.PageSize, query.Name, query.Type, query.VerificationStatus);

        var providers = await providerQueries.GetPagedAsync(
            page: query.Page,
            pageSize: query.PageSize,
            nameFilter: query.Name,
            typeFilter: query.Type.HasValue ? (EProviderType)query.Type.Value : null,
            verificationStatusFilter: query.VerificationStatus.HasValue ? (EVerificationStatus)query.VerificationStatus.Value : null,
            cancellationToken: cancellationToken);

        var providerDtos = providers.Items.Select(ProviderMapper.ToDto).ToList();

        var result = new PagedResult<ProviderDto>
        {
            Items = providerDtos,
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalItems = providers.TotalItems
        };

        logger.LogInformation(
            "Provider search completed - Total: {Total}, Current page: {Page}/{TotalPages}",
            result.TotalItems, result.PageNumber, result.TotalPages);

        return Result<PagedResult<ProviderDto>>.Success(result);
    }
}
