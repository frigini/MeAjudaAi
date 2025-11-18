using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Catalogs.Domain.Repositories;

/// <summary>
/// Repository contract for Service aggregate.
/// </summary>
public interface IServiceRepository
{
    /// <summary>
    /// Retrieves a service by its ID.
    /// </summary>
    Task<Service?> GetByIdAsync(ServiceId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a service by its name.
    /// </summary>
    Task<Service?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all services.
    /// </summary>
    /// <param name="activeOnly">If true, returns only active services</param>
    /// <param name="cancellationToken"></param>
    Task<IReadOnlyList<Service>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all services in a specific category.
    /// </summary>
    /// <param name="categoryId">ID of the category</param>
    /// <param name="activeOnly">If true, returns only active services</param>
    /// <param name="cancellationToken"></param>
    Task<IReadOnlyList<Service>> GetByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a service with the given name already exists.
    /// </summary>
    Task<bool> ExistsWithNameAsync(string name, ServiceId? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts how many services exist in a category.
    /// </summary>
    Task<int> CountByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new service.
    /// </summary>
    Task AddAsync(Service service, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing service.
    /// </summary>
    Task UpdateAsync(Service service, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a service by its ID (hard delete - use with caution).
    /// </summary>
    Task DeleteAsync(ServiceId id, CancellationToken cancellationToken = default);
}
