using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using MediatR;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler para buscar cidade permitida por ID
/// </summary>
internal sealed class GetAllowedCityByIdHandler(
    IAllowedCityRepository repository) : IRequestHandler<GetAllowedCityByIdQuery, AllowedCityDto?>
{
    public async Task<AllowedCityDto?> Handle(GetAllowedCityByIdQuery request, CancellationToken cancellationToken)
    {
        var city = await repository.GetByIdAsync(request.Id, cancellationToken);

        return city is null
            ? null
            : new AllowedCityDto(
                city.Id,
                city.CityName,
                city.StateSigla,
                city.IbgeCode,
                city.IsActive,
                city.CreatedAt,
                city.UpdatedAt,
                city.CreatedBy,
                city.UpdatedBy);
    }
}
