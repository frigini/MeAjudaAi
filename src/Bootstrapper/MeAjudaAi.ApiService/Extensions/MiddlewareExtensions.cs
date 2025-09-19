using MeAjudaAi.ApiService.Middlewares;

namespace MeAjudaAi.ApiService.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseApiMiddlewares(this IApplicationBuilder app)
    {
        // Security headers (early in pipeline)
        app.UseMiddleware<SecurityHeadersMiddleware>();
        
        // Response compression
        app.UseResponseCompression();
        
        // Static files with caching
        app.UseMiddleware<StaticFilesMiddleware>();
        
        // Request logging
        app.UseMiddleware<RequestLoggingMiddleware>();
        
        // Rate limiting
        app.UseMiddleware<RateLimitingMiddleware>();

        return app;
    }
}