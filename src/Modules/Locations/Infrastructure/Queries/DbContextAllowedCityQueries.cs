using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Queries;

public class DbContextAllowedCityQueries(LocationsDbContext dbContext) : IAllowedCityQueries
{
    public async Task<IReadOnlyList<AllowedCity>> GetAllActiveAsync(CancellationToken cancellationToken = default) =>
        await dbContext.AllowedCities
            .Where(x => x.IsActive)
            .OrderBy(x => x.StateSigla)
            .ThenBy(x => x.CityName)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AllowedCity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.AllowedCities
            .OrderBy(x => x.StateSigla)
            .ThenBy(x => x.CityName)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

public async Task<AllowedCity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.AllowedCities
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<AllowedCity?> GetByCityAndStateAsync(string cityName, string stateSigla, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cityName) || string.IsNullOrWhiteSpace(stateSigla))
            return null;

        var normalizedCity = cityName.Trim().ToUpperInvariant();
        var normalizedState = stateSigla.Trim().ToUpperInvariant();

        return await dbContext.AllowedCities
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.CityName.ToUpperInvariant() == normalizedCity &&
                x.StateSigla.ToUpperInvariant() == normalizedState,
                cancellationToken);
    }

    public async Task<bool> IsCityAllowedAsync(string cityName, string stateSigla, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cityName) || string.IsNullOrWhiteSpace(stateSigla))
            return false;

        var normalizedCity = cityName.Trim().ToUpperInvariant();
        var normalizedState = stateSigla.Trim().ToUpperInvariant();

        return await dbContext.AllowedCities
            .AnyAsync(x =>
                x.CityName.ToUpperInvariant() == normalizedCity &&
                x.StateSigla.ToUpperInvariant() == normalizedState &&
                x.IsActive,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(string cityName, string stateSigla, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cityName) || string.IsNullOrWhiteSpace(stateSigla))
            return false;

        var normalizedCity = cityName.Trim().ToUpperInvariant();
        var normalizedState = stateSigla.Trim().ToUpperInvariant();

        return await dbContext.AllowedCities
            .AnyAsync(x =>
                x.CityName.ToUpperInvariant() == normalizedCity &&
                x.StateSigla.ToUpperInvariant() == normalizedState,
                cancellationToken);
    }
}