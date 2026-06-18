using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler responsável por processar a query de busca de cidades permitidas por estado.
/// </summary>
public sealed class GetAllowedCitiesByStateHandler(IAllowedCityQueries queries)
    : IQueryHandler<GetAllowedCitiesByStateQuery, IReadOnlyList<AllowedCityDto>>
{
    public async Task<IReadOnlyList<AllowedCityDto>> HandleAsync(GetAllowedCitiesByStateQuery query, CancellationToken cancellationToken = default)
    {
        var cities = await queries.GetByStateAsync(query.State, cancellationToken);

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
