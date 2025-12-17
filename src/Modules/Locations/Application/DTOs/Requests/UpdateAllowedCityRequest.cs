namespace MeAjudaAi.Modules.Locations.Application.DTOs.Requests;

/// <summary>
/// Request DTO para atualização de cidade permitida
/// </summary>
public sealed record UpdateAllowedCityRequest(
    string CityName,
    string StateSigla,
    int? IbgeCode,
    bool IsActive);
