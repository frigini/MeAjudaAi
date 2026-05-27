using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence;

/// <summary>
/// Implementação parcial de ProvidersDbContext como IRepository para o agregado Provider.
/// </summary>
public partial class ProvidersDbContext : IRepository<Provider, ProviderId>
{
    public async Task<Provider?> TryFindAsync(ProviderId key, CancellationToken ct) =>
        await Providers
            .Include(p => p.Documents)
            .Include(p => p.Qualifications)
            .Include(p => p.Services)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == key && !p.IsDeleted, ct);

    public void Add(Provider aggregate) =>
        Providers.Add(aggregate);

    public void Delete(Provider aggregate) =>
        Providers.Remove(aggregate);
}
