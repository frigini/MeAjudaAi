using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using ServiceEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service;
using ServiceCategoryEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.ModuleApi;

[ModuleApi(ModuleMetadata.Name, ModuleMetadata.Version)]
public sealed class ServiceCatalogsModuleApi(
    IUnitOfWork uow,
    ILogger<ServiceCatalogsModuleApi> logger) : IServiceCatalogsModuleApi
{
    private static class ModuleMetadata
    {
        public const string Name = "ServiceCatalogs";
        public const string Version = "1.0";
    }

    public string ModuleName => ModuleMetadata.Name;
    public string ApiVersion => ModuleMetadata.Version;

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryRepo = uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>();
            _ = await categoryRepo.TryFindAsync(ServiceCategoryId.From(Guid.NewGuid()), cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking ServiceCatalogs module availability");
            return false;
        }
    }

    public async Task<Result<ModuleServiceCategoryDto?>> GetServiceCategoryByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        if (categoryId == Guid.Empty)
            return Result<ModuleServiceCategoryDto?>.Failure("Category id must be provided");

        var categoryRepository = uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>();
        var category = await categoryRepository.TryFindAsync(ServiceCategoryId.From(categoryId), cancellationToken);

        if (category is null)
            return Result<ModuleServiceCategoryDto?>.Success(null);

        var dto = new ModuleServiceCategoryDto(
            category.Id.Value,
            category.Name,
            category.Description,
            category.IsActive,
            category.DisplayOrder
        );

        return Result<ModuleServiceCategoryDto?>.Success(dto);
    }

    public async Task<Result<IReadOnlyList<ModuleServiceCategoryDto>>> GetAllServiceCategoriesAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var categoryRepository = uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>();
        
        var dtos = new List<ModuleServiceCategoryDto>();
        
        return Result<IReadOnlyList<ModuleServiceCategoryDto>>.Success(dtos);
    }

    public async Task<Result<ModuleServiceDto?>> GetServiceByIdAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        if (serviceId == Guid.Empty)
            return Result<ModuleServiceDto?>.Failure("Service id must be provided");

        var serviceRepository = uow.GetRepository<ServiceEntity, ServiceId>();
        var service = await serviceRepository.TryFindAsync(ServiceId.From(serviceId), cancellationToken);

        if (service is null)
            return Result<ModuleServiceDto?>.Success(null);

        var categoryRepository = uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>();
        var category = await categoryRepository.TryFindAsync(service.CategoryId, cancellationToken);

        var dto = new ModuleServiceDto(
            service.Id.Value,
            null, // ProviderId not needed here
            service.CategoryId.Value,
            category?.Name ?? string.Empty,
            service.Name,
            service.Description,
            service.IsActive
        );

        return Result<ModuleServiceDto?>.Success(dto);
    }

    public async Task<Result<IReadOnlyList<ModuleServiceListDto>>> GetAllServicesAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var serviceRepository = uow.GetRepository<ServiceEntity, ServiceId>();
        
        var dtos = new List<ModuleServiceListDto>();
        
        return Result<IReadOnlyList<ModuleServiceListDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<ModuleServiceDto>>> GetServicesByCategoryAsync(
        Guid categoryId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var serviceRepository = uow.GetRepository<ServiceEntity, ServiceId>();
        
        var dtos = new List<ModuleServiceDto>();
        
        return Result<IReadOnlyList<ModuleServiceDto>>.Success(dtos);
    }

    public async Task<Result<bool>> IsServiceActiveAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        if (serviceId == Guid.Empty)
            return Result<bool>.Success(false);

        var serviceRepository = uow.GetRepository<ServiceEntity, ServiceId>();
        var service = await serviceRepository.TryFindAsync(ServiceId.From(serviceId), cancellationToken);

        return Result<bool>.Success(service?.IsActive ?? false);
    }

    public async Task<Result<ModuleServiceValidationResultDto>> ValidateServicesAsync(
        IReadOnlyCollection<Guid> serviceIds,
        CancellationToken cancellationToken = default)
    {
        if (serviceIds == null || serviceIds.Count == 0)
            return Result<ModuleServiceValidationResultDto>.Success(
                new ModuleServiceValidationResultDto(
                    true,
                    Array.Empty<Guid>(),
                    Array.Empty<Guid>()));

        var serviceRepository = uow.GetRepository<ServiceEntity, ServiceId>();
        var valid = new List<Guid>();
        var invalid = new List<Guid>();
        var inactive = new List<Guid>();

        foreach (var id in serviceIds)
        {
            var service = await serviceRepository.TryFindAsync(ServiceId.From(id), cancellationToken);
            if (service is null)
                invalid.Add(id);
            else if (!service.IsActive)
                inactive.Add(id);
            else
                valid.Add(id);
        }

return Result<ModuleServiceValidationResultDto>.Success(
                new ModuleServiceValidationResultDto(
                    valid.Count == serviceIds.Count && invalid.Count == 0 && inactive.Count == 0,
                    valid,
                    inactive
                ));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return HealthCheckResult.Healthy("ServiceCatalogs module is healthy.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Health check failed");
            return HealthCheckResult.Unhealthy("ServiceCatalogs module is unhealthy.", ex);
        }
    }
}