namespace MeAjudaAi.ApiService.Middlewares;

/// <summary>
/// Middleware que previne ataques CRIME/BREACH verificando se é seguro comprimir a resposta.
/// Deve ser registrado ANTES do UseResponseCompression() no pipeline.
/// 
/// NOTA: Este middleware é executado ANTES de UseAuthentication(), então não pode usar
/// HttpContext.User.Identity.IsAuthenticated. Em vez disso, verifica headers de autenticação.
/// </summary>
public class CompressionSecurityMiddleware
{
    private readonly RequestDelegate _next;

    public CompressionSecurityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Verifica se é seguro comprimir antes de permitir que o middleware de compressão processe
        if (!IsSafeForCompression(context))
        {
            // Desabilita a compressão para esta requisição removendo o Accept-Encoding
            context.Request.Headers.Remove("Accept-Encoding");
        }

        await _next(context);
    }

    /// <summary>
    /// Verifica se é seguro comprimir esta requisição.
    /// Executado antes da autenticação para prevenir ataques CRIME/BREACH.
    /// </summary>
    private static bool IsSafeForCompression(HttpContext context)
    {
        var request = context.Request;

        // Verifica se há headers de autenticação (indica requisição autenticada)
        // Não comprime requisições autenticadas para proteção BREACH/CRIME
        if (request.Headers.ContainsKey("Authorization") ||
            request.Headers.ContainsKey("X-API-Key"))
        {
            return false;
        }

        // Não comprimir endpoints sensíveis
        var sensitivePaths = new[]
        {
            "/auth", "/login", "/token", "/refresh", "/logout",
            "/api/auth", "/api/login", "/api/token", "/api/refresh",
            "/connect", "/oauth", "/openid", "/identity",
            "/users/profile", "/users/me", "/account"
        };

        if (sensitivePaths.Any(sensitive =>
            request.Path.StartsWithSegments(sensitive, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return true;
    }
}
