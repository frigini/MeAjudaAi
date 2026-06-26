using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;

public interface IServiceCategoryQueries
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
    Task<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory?> GetByIdAsync(ServiceCategoryId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNameAsync(string name, ServiceCategoryId? excludeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory Category, int ServiceCount)>> GetAllWithServiceCountAsync(bool activeOnly = false, CancellationToken cancellationToken = default);
}
