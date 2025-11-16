namespace MeAjudaAi.Modules.Search.Application.DTOs;

/// <summary>
/// DTO representing geographic coordinates.
/// </summary>
public sealed record LocationDto
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
