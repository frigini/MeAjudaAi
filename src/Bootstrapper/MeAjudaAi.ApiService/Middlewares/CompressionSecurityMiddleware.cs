using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<CompressionSecurityMiddleware> _logger;

    public CompressionSecurityMiddleware(RequestDelegate next, ILogger<CompressionSecurityMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var isSafe = IsSafeForCompression(context);
        
        // Verifica se é seguro comprimir antes de permitir que o middleware de compressão processe
        if (!isSafe)
        {
            // Desabilita a compressão para esta requisição. 
            // Definir como "identity" é mais explícito do que remover o header para alguns proxies/middlewares.
            context.Request.Headers["Accept-Encoding"] = "identity";
            
            // Adiciona um marker no context para auditoria/testes
            context.Items["CompressionDisabledBySecurity"] = true;
            
            // Adiciona o header de resposta ANTES de chamar o próximo middleware
            context.Response.Headers["X-Compression-Disabled"] = "Security-Policy";
        }

        await _next(context);
    }

    /// <summary>
    /// Verifica se é seguro comprimir esta requisição.
    /// Executado antes da autenticação para prevenir ataques CRIME/BREACH.
    /// </summary>
    private bool IsSafeForCompression(HttpContext context)
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
