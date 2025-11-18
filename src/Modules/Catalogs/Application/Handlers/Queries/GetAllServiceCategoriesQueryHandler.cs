using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Modules.Catalogs.Application.Queries;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Queries;

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
