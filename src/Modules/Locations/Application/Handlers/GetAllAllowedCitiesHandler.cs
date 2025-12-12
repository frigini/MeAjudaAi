using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using MediatR;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler para buscar todas as cidades permitidas
/// </summary>
internal sealed class GetAllAllowedCitiesHandler(
    IAllowedCityRepository repository) : IRequestHandler<GetAllAllowedCitiesQuery, IReadOnlyList<AllowedCityDto>>
{
    public async Task<IReadOnlyList<AllowedCityDto>> Handle(GetAllAllowedCitiesQuery request, CancellationToken cancellationToken)
    {
        var cities = request.OnlyActive
            ? await repository.GetAllActiveAsync(cancellationToken)
            : await repository.GetAllAsync(cancellationToken);

        return cities.Select(c => new AllowedCityDto(
            c.Id,
            c.CityName,
            c.StateSigla,
            c.IbgeCode,
            c.IsActive,
            c.CreatedAt,
            c.UpdatedAt,
            c.CreatedBy,
            c.UpdatedBy))
            .ToList();
    }
}
