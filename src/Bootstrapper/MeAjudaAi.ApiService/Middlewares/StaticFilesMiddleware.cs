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
    private static readonly string LongCacheExpires = DateTime.UtcNow.AddDays(30).ToString("R");
    
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
                    headers.CacheControl = LongCacheControl;
                    headers.Expires = LongCacheExpires;
                    headers.ETag = GenerateETag(context.Request.Path.Value);
                    
                    return Task.CompletedTask;
                });
            }
            else
            {
                // Não cacheia tipos de arquivo desconhecidos
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers.CacheControl = NoCacheControl;
                    return Task.CompletedTask;
                });
            }
        }

        await _next(context);
    }

    private static string GenerateETag(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return "\"default\"";
            
        // Geração simples de ETag baseada no hash do caminho
        var hash = path.GetHashCode();
        return $"\"{hash:x}\"";
    }
}