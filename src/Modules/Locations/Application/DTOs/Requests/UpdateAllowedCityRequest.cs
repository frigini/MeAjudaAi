using System.Diagnostics.CodeAnalysis;
namespace MeAjudaAi.Modules.Locations.Application.DTOs.Requests;

/// <summary>
/// Request DTO para atualização de cidade permitida
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record UpdateAllowedCityRequest(
    string CityName,
    string StateSigla,
    int? IbgeCode,
    double Latitude,
    double Longitude,
    double ServiceRadiusKm,
    bool IsActive);
