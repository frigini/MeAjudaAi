using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for AllowedCity entity
/// </summary>
public sealed class AllowedCityRepository(LocationsDbContext context) : IAllowedCityRepository
{
    public async Task<IReadOnlyList<AllowedCity>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await context.AllowedCities
            .Where(x => x.IsActive)
            .OrderBy(x => x.StateSigla)
            .ThenBy(x => x.CityName)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AllowedCity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.AllowedCities
            .OrderBy(x => x.StateSigla)
            .ThenBy(x => x.CityName)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<AllowedCity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.AllowedCities
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<AllowedCity?> GetByCityAndStateAsync(string cityName, string stateSigla, CancellationToken cancellationToken = default)
    {
        return await context.AllowedCities
            .FirstOrDefaultAsync(x =>
                x.CityName == cityName.Trim() &&
                x.StateSigla == stateSigla.Trim().ToUpperInvariant(),
                cancellationToken);
    }

    public async Task<bool> IsCityAllowedAsync(string cityName, string stateSigla, CancellationToken cancellationToken = default)
    {
        return await context.AllowedCities
            .AnyAsync(x =>
                x.CityName == cityName.Trim() &&
                x.StateSigla == stateSigla.Trim().ToUpperInvariant() &&
                x.IsActive,
                cancellationToken);
    }

    public async Task AddAsync(AllowedCity allowedCity, CancellationToken cancellationToken = default)
    {
        await context.AllowedCities.AddAsync(allowedCity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AllowedCity allowedCity, CancellationToken cancellationToken = default)
    {
        context.AllowedCities.Update(allowedCity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(AllowedCity allowedCity, CancellationToken cancellationToken = default)
    {
        context.AllowedCities.Remove(allowedCity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string cityName, string stateSigla, CancellationToken cancellationToken = default)
    {
        return await context.AllowedCities
            .AnyAsync(x =>
                x.CityName == cityName.Trim() &&
                x.StateSigla == stateSigla.Trim().ToUpperInvariant(),
                cancellationToken);
    }
}
