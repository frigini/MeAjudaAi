namespace MeAjudaAi.Shared.Contracts.Contracts.Modules.Locations.DTOs;

/// <summary>
/// Request DTO para atualização de cidade permitida.
/// Usado pelo Admin Portal para editar cidades existentes.
/// </summary>
public sealed record UpdateAllowedCityRequestDto(
    string City,
    string State,
    string Country,
    double Latitude,
    double Longitude,
    int ServiceRadiusKm,
    bool IsActive);
