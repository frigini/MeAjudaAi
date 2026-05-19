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

        var dtos = new List<ServiceCategoryWithCountDto>();

        foreach (var category in categories)
        {
            var totalCount = await serviceQueries.CountByCategoryAsync(
                category.Id,
                activeOnly: false,
                cancellationToken);

            var activeCount = await serviceQueries.CountByCategoryAsync(
                category.Id,
                activeOnly: true,
                cancellationToken);

            dtos.Add(new ServiceCategoryWithCountDto(
                category.Id.Value,
                category.Name,
                category.Description,
                category.IsActive,
                category.DisplayOrder,
                activeCount,
                totalCount
            ));
        }

        return Result<IReadOnlyList<ServiceCategoryWithCountDto>>.Success(dtos);
    }
}
