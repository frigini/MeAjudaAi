using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Catalogs.Domain.Repositories;

/// <summary>
/// Repository contract for ServiceCategory aggregate.
/// </summary>
public interface IServiceCategoryRepository
{
    /// <summary>
    /// Retrieves a service category by its ID.
    /// </summary>
    Task<ServiceCategory?> GetByIdAsync(ServiceCategoryId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a service category by its name.
    /// </summary>
    Task<ServiceCategory?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all service categories.
    /// </summary>
    /// <param name="activeOnly">If true, returns only active categories</param>
    Task<IReadOnlyList<ServiceCategory>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a category with the given name already exists.
    /// </summary>
    Task<bool> ExistsWithNameAsync(string name, ServiceCategoryId? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new service category.
    /// </summary>
    Task AddAsync(ServiceCategory category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing service category.
    /// </summary>
    Task UpdateAsync(ServiceCategory category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a service category by its ID (hard delete - use with caution).
    /// </summary>
    Task DeleteAsync(ServiceCategoryId id, CancellationToken cancellationToken = default);
}
