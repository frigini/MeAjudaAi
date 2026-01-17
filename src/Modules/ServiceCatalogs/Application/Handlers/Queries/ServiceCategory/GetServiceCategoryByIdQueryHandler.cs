using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Mappings;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory;

public sealed class GetServiceCategoryByIdQueryHandler(IServiceCategoryRepository repository)
    : IQueryHandler<GetServiceCategoryByIdQuery, Result<ServiceCategoryDto?>>
{
    public async Task<Result<ServiceCategoryDto?>> HandleAsync(
        GetServiceCategoryByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        if (request.Id == Guid.Empty)
            return Result<ServiceCategoryDto?>.Failure("Service Category ID cannot be empty.");

        var categoryId = ServiceCategoryId.From(request.Id);
        var category = await repository.GetByIdAsync(categoryId, cancellationToken);

        if (category is null)
            return Result<ServiceCategoryDto?>.Success(null);

        var dto = category.ToDto();

        return Result<ServiceCategoryDto?>.Success(dto);
    }
}
