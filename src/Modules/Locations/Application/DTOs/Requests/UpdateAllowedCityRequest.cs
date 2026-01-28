namespace MeAjudaAi.Modules.Locations.Application.DTOs.Requests;

/// <summary>
/// Request DTO para atualização de cidade permitida
/// </summary>
public sealed record UpdateAllowedCityRequest(
    string City,
    string State,
    int? IbgeCode,
    bool IsActive);
