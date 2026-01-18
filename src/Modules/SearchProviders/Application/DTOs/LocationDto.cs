using System.ComponentModel.DataAnnotations;

namespace MeAjudaAi.Modules.SearchProviders.Application.DTOs;

/// <summary>
/// DTO representando coordenadas geográficas.
/// Intervalos válidos: Latitude [-90, 90], Longitude [-180, 180].
/// </summary>
public sealed record LocationDto(
    [property: Range(-90, 90, ErrorMessage = "Latitude deve estar entre -90 e 90")]
    double Latitude,
    [property: Range(-180, 180, ErrorMessage = "Longitude deve estar entre -180 e 180")]
    double Longitude);
