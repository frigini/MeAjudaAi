using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;

/// <summary>
/// Implementação IRepository&lt;ServiceCategory, ServiceCategoryId&gt; via partial class do ServiceCatalogsDbContext.
/// Operações de tracking para uso em command handlers.
/// </summary>
public partial class ServiceCatalogsDbContext : IRepository<ServiceCategory, ServiceCategoryId>
{
    async Task<ServiceCategory?> IRepository<ServiceCategory, ServiceCategoryId>.TryFindAsync(ServiceCategoryId key, CancellationToken ct) =>
        await ServiceCategories
            .FirstOrDefaultAsync(c => c.Id == key, ct);

    void IRepository<ServiceCategory, ServiceCategoryId>.Add(ServiceCategory aggregate) =>
        ServiceCategories.Add(aggregate);

    void IRepository<ServiceCategory, ServiceCategoryId>.Delete(ServiceCategory aggregate) =>
        ServiceCategories.Remove(aggregate);
}
