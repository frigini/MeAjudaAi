using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler responsável por processar a query de listagem de cidades permitidas.
/// </summary>
public sealed class GetAllAllowedCitiesHandler(IAllowedCityRepository repository)
    : IQueryHandler<GetAllAllowedCitiesQuery, IReadOnlyList<AllowedCityDto>>
{
    public async Task<IReadOnlyList<AllowedCityDto>> HandleAsync(GetAllAllowedCitiesQuery query, CancellationToken cancellationToken = default)
    {
        var cities = query.OnlyActive
            ? await repository.GetAllActiveAsync(cancellationToken)
            : await repository.GetAllAsync(cancellationToken);

        return cities.Select(c => new AllowedCityDto(
            c.Id,
            c.CityName,
            c.StateSigla,
            c.IbgeCode,
            c.Latitude,
            c.Longitude,
            c.ServiceRadiusKm,
            c.IsActive,
            c.CreatedAt,
            c.UpdatedAt,
            c.CreatedBy,
            c.UpdatedBy
        )).ToList();
    }
}
