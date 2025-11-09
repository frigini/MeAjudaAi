using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para obter uma lista paginada de prestadores de serviços com filtros opcionais.
/// </summary>
/// <param name="Page">Número da página (1-based)</param>
/// <param name="PageSize">Número de itens por página</param>
/// <param name="Name">Filtro opcional por nome (busca parcial)</param>
/// <param name="Type">Filtro opcional por tipo de prestador</param>
/// <param name="VerificationStatus">Filtro opcional por status de verificação</param>
public record GetProvidersQuery(
    int Page,
    int PageSize,
    string? Name = null,
    int? Type = null,
    int? VerificationStatus = null
) : Query<Result<PagedResult<ProviderDto>>>;
