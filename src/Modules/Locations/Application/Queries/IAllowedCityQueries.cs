using MeAjudaAi.Modules.Locations.Domain.Entities;

namespace MeAjudaAi.Modules.Locations.Application.Queries;

public interface IAllowedCityQueries
{
    Task<IReadOnlyList<AllowedCity>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AllowedCity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AllowedCity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AllowedCity?> GetByCityAndStateAsync(string cityName, string stateSigla, CancellationToken cancellationToken = default);
    Task<bool> IsCityAllowedAsync(string cityName, string stateSigla, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string cityName, string stateSigla, CancellationToken cancellationToken = default);
}