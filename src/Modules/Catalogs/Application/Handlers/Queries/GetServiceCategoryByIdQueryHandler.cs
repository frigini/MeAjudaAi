using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Modules.Catalogs.Application.Queries;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Queries;

public sealed class GetServiceCategoryByIdQueryHandler(IServiceCategoryRepository repository)
    : IQueryHandler<GetServiceCategoryByIdQuery, Result<ServiceCategoryDto?>>
{
    public async Task<Result<ServiceCategoryDto?>> HandleAsync(
        GetServiceCategoryByIdQuery request, 
        CancellationToken cancellationToken = default)
    {
        var categoryId = ServiceCategoryId.From(request.Id);
        var category = await repository.GetByIdAsync(categoryId, cancellationToken);

        if (category is null)
            return Result<ServiceCategoryDto?>.Success(null);

        var dto = new ServiceCategoryDto(
            category.Id.Value,
            category.Name,
            category.Description,
            category.IsActive,
            category.DisplayOrder,
            category.CreatedAt,
            category.UpdatedAt
        );

        return Result<ServiceCategoryDto?>.Success(dto);
    }
}
