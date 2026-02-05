namespace MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;

/// <summary>
/// DTO para atualização parcial de cidade permitida.
/// </summary>
public sealed record PatchAllowedCityRequestDto(
    double? ServiceRadiusKm,
    bool? IsActive);
