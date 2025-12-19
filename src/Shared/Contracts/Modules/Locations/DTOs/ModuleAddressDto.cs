namespace MeAjudaAi.Shared.Contracts.Modules.Locations.DTOs;

/// <summary>
/// DTO representando um endereço completo para comunicação entre módulos.
/// </summary>
public sealed record ModuleAddressDto(
    string Cep,
    string Street,
    string Neighborhood,
    string City,
    string State,
    string? Complement = null,
    ModuleCoordinatesDto? Coordinates = null);
