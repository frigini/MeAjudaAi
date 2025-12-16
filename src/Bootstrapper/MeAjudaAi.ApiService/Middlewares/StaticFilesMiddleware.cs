using Microsoft.Net.Http.Headers;

namespace MeAjudaAi.ApiService.Middlewares;

/// <summary>
/// Middleware para servir arquivos estáticos com cabeçalhos de cache apropriados
/// </summary>
public class StaticFilesMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    // Cabeçalhos de cache pré-computados para melhor performance
    private const string LongCacheControl = "public,max-age=2592000,immutable"; // 30 dias
    private const string NoCacheControl = "no-cache,no-store,must-revalidate";
    private static readonly TimeSpan LongCacheDuration = TimeSpan.FromDays(30);

    // Extensões de arquivos estáticos que devem ser cacheados
    private static readonly HashSet<string> CacheableExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".css", ".js", ".ico", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".woff", ".woff2", ".ttf", ".eot"
    };

    public async Task InvokeAsync(HttpContext context)
    {
        // Processa apenas requisições de arquivos estáticos
        if (context.Request.Path.StartsWithSegments("/css") ||
            context.Request.Path.StartsWithSegments("/js") ||
            context.Request.Path.StartsWithSegments("/images") ||
            context.Request.Path.StartsWithSegments("/fonts"))
        {
            var extension = Path.GetExtension(context.Request.Path.Value);

            if (!string.IsNullOrEmpty(extension) && CacheableExtensions.Contains(extension))
            {
                // Define cabeçalhos de cache antes de servir o arquivo
                context.Response.OnStarting(() =>
                {
                    var headers = context.Response.Headers;
                    headers[HeaderNames.CacheControl] = LongCacheControl;
                    headers[HeaderNames.Expires] = DateTimeOffset.UtcNow.Add(LongCacheDuration).ToString("R");
                    // Removido atribuição manual de ETag - deixa middleware de arquivos estáticos do ASP.NET Core lidar com ETags baseados em conteúdo

                    return Task.CompletedTask;
                });
            }
            else
            {
                // Não cacheia tipos de arquivo desconhecidos
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers[HeaderNames.CacheControl] = NoCacheControl;
                    return Task.CompletedTask;
                });
            }
        }

        await _next(context);
    }
}
