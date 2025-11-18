using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Modules.Catalogs.Application.Queries;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Queries;

// ============================================================================
// SERVICE QUERY HANDLERS
// ============================================================================

public sealed class GetServiceByIdQueryHandler(IServiceRepository repository)
    : IQueryHandler<GetServiceByIdQuery, Result<ServiceDto?>>
{
    public async Task<Result<ServiceDto?>> HandleAsync(
        GetServiceByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var serviceId = ServiceId.From(request.Id);
        var service = await repository.GetByIdAsync(serviceId, cancellationToken);

        if (service is null)
            return Result<ServiceDto?>.Success(null);

        // Note: Category navigation property should be loaded by repository
        var categoryName = service.Category?.Name ?? "Unknown";

        var dto = new ServiceDto(
            service.Id.Value,
            service.CategoryId.Value,
            categoryName,
            service.Name,
            service.Description,
            service.IsActive,
            service.DisplayOrder,
            service.CreatedAt,
            service.UpdatedAt
        );

        return Result<ServiceDto?>.Success(dto);
    }
}

public sealed class GetAllServicesQueryHandler(IServiceRepository repository)
    : IQueryHandler<GetAllServicesQuery, Result<IReadOnlyList<ServiceListDto>>>
{
    public async Task<Result<IReadOnlyList<ServiceListDto>>> HandleAsync(
        GetAllServicesQuery request,
        CancellationToken cancellationToken = default)
    {
        var services = await repository.GetAllAsync(request.ActiveOnly, cancellationToken);

        var dtos = services.Select(s => new ServiceListDto(
            s.Id.Value,
            s.CategoryId.Value,
            s.Name,
            s.Description,
            s.IsActive
        )).ToList();

        return Result<IReadOnlyList<ServiceListDto>>.Success(dtos);
    }
}

public sealed class GetServicesByCategoryQueryHandler(IServiceRepository repository)
    : IQueryHandler<GetServicesByCategoryQuery, Result<IReadOnlyList<ServiceListDto>>>
{
    public async Task<Result<IReadOnlyList<ServiceListDto>>> HandleAsync(
        GetServicesByCategoryQuery request,
        CancellationToken cancellationToken = default)
    {
        var categoryId = ServiceCategoryId.From(request.CategoryId);
        var services = await repository.GetByCategoryAsync(categoryId, request.ActiveOnly, cancellationToken);

        var dtos = services.Select(s => new ServiceListDto(
            s.Id.Value,
            s.CategoryId.Value,
            s.Name,
            s.Description,
            s.IsActive
        )).ToList();

        return Result<IReadOnlyList<ServiceListDto>>.Success(dtos);
    }
}

// ============================================================================
// SERVICE CATEGORY QUERY HANDLERS
// ============================================================================

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
