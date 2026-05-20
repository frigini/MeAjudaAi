using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(ServiceId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Service>> GetByIdsAsync(IEnumerable<ServiceId> ids, CancellationToken cancellationToken = default);
    Task<Service?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Service>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Service>> GetByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNameAsync(string name, ServiceId? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Service service, CancellationToken cancellationToken = default);
    Task UpdateAsync(Service service, CancellationToken cancellationToken = default);
    Task DeleteAsync(ServiceId id, CancellationToken cancellationToken = default);
}