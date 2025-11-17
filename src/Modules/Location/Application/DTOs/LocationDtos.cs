namespace MeAjudaAi.Modules.Location.Application.DTOs;

/// <summary>
/// DTO representando um endereço completo.
/// </summary>
public sealed record AddressDto(
    string Cep,
    string Street,
    string Neighborhood,
    string City,
    string State,
    string? Complement = null,
    CoordinatesDto? Coordinates = null);

/// <summary>
/// DTO representando coordenadas geográficas.
/// </summary>
public sealed record CoordinatesDto(
    double Latitude,
    double Longitude);
