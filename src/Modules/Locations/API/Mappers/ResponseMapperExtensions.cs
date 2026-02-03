using MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;
using MeAjudaAi.Modules.Locations.Application.DTOs;

namespace MeAjudaAi.Modules.Locations.API.Mappers;

public static class ResponseMapperExtensions
{
    public static ModuleAllowedCityDto ToContract(this AllowedCityDto city)
    {
        return new ModuleAllowedCityDto(
            city.Id,
            city.CityName,
            city.StateSigla,
            "Brasil", // Defaulting to Brasil since it's not in the internal DTO explicitly as a property for listing
            city.Latitude,
            city.Longitude,
            (int)city.ServiceRadiusKm, // Contract uses int, Internal uses double
            city.IsActive,
            city.CreatedAt,
            city.UpdatedAt
        );
    }

    public static IReadOnlyList<ModuleAllowedCityDto> ToContract(this IEnumerable<AllowedCityDto> cities)
    {
        return cities.Select(ToContract).ToList();
    }
}
