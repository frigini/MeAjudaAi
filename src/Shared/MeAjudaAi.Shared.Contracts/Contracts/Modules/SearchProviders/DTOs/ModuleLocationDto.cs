namespace MeAjudaAi.Shared.Contracts.Modules.SearchProviders.DTOs;

/// <summary>
/// DTO de localização geográfica para a API do módulo.
/// </summary>
public sealed record ModuleLocationDto
{
    /// <summary>
    /// Coordenada de latitude. Intervalo válido: -90 a +90 graus.
    /// </summary>
    public required double Latitude { get; init; }

    /// <summary>
    /// Coordenada de longitude. Intervalo válido: -180 a +180 graus.
    /// </summary>
    public required double Longitude { get; init; }
}

