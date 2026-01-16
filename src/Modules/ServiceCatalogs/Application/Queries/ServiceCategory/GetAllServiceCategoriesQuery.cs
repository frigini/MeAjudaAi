using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;

public sealed record GetAllServiceCategoriesQuery(bool ActiveOnly = false)
    : Query<Result<IReadOnlyList<ServiceCategoryDto>>>;
