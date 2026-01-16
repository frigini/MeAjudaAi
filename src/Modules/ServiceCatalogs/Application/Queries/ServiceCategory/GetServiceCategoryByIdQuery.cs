using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;

/// <summary>
/// Query to retrieve a service category by its identifier.
/// </summary>
public sealed record GetServiceCategoryByIdQuery(Guid Id)
    : Query<Result<ServiceCategoryDto?>>;
