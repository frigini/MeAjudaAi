using MeAjudaAi.ApiService.Middlewares;

namespace MeAjudaAi.ApiService.Extensions;

/// <summary>
/// Extension methods para registrar o middleware CSP.
/// </summary>
public static class ContentSecurityPolicyMiddlewareExtensions
{
    public static IApplicationBuilder UseContentSecurityPolicy(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ContentSecurityPolicyMiddleware>();
    }
}
