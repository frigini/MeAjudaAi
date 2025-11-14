namespace MeAjudaAi.Shared.Contracts.Modules.Search.DTOs;

/// <summary>
/// Geographic location DTO for module API.
/// </summary>
public sealed record ModuleLocationDto
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
