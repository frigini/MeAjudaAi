using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;

public interface IServiceCategoryQueries
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
    Task<Domain.Entities.ServiceCategory?> GetByIdAsync(ServiceCategoryId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.ServiceCategory>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNameAsync(string name, ServiceCategoryId? excludeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(Domain.Entities.ServiceCategory Category, int ServiceCount)>> GetAllWithServiceCountAsync(bool activeOnly = false, CancellationToken cancellationToken = default);
}
