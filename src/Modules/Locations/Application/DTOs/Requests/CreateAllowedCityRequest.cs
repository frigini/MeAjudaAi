namespace MeAjudaAi.Modules.Locations.Application.DTOs.Requests;

/// <summary>
/// Request DTO para criação de cidade permitida
/// </summary>
public sealed record CreateAllowedCityRequest(
    string CityName,
    string StateSigla,
    int? IbgeCode,
    bool IsActive = true);
