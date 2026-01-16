namespace MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;

/// <summary>
/// Request DTO para criação de cidade permitida.
/// Usado pelo Admin Portal para adicionar novas cidades ao sistema.
/// </summary>
public sealed record CreateAllowedCityRequestDto(
    string City,
    string State,
    string Country,
    double Latitude,
    double Longitude,
    int ServiceRadiusKm,
    bool IsActive = true);
