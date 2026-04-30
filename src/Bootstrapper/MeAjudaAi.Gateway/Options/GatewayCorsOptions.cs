namespace MeAjudaAi.Gateway.Options;

public class GatewayCorsOptions
{
    public const string SectionName = "Cors";
    public List<string> AllowedOrigins { get; set; } = [];
    public List<string> AllowedMethods { get; set; } = ["GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS"];
    public List<string> AllowedHeaders { get; set; } = ["*"];
    public bool AllowCredentials { get; set; } = true;
    public int MaxAgeSeconds { get; set; } = 3600;
}