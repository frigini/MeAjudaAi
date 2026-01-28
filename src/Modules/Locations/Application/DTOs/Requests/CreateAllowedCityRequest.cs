namespace MeAjudaAi.Modules.Locations.Application.DTOs.Requests;

/// <summary>
/// Request DTO para criação de cidade permitida
/// </summary>
public sealed record CreateAllowedCityRequest(
    string City,
    string State,
    int? IbgeCode,
    bool IsActive = true);
