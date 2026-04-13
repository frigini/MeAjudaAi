using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;

/// <summary>
/// Consulta para recuperar todos os serviços de uma categoria específica.
/// </summary>
/// <param name="CategoryId">ID da categoria para filtrar os serviços.</param>
public sealed record GetServicesByCategoryQuery(Guid CategoryId, bool ActiveOnly = false)
    : Query<Result<IReadOnlyList<ServiceListDto>>>;
