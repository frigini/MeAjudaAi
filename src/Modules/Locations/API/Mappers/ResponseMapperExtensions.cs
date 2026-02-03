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
            "Brasil", // Padrão Brasil, pois não está explícito no DTO interno para listagem
            city.Latitude,
            city.Longitude,
            MapServiceRadius(city.ServiceRadiusKm),
            city.IsActive,
            city.CreatedAt,
            city.UpdatedAt
        );
    }

    private static int MapServiceRadius(double radius)
    {
        // Validação explícita para evitar truncamento silencioso
        if (Math.Abs(radius - Math.Round(radius)) > 0.01)
        {
            throw new FormatException($"O raio de serviço {radius}km tem precisão decimal e não pode ser convertido seguramente para int no contrato.");
        }
        
        return (int)Math.Round(radius);
    }

    public static IReadOnlyList<ModuleAllowedCityDto> ToContract(this IEnumerable<AllowedCityDto> cities)
    {
        return cities.Select(ToContract).ToList();
    }
}
