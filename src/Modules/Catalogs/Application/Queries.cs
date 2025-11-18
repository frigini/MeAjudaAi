using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Catalogs.Application.Queries;

// ============================================================================
// SERVICE QUERIES
// ============================================================================

public sealed record GetServiceByIdQuery(Guid Id) 
    : Query<Result<ServiceDto?>>;

public sealed record GetAllServicesQuery(bool ActiveOnly = false) 
    : Query<Result<IReadOnlyList<ServiceListDto>>>;

public sealed record GetServicesByCategoryQuery(Guid CategoryId, bool ActiveOnly = false) 
    : Query<Result<IReadOnlyList<ServiceListDto>>>;

// ============================================================================
// SERVICE CATEGORY QUERIES
// ============================================================================

public sealed record GetServiceCategoryByIdQuery(Guid Id) 
    : Query<Result<ServiceCategoryDto?>>;

public sealed record GetAllServiceCategoriesQuery(bool ActiveOnly = false) 
    : Query<Result<IReadOnlyList<ServiceCategoryDto>>>;

public sealed record GetServiceCategoriesWithCountQuery(bool ActiveOnly = false) 
    : Query<Result<IReadOnlyList<ServiceCategoryWithCountDto>>>;
