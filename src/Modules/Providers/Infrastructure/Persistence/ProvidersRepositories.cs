using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence;

public partial class ProvidersDbContext : IRepository<Provider, Guid>, IRepository<Provider, ProviderId>
{
    async Task<Provider?> IRepository<Provider, Guid>.TryFindAsync(
        Guid key, CancellationToken ct) =>
        await Providers.FirstOrDefaultAsync(x => x.Id == key, ct);

    void IRepository<Provider, Guid>.Add(Provider aggregate) =>
        Providers.Add(aggregate);

    void IRepository<Provider, Guid>.Delete(Provider aggregate) =>
        Providers.Remove(aggregate);

    public async Task<Provider?> TryFindAsync(ProviderId key, CancellationToken cancellationToken) =>
        await Providers
            .Include(p => p.Documents)
            .Include(p => p.Qualifications)
            .Include(p => p.Services)
            .FirstOrDefaultAsync(p => p.Id == key && !p.IsDeleted, cancellationToken);

    void IRepository<Provider, ProviderId>.Add(Provider aggregate) =>
        Providers.Add(aggregate);

    public void Delete(Provider aggregate) =>
        Providers.Remove(aggregate);
}
