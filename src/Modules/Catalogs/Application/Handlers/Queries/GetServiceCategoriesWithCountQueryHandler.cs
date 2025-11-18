using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Modules.Catalogs.Application.Queries;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Queries;

public sealed class GetServiceCategoriesWithCountQueryHandler(
    IServiceCategoryRepository categoryRepository,
    IServiceRepository serviceRepository)
    : IQueryHandler<GetServiceCategoriesWithCountQuery, Result<IReadOnlyList<ServiceCategoryWithCountDto>>>
{
    public async Task<Result<IReadOnlyList<ServiceCategoryWithCountDto>>> HandleAsync(
        GetServiceCategoriesWithCountQuery request, 
        CancellationToken cancellationToken = default)
    {
        var categories = await categoryRepository.GetAllAsync(request.ActiveOnly, cancellationToken);

        var dtos = new List<ServiceCategoryWithCountDto>();

        foreach (var category in categories)
        {
            var totalCount = await serviceRepository.CountByCategoryAsync(
                category.Id, 
                activeOnly: false, 
                cancellationToken);
            
            var activeCount = await serviceRepository.CountByCategoryAsync(
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
