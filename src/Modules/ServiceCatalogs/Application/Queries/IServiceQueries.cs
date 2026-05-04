using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using ServiceEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;

public interface IServiceQueries
{
    Task<IReadOnlyList<ServiceEntity>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default);
    Task<ServiceEntity?> GetByIdAsync(ServiceId id, CancellationToken cancellationToken = default);
    Task<ServiceEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceEntity>> GetByIdsAsync(IEnumerable<ServiceId> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceEntity>> GetByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly, CancellationToken cancellationToken = default);
    Task<int> CountByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNameAsync(string name, ServiceId? excludeId, ServiceCategoryId? categoryId, CancellationToken cancellationToken = default);
}