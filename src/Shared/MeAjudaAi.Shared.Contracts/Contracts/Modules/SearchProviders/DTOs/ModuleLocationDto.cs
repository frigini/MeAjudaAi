namespace MeAjudaAi.Shared.Contracts.Modules.SearchProviders.DTOs;

/// <summary>
/// Geographic location DTO for module API.
/// </summary>
public sealed record ModuleLocationDto
{
    /// <summary>
    /// Latitude coordinate. Valid range: -90 to +90 degrees.
    /// </summary>
    public required double Latitude { get; init; }

    /// <summary>
    /// Longitude coordinate. Valid range: -180 to +180 degrees.
    /// </summary>
    public required double Longitude { get; init; }
}

