using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;

/// <summary>
/// Interface de consultas somente-leitura para o agregado Service.
/// Utiliza AsNoTracking para máximo desempenho em operações de leitura.
/// </summary>
public interface IServiceQueries
{
    Task<Service?> GetByIdAsync(ServiceId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Service>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Service>> GetByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Service>> GetByIdsAsync(IEnumerable<ServiceId> ids, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNameAsync(string name, ServiceId? excludeId, ServiceCategoryId? categoryId, CancellationToken cancellationToken = default);
    Task<int> CountByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly = false, CancellationToken cancellationToken = default);
}
