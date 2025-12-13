using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Contracts.Modules;
using MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.ModuleApi;

/// <summary>
/// Implementação da API pública para o módulo ServiceCatalogs.
/// </summary>
[ModuleApi(ModuleMetadata.Name, ModuleMetadata.Version)]
public sealed class ServiceCatalogsModuleApi(
    IServiceCategoryRepository categoryRepository,
    IServiceRepository serviceRepository,
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
            logger.LogDebug("Checking ServiceCatalogs module availability");

            // Simple database connectivity test
            _ = await categoryRepository.GetAllAsync(activeOnly: true, cancellationToken);

            logger.LogDebug("ServiceCatalogs module is available and healthy");
            return true;
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("ServiceCatalogs module availability check was cancelled");
            throw;
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
        try
        {
            if (categoryId == Guid.Empty)
                return Result<ModuleServiceCategoryDto?>.Failure("Category id must be provided");

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
            if (serviceId == Guid.Empty)
                return Result<ModuleServiceDto?>.Failure("Service id must be provided");

            var id = ServiceId.From(serviceId);
            var service = await serviceRepository.GetByIdAsync(id, cancellationToken);

            if (service is null)
                return Result<ModuleServiceDto?>.Success(null);

            var categoryName = service.Category?.Name ?? ValidationMessages.Catalogs.UnknownCategoryName;

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
            if (categoryId == Guid.Empty)
                return Result<IReadOnlyList<ModuleServiceDto>>.Failure("Category id must be provided");

            var id = ServiceCategoryId.From(categoryId);
            var services = await serviceRepository.GetByCategoryAsync(id, activeOnly, cancellationToken);

            var dtos = services.Select(s => new ModuleServiceDto(
                s.Id.Value,
                s.CategoryId.Value,
                s.Category?.Name ?? ValidationMessages.Catalogs.UnknownCategoryName,
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
            if (serviceId == Guid.Empty)
                return Result<bool>.Failure("Service id must be provided");

            var serviceIdValue = ServiceId.From(serviceId);
            var service = await serviceRepository.GetByIdAsync(serviceIdValue, cancellationToken);

            // Return false for not-found to align with query semantics (vs Failure)
            if (service is null)
                return Result<bool>.Success(false);

            return Result<bool>.Success(service.IsActive);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if service {ServiceId} is active", serviceId);
            return Result<bool>.Failure($"Error checking service status: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates that the specified service IDs exist in the catalog.
    /// </summary>
    /// <param name="serviceIds">Collection of service IDs to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result indicating which services are valid.</returns>
    public async Task<Result<ModuleServiceValidationResultDto>> ValidateServicesAsync(
        IReadOnlyCollection<Guid> serviceIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (serviceIds is null)
                return Result<ModuleServiceValidationResultDto>.Failure("Service IDs collection cannot be null");

            // Short-circuit for empty collection
            if (serviceIds.Count == 0)
            {
                return Result<ModuleServiceValidationResultDto>.Success(
                    new ModuleServiceValidationResultDto(true, Array.Empty<Guid>(), Array.Empty<Guid>()));
            }

            var invalidIds = new List<Guid>();
            var inactiveIds = new List<Guid>();

            // Deduplicate input IDs and separate empty GUIDs
            var distinctIds = serviceIds.Distinct().ToList();
            var validGuids = new List<Guid>();

            foreach (var id in distinctIds)
            {
                if (id == Guid.Empty)
                {
                    invalidIds.Add(id);
                }
                else
                {
                    validGuids.Add(id);
                }
            }

            // Only convert non-empty GUIDs to ServiceId value objects
            if (validGuids.Count > 0)
            {
                var serviceIdValues = validGuids.Select(ServiceId.From).ToList();

                // Batch query to avoid N+1 problem
                var services = await serviceRepository.GetByIdsAsync(serviceIdValues, cancellationToken);
                var serviceLookup = services.ToDictionary(s => s.Id.Value);

                foreach (var serviceId in validGuids)
                {
                    if (!serviceLookup.TryGetValue(serviceId, out var service))
                    {
                        invalidIds.Add(serviceId);
                    }
                    else if (!service.IsActive)
                    {
                        inactiveIds.Add(serviceId);
                    }
                }
            }

            var allValid = invalidIds.Count == 0 && inactiveIds.Count == 0;

            var result = new ModuleServiceValidationResultDto(
                allValid,
                invalidIds.ToArray(),
                inactiveIds.ToArray()
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
