using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;

public interface IServiceCategoryRepository
{
    Task<ServiceCategory?> GetByIdAsync(ServiceCategoryId id, CancellationToken cancellationToken = default);
    Task<ServiceCategory?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceCategory>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNameAsync(string name, ServiceCategoryId? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(ServiceCategory category, CancellationToken cancellationToken = default);
    Task UpdateAsync(ServiceCategory category, CancellationToken cancellationToken = default);
    Task DeleteAsync(ServiceCategoryId id, CancellationToken cancellationToken = default);
}