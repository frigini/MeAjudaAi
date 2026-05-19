using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;

/// <summary>
/// Interface de consultas somente-leitura para o agregado ServiceCategory.
/// Utiliza AsNoTracking para máximo desempenho em operações de leitura.
/// </summary>
public interface IServiceCategoryQueries
{
    Task<ServiceCategory?> GetByIdAsync(ServiceCategoryId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceCategory>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNameAsync(string name, ServiceCategoryId? excludeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(ServiceCategory Category, int ServiceCount)>> GetAllWithServiceCountAsync(bool activeOnly = false, CancellationToken cancellationToken = default);
}
