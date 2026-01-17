using System.ComponentModel.DataAnnotations;

namespace MeAjudaAi.Modules.SearchProviders.Application.DTOs;

/// <summary>
/// DTO representing geographic coordinates.
/// Valid ranges: Latitude [-90, 90], Longitude [-180, 180].
/// </summary>
public sealed record LocationDto(
    [property: Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    double Latitude,
    [property: Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    double Longitude);
