namespace MeAjudaAi.Modules.Locations.Application.DTOs;

/// <summary>
/// DTO para resposta de cidade permitida
/// </summary>
public sealed record AllowedCityDto(
    Guid Id,
    string CityName,
    string StateSigla,
    int? IbgeCode,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy);
