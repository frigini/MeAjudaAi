using System.ComponentModel.DataAnnotations;

namespace MeAjudaAi.Modules.Search.Application.DTOs;

/// <summary>
/// DTO representing geographic coordinates.
/// Valid ranges: Latitude [-90, 90], Longitude [-180, 180].
/// </summary>
public sealed record LocationDto
{
    /// <summary>
    /// Latitude in decimal degrees. Valid range: -90 (South Pole) to 90 (North Pole).
    /// </summary>
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public required double Latitude { get; init; }

    /// <summary>
    /// Longitude in decimal degrees. Valid range: -180 (West) to 180 (East).
    /// </summary>
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public required double Longitude { get; init; }
}
