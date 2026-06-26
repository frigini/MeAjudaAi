using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.SearchProviders.Application.DTOs;

/// <summary>
/// DTO representando coordenadas geográficas.
/// Intervalos válidos: Latitude [-90, 90], Longitude [-180, 180].
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record LocationDto(double Latitude, double Longitude);
