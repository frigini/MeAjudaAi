using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;

public sealed record GetServiceCategoryByIdQuery(Guid Id)
    : Query<Result<ServiceCategoryDto?>>;
