using ServiceCategoryEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;

public interface IServiceCategoryQueries
{
    Task<IReadOnlyList<ServiceCategoryEntity>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default);
    Task<ServiceCategoryEntity?> GetByIdAsync(ServiceCategoryId id, CancellationToken cancellationToken = default);
    Task<ServiceCategoryEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNameAsync(string name, ServiceCategoryId? excludeId, CancellationToken cancellationToken = default);
}