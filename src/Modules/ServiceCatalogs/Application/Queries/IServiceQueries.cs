using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;

public interface IServiceQueries
{
    Task<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service?> GetByIdAsync(ServiceId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service>> GetByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service>> GetByIdsAsync(IEnumerable<ServiceId> ids, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNameAsync(string name, ServiceId? excludeId, ServiceCategoryId? categoryId, CancellationToken cancellationToken = default);
    Task<int> CountByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly = false, CancellationToken cancellationToken = default);
}
