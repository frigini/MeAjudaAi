using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler responsável por processar a query de busca de cidade permitida por ID.
/// </summary>
internal sealed class GetAllowedCityByIdHandler(IAllowedCityRepository repository)
    : IQueryHandler<GetAllowedCityByIdQuery, AllowedCityDto?>
{
    public async Task<AllowedCityDto?> HandleAsync(GetAllowedCityByIdQuery query, CancellationToken cancellationToken)
    {
        var city = await repository.GetByIdAsync(query.Id, cancellationToken);

        if (city is null)
        {
            return null;
        }

        return new AllowedCityDto(
            city.Id,
            city.CityName,
            city.StateSigla,
            city.IbgeCode,
            city.IsActive,
            city.CreatedAt,
            city.UpdatedAt,
            city.CreatedBy,
            city.UpdatedBy
        );
    }
}
