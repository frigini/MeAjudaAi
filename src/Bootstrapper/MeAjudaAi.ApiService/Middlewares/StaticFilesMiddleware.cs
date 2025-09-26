using Microsoft.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

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
                    headers[HeaderNames.ETag] = GenerateETag(context.Request.Path.Value);
                    
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

    private static string GenerateETag(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return "\"default\"";
            
        try
        {
            // Geração determinística de ETag baseada no SHA-256 do caminho
            var pathBytes = Encoding.UTF8.GetBytes(path);
            var hashBytes = SHA256.HashData(pathBytes);
            
            // Converte os bytes do hash para string hexadecimal em minúsculas
            // Usa apenas os primeiros 16 bytes (32 caracteres hex) para um ETag mais compacto
            var hexHash = Convert.ToHexString(hashBytes[..16]).ToLowerInvariant();
            return $"\"{hexHash}\"";
        }
        catch
        {
            // Em caso de erro no hashing, retorna um ETag fixo para evitar exceções
            return "\"fallback\"";
        }
    }
}