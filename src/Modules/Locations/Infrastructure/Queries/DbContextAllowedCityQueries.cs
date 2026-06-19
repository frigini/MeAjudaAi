using MeAjudaAi.Modules.Locations.Application.Queries.Interfaces;
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

    public async Task<IReadOnlyList<AllowedCity>> GetByStateAsync(string state, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(state))
            return [];

        var normalizedState = state.Trim().ToUpperInvariant();

        return await dbContext.AllowedCities
            .Where(x => x.StateSigla == normalizedState)
            .OrderBy(x => x.CityName)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<AllowedCity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.AllowedCities
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
                EF.Functions.ILike(x.CityName, normalizedCity) &&
                x.StateSigla == normalizedState,
                cancellationToken);
    }

    public async Task<bool> IsCityAllowedAsync(string cityName, string stateSigla, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cityName) || string.IsNullOrWhiteSpace(stateSigla))
            return false;

        var normalizedCity = cityName.Trim().ToUpperInvariant();
        var normalizedState = stateSigla.Trim().ToUpperInvariant();

        return await dbContext.AllowedCities
            .AsNoTracking()
            .AnyAsync(x =>
                EF.Functions.ILike(x.CityName, normalizedCity) &&
                x.StateSigla == normalizedState &&
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
            .AsNoTracking()
            .AnyAsync(x =>
                EF.Functions.ILike(x.CityName, normalizedCity) &&
                x.StateSigla == normalizedState,
                cancellationToken);
    }
}