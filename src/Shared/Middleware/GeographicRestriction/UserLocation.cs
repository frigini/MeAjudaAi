namespace MeAjudaAi.Shared.Middleware.GeographicRestriction;

public record UserLocation(string? City, string? State)
{
    public static UserLocation Create(string? city, string? state) => new(city, state);
}
