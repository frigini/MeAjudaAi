using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Contracts;

namespace MeAjudaAi.Modules.Providers.Application.Services.Interfaces;

/// <summary>
/// Interface para consultas específicas de prestadores de serviços.
/// </summary>
/// <remarks>
/// Separada do repositório do domínio para permitir consultas mais complexas
/// e paginadas que são específicas da aplicação.
/// </remarks>
public interface IProviderQueryService
{
    /// <summary>
    /// Busca prestadores de serviços com paginação e filtros opcionais.
    /// </summary>
    /// <param name="page">Número da página (1-based)</param>
    /// <param name="pageSize">Número de itens por página</param>
    /// <param name="nameFilter">Filtro opcional por nome (busca parcial)</param>
    /// <param name="typeFilter">Filtro opcional por tipo</param>
    /// <param name="verificationStatusFilter">Filtro opcional por status de verificação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado paginado com prestadores</returns>
    Task<PagedResult<Provider>> GetProvidersAsync(
        int page = 1,
        int pageSize = 20,
        string? nameFilter = null,
        EProviderType? typeFilter = null,
        EVerificationStatus? verificationStatusFilter = null,
        CancellationToken cancellationToken = default);
}
