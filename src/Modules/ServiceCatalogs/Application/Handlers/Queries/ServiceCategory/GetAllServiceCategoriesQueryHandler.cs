using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory;

public sealed class GetAllServiceCategoriesQueryHandler(IServiceCategoryRepository repository)
    : IQueryHandler<GetAllServiceCategoriesQuery, Result<IReadOnlyList<ServiceCategoryDto>>>
{
    public async Task<Result<IReadOnlyList<ServiceCategoryDto>>> HandleAsync(
        GetAllServiceCategoriesQuery request,
        CancellationToken cancellationToken = default)
    {
        var categories = await repository.GetAllAsync(request.ActiveOnly, cancellationToken);

        var dtos = categories.Select(c => new ServiceCategoryDto(
            c.Id.Value,
            c.Name,
            c.Description,
            c.IsActive,
            c.DisplayOrder,
            c.CreatedAt,
            c.UpdatedAt
        )).ToList();

        return Result<IReadOnlyList<ServiceCategoryDto>>.Success(dtos);
    }
}
