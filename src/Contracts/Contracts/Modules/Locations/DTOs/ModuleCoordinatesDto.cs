namespace MeAjudaAi.Contracts.Modules.Locations.DTOs;

/// <summary>
/// DTO representando coordenadas geográficas para comunicação entre módulos.
/// </summary>
public sealed record ModuleCoordinatesDto(
    double Latitude,
    double Longitude);

