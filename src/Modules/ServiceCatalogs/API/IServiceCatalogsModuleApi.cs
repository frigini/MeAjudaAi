using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.API;

/// <summary>
/// Public API for cross-module communication with ServiceCatalogs module.
/// Allows other modules (especially Providers) to validate services and categories.
/// </summary>
public interface IServiceCatalogsModuleApi
{
    /// <summary>
    /// Validates a list of service IDs.
    /// Used by Providers module to ensure offered services exist and are active.
    /// </summary>
    /// <param name="serviceIds">List of service IDs to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with valid and invalid service IDs</returns>
    Task<Result<ServiceValidationResult>> ValidateServicesAsync(IReadOnlyCollection<Guid> serviceIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets service details by ID.
    /// </summary>
    /// <param name="serviceId">Service unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Service details or null if not found</returns>
    Task<Result<ServiceInfoDto?>> GetServiceByIdAsync(Guid serviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active services for a specific category.
    /// </summary>
    /// <param name="categoryId">Category unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active services in the category</returns>
    Task<Result<List<ServiceInfoDto>>> GetServicesByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of service validation operation.
/// </summary>
public record ServiceValidationResult(
    List<Guid> ValidServiceIds,
    List<Guid> InvalidServiceIds,
    List<Guid> InactiveServiceIds,
    bool AllValid)
{
    /// <summary>
    /// Creates a successful validation result (all services valid and active).
    /// </summary>
    public static ServiceValidationResult Success(List<Guid> validIds) =>
        new(validIds, new List<Guid>(), new List<Guid>(), true);

    /// <summary>
    /// Creates a partial validation result (some services invalid or inactive).
    /// </summary>
    public static ServiceValidationResult Partial(List<Guid> validIds, List<Guid> invalidIds, List<Guid> inactiveIds) =>
        new(validIds, invalidIds, inactiveIds, false);

    /// <summary>
    /// Creates a failed validation result (all services invalid).
    /// </summary>
    public static ServiceValidationResult Failed(List<Guid> invalidIds) =>
        new(new List<Guid>(), invalidIds, new List<Guid>(), false);
}

/// <summary>
/// Service information data transfer object for cross-module communication.
/// </summary>
public record ServiceInfoDto(
    Guid Id,
    string Name,
    string Description,
    Guid CategoryId,
    string CategoryName,
    bool IsActive,
    decimal? Price,
    int? DurationMinutes);
