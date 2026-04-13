using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;

/// <summary>
/// Query to retrieve all services within a specific category.
/// </summary>
public sealed record GetServicesByCategoryQuery(Guid CategoryId, bool ActiveOnly = false)
    : Query<Result<IReadOnlyList<ServiceListDto>>>;
