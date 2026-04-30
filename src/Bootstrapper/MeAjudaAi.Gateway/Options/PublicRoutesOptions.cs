namespace MeAjudaAi.Gateway.Options;

/// <summary>
/// Configuração de rotas públicas do Gateway que não requerem autenticação.
/// Usada pelo <see cref="MeAjudaAi.Gateway.Middleware.AuthenticationGuardMiddleware"/> para
/// determinar quais caminhos podem ser acessados sem um token JWT válido.
/// </summary>
public class PublicRoutesOptions
{
    public const string SectionName = "PublicRoutes";

    /// <summary>
    /// Lista de prefixos de rota que são acessíveis sem autenticação.
    /// A verificação é feita via <c>StartsWith</c> case-insensitive.
    /// Exemplo: "/health" permite /health, /health/live, /health/ready.
    /// </summary>
    public List<string> Routes { get; set; } = ["/health"];
}