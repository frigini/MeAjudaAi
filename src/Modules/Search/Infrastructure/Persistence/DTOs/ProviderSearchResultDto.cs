namespace MeAjudaAi.Modules.Search.Infrastructure.Persistence.DTOs;

/// <summary>
/// DTO para mapear resultados da query espacial Dapper/PostGIS.
/// Usado apenas internamente no repositório para otimizar queries geoespaciais.
/// </summary>
internal sealed class ProviderSearchResultDto
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int SubscriptionTier { get; set; }
    public Guid[] ServiceIds { get; set; } = Array.Empty<Guid>();
    public string? City { get; set; }
    public string? State { get; set; }
    public bool IsActive { get; set; }
    public double DistanceKm { get; set; }
}
