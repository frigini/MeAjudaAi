namespace MeAjudaAi.Shared.Middleware.GeographicRestriction;

public class GeographicRestrictionOptions
{
    public const string SectionName = "GeographicRestriction";

    public bool Enabled { get; set; } = false;
    public List<string> AllowedStates { get; set; } = [];
    public List<string> AllowedCities { get; set; } = [];
    public string? BlockedMessage { get; set; }
    public string? DefaultBlockedMessage { get; set; }
    public bool FailOpen { get; set; } = true;
}
