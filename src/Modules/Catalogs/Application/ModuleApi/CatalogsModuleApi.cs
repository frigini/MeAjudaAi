using MeAjudaAi.Modules.Catalogs.Application.Queries;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Contracts.Modules;
using MeAjudaAi.Shared.Contracts.Modules.Catalogs;
using MeAjudaAi.Shared.Contracts.Modules.Catalogs.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Catalogs.Application.ModuleApi;

/// <summary>
/// Implementation of the public API for the Catalogs module.
/// </summary>
[ModuleApi("Catalogs", "1.0")]
public sealed class CatalogsModuleApi(
    IServiceCategoryRepository categoryRepository,
    IServiceRepository serviceRepository,
    IServiceProvider serviceProvider,
    ILogger<CatalogsModuleApi> logger) : ICatalogsModuleApi
{
    public string ModuleName => "Catalogs";
    public string ApiVersion => "1.0";

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Checking Catalogs module availability");
            
            // Simple database connectivity test
            var categories = await categoryRepository.GetAllAsync(activeOnly: true, cancellationToken);
            
            logger.LogDebug("Catalogs module is available and healthy");
            return true;
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Catalogs module availability check was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking Catalogs module availability");
            return false;
        }
    }

    public async Task<Result<ModuleServiceCategoryDto?>> GetServiceCategoryByIdAsync(
        Guid categoryId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = ServiceCategoryId.From(categoryId);
            var category = await categoryRepository.GetByIdAsync(id, cancellationToken);

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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving service category {CategoryId}", categoryId);
            return Result<ModuleServiceCategoryDto?>.Failure($"Error retrieving service category: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<ModuleServiceCategoryDto>>> GetAllServiceCategoriesAsync(
        bool activeOnly = true, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await categoryRepository.GetAllAsync(activeOnly, cancellationToken);

            var dtos = categories.Select(c => new ModuleServiceCategoryDto(
                c.Id.Value,
                c.Name,
                c.Description,
                c.IsActive,
                c.DisplayOrder
            )).ToList();

            return Result<IReadOnlyList<ModuleServiceCategoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving service categories");
            return Result<IReadOnlyList<ModuleServiceCategoryDto>>.Failure($"Error retrieving service categories: {ex.Message}");
        }
    }

    public async Task<Result<ModuleServiceDto?>> GetServiceByIdAsync(
        Guid serviceId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = ServiceId.From(serviceId);
            var service = await serviceRepository.GetByIdAsync(id, cancellationToken);

            if (service is null)
                return Result<ModuleServiceDto?>.Success(null);

            var categoryName = service.Category?.Name ?? "Unknown";

            var dto = new ModuleServiceDto(
                service.Id.Value,
                service.CategoryId.Value,
                categoryName,
                service.Name,
                service.Description,
                service.IsActive
            );

            return Result<ModuleServiceDto?>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving service {ServiceId}", serviceId);
            return Result<ModuleServiceDto?>.Failure($"Error retrieving service: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<ModuleServiceListDto>>> GetAllServicesAsync(
        bool activeOnly = true, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var services = await serviceRepository.GetAllAsync(activeOnly, cancellationToken);

            var dtos = services.Select(s => new ModuleServiceListDto(
                s.Id.Value,
                s.CategoryId.Value,
                s.Name,
                s.IsActive
            )).ToList();

            return Result<IReadOnlyList<ModuleServiceListDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving services");
            return Result<IReadOnlyList<ModuleServiceListDto>>.Failure($"Error retrieving services: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<ModuleServiceDto>>> GetServicesByCategoryAsync(
        Guid categoryId, 
        bool activeOnly = true, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = ServiceCategoryId.From(categoryId);
            var services = await serviceRepository.GetByCategoryAsync(id, activeOnly, cancellationToken);

            var dtos = services.Select(s => new ModuleServiceDto(
                s.Id.Value,
                s.CategoryId.Value,
                s.Category?.Name ?? "Unknown",
                s.Name,
                s.Description,
                s.IsActive
            )).ToList();

            return Result<IReadOnlyList<ModuleServiceDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving services for category {CategoryId}", categoryId);
            return Result<IReadOnlyList<ModuleServiceDto>>.Failure($"Error retrieving services: {ex.Message}");
        }
    }

    public async Task<Result<bool>> IsServiceActiveAsync(
        Guid serviceId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var serviceIdValue = ServiceId.From(serviceId);
            var service = await serviceRepository.GetByIdAsync(serviceIdValue, cancellationToken);

            if (service is null)
                return Result<bool>.Failure($"Service with ID '{serviceId}' not found.");

            return Result<bool>.Success(service.IsActive);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if service {ServiceId} is active", serviceId);
            return Result<bool>.Failure($"Error checking service status: {ex.Message}");
        }
    }

    public async Task<Result<ModuleServiceValidationResultDto>> ValidateServicesAsync(
        Guid[] serviceIds, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invalidIds = new List<Guid>();
            var inactiveIds = new List<Guid>();

            foreach (var serviceId in serviceIds)
            {
                var serviceIdValue = ServiceId.From(serviceId);
                var service = await serviceRepository.GetByIdAsync(serviceIdValue, cancellationToken);

                if (service is null)
                {
                    invalidIds.Add(serviceId);
                }
                else if (!service.IsActive)
                {
                    inactiveIds.Add(serviceId);
                }
            }

            var allValid = invalidIds.Count == 0 && inactiveIds.Count == 0;

            var result = new ModuleServiceValidationResultDto(
                allValid,
                [.. invalidIds],
                [.. inactiveIds]
            );

            return Result<ModuleServiceValidationResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating services");
            return Result<ModuleServiceValidationResultDto>.Failure($"Error validating services: {ex.Message}");
        }
    }
}
