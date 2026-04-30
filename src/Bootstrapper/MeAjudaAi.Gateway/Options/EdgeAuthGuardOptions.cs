namespace MeAjudaAi.Gateway.Options;

public class EdgeAuthGuardOptions
{
    public const string SectionName = "EdgeAuthGuard";

    public bool Enabled { get; set; } = true;
    public List<string> PublicRoutes { get; set; } = [
        "/health",
        "/swagger",
        "/api/v1/auth/login",
        "/api/v1/auth/register",
        "/api/v1/providers/public",
        "/api/v1/customers/register",
        "/api/v1/providers/register",
        "/webhooks/stripe"
    ];
    public string ChallengeHeader { get; set; } = "X-Gateway-Challenge";
    public string AuthenticatedHeader { get; set; } = "X-Gateway-Authenticated";
}