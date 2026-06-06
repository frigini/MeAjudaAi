using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;

public partial class SearchProvidersDbContext : IRepository<SearchableProvider, SearchableProviderId>
{
    async Task<SearchableProvider?> IRepository<SearchableProvider, SearchableProviderId>.TryFindAsync(
        SearchableProviderId key, CancellationToken ct) =>
        await SearchableProviders.FirstOrDefaultAsync(x => x.Id == key, ct);

    void IRepository<SearchableProvider, SearchableProviderId>.Add(SearchableProvider aggregate) =>
        SearchableProviders.Add(aggregate);

    void IRepository<SearchableProvider, SearchableProviderId>.Delete(SearchableProvider aggregate) =>
        SearchableProviders.Remove(aggregate);
}


