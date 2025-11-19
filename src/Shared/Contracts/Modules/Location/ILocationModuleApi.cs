using MeAjudaAi.Shared.Contracts.Modules.Location.DTOs;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Shared.Contracts.Modules.Location;

/// <summary>
/// API pública do módulo Location para consumo por outros módulos.
/// </summary>
public interface ILocationModuleApi
{
    /// <summary>
    /// Busca um endereço a partir de um CEP brasileiro.
    /// Utiliza fallback chain: ViaCEP → BrasilAPI → OpenCEP.
    /// </summary>
    Task<Result<ModuleAddressDto>> GetAddressFromCepAsync(string cep, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém coordenadas geográficas a partir de um endereço.
    /// Utiliza geocoding API (Nominatim).
    /// </summary>
    Task<Result<ModuleCoordinatesDto>> GetCoordinatesFromAddressAsync(string address, CancellationToken cancellationToken = default);
}
