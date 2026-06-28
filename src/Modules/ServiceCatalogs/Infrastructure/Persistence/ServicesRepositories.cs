using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;

/// <summary>
/// Implementação IRepository&lt;Service, ServiceId&gt; via partial class do ServiceCatalogsDbContext.
/// Operações de tracking para uso em command handlers.
/// </summary>
public partial class ServiceCatalogsDbContext : IRepository<Service, ServiceId>
{
    async Task<Service?> IRepository<Service, ServiceId>.TryFindAsync(ServiceId key, CancellationToken ct) =>
        await Services
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == key, ct);

    void IRepository<Service, ServiceId>.Add(Service aggregate) =>
        Services.Add(aggregate);

    void IRepository<Service, ServiceId>.Delete(Service aggregate) =>
        Services.Remove(aggregate);
}