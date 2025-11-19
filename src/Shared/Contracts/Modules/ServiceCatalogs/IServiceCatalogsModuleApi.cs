using MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs.DTOs;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs;

/// <summary>
/// Public API contract for the Catalogs module.
/// Provides access to service categories and services catalog for other modules.
/// </summary>
public interface IServiceCatalogsModuleApi : IModuleApi
{
    // ============ Service Categories ============

    /// <summary>
    /// Retrieves a service category by ID.
    /// </summary>
    Task<Result<ModuleServiceCategoryDto?>> GetServiceCategoryByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all service categories.
    /// </summary>
    /// <param name="activeOnly">If true, returns only active categories</param>
    /// <param name="cancellationToken"></param>
    Task<Result<IReadOnlyList<ModuleServiceCategoryDto>>> GetAllServiceCategoriesAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    // ============ Services ============

    /// <summary>
    /// Retrieves a service by ID.
    /// </summary>
    Task<Result<ModuleServiceDto?>> GetServiceByIdAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all services.
    /// </summary>
    /// <param name="activeOnly">If true, returns only active services</param>
    /// <param name="cancellationToken"></param>
    Task<Result<IReadOnlyList<ModuleServiceListDto>>> GetAllServicesAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all services in a specific category.
    /// </summary>
    Task<Result<IReadOnlyList<ModuleServiceDto>>> GetServicesByCategoryAsync(
        Guid categoryId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a service exists and is active.
    /// </summary>
    Task<Result<bool>> IsServiceActiveAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if all provided service IDs exist and are active.
    /// </summary>
    /// <returns>Result containing validation outcome and list of invalid service IDs</returns>
    Task<Result<ModuleServiceValidationResultDto>> ValidateServicesAsync(
        IReadOnlyCollection<Guid> serviceIds,
        CancellationToken cancellationToken = default);
}
