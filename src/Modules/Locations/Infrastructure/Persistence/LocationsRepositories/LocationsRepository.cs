using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Persistence;

public partial class LocationsDbContext : IRepository<AllowedCity, Guid>
{
    async Task<AllowedCity?> IRepository<AllowedCity, Guid>.TryFindAsync(Guid key, CancellationToken ct) =>
        await AllowedCities.FirstOrDefaultAsync(x => x.Id == key, ct);

    void IRepository<AllowedCity, Guid>.Add(AllowedCity aggregate) =>
        AllowedCities.Add(aggregate);

    void IRepository<AllowedCity, Guid>.Delete(AllowedCity aggregate) =>
        AllowedCities.Remove(aggregate);
}