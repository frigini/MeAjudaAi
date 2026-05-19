using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory;

public sealed class GetServiceCategoriesWithCountQueryHandler(
    IServiceCategoryQueries categoryQueries,
    IServiceQueries serviceQueries)
    : IQueryHandler<GetServiceCategoriesWithCountQuery, Result<IReadOnlyList<ServiceCategoryWithCountDto>>>
{
    public async Task<Result<IReadOnlyList<ServiceCategoryWithCountDto>>> HandleAsync(
        GetServiceCategoriesWithCountQuery request,
        CancellationToken cancellationToken = default)
    {
        var categories = await categoryQueries.GetAllAsync(request.ActiveOnly, cancellationToken);

        if (categories.Count == 0)
            return Result<IReadOnlyList<ServiceCategoryWithCountDto>>.Success(Array.Empty<ServiceCategoryWithCountDto>());

        var categoryIds = categories.Select(c => c.Id).ToList();
        var counts = await serviceQueries.CountByCategoriesAsync(categoryIds, cancellationToken);

        var dtos = categories.Select(category =>
        {
            var (totalCount, activeCount) = counts.GetValueOrDefault(category.Id, (0, 0));
            return new ServiceCategoryWithCountDto(
                category.Id.Value,
                category.Name,
                category.Description,
                category.IsActive,
                category.DisplayOrder,
                activeCount,
                totalCount
            );
        }).ToList();

        return Result<IReadOnlyList<ServiceCategoryWithCountDto>>.Success(dtos);
    }
}
