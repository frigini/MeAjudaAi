using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler responsável por processar a query de busca de cidade permitida por ID.
/// </summary>
public sealed class GetAllowedCityByIdHandler(IAllowedCityQueries queries)
    : IQueryHandler<GetAllowedCityByIdQuery, AllowedCityDto?>
{
    public async Task<AllowedCityDto?> HandleAsync(GetAllowedCityByIdQuery query, CancellationToken cancellationToken = default)
    {
        var city = await queries.GetByIdAsync(query.Id, cancellationToken);

        if (city is null)
        {
            return null;
        }

        return new AllowedCityDto(
            city.Id,
            city.CityName,
            city.StateSigla,
            city.IbgeCode,
            city.Latitude,
            city.Longitude,
            city.ServiceRadiusKm,
            city.IsActive,
            city.CreatedAt,
            city.UpdatedAt,
            city.CreatedBy,
            city.UpdatedBy
        );
    }
}
