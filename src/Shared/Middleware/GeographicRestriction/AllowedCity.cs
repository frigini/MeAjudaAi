namespace MeAjudaAi.Shared.Middleware.GeographicRestriction;

public record AllowedCity(string Name, string? State)
{
    public static AllowedCity Create(string name, string? state = null) => new(name, state);
}
