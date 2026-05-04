using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using ServiceEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory;

public sealed class GetServiceCategoriesWithCountQueryHandler(IServiceCategoryQueries categoryQueries, IServiceQueries serviceQueries)
    : IQueryHandler<GetServiceCategoriesWithCountQuery, Result<IReadOnlyList<ServiceCategoryWithCountDto>>>
{
    public async Task<Result<IReadOnlyList<ServiceCategoryWithCountDto>>> HandleAsync(
        GetServiceCategoriesWithCountQuery request,
        CancellationToken cancellationToken = default)
    {
        var categories = await categoryQueries.GetAllAsync(request.ActiveOnly, cancellationToken);
        var services = await serviceQueries.GetAllAsync(request.ActiveOnly, cancellationToken);
        
        var serviceCountByCategory = services
            .GroupBy(s => s.CategoryId.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var dtos = categories.Select(c => new ServiceCategoryWithCountDto(
            c.Id.Value,
            c.Name,
            c.Description,
            c.IsActive,
            c.DisplayOrder,
            serviceCountByCategory.GetValueOrDefault(c.Id.Value, 0),
            0
        )).ToList();

        return Result<IReadOnlyList<ServiceCategoryWithCountDto>>.Success(dtos);
    }
}