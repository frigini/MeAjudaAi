namespace MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;

/// <summary>
/// DTO para resposta de cidade permitida.
/// Usado pelo frontend para exibir cidades permitidas no sistema.
/// </summary>
public sealed record ModuleAllowedCityDto(
    Guid Id,
    string City,
    string State,
    string Country,
    double Latitude,
    double Longitude,
    int ServiceRadiusKm,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
