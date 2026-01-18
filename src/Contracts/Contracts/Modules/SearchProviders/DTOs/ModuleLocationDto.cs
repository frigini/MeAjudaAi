namespace MeAjudaAi.Contracts.Modules.SearchProviders.DTOs;

/// <summary>
/// DTO de localização geográfica para a API do módulo.
/// </summary>
/// <param name="Latitude">Latitude geográfica.</param>
/// <param name="Longitude">Longitude geográfica.</param>
public sealed record ModuleLocationDto(
    double Latitude,
    double Longitude);

