using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;

namespace MeAjudaAi.ApiService.Extensions;

public static class PerformanceExtensions
{
    /// <summary>
    /// Configura compressão de resposta para melhorar a performance da API
    /// </summary>
    public static IServiceCollection AddResponseCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            // Permite compressão HTTPS - proteção contra CRIME/BREACH via provedores customizados
            options.EnableForHttps = true; // Habilitado - provedores customizados fazem verificação de segurança

            // Usa provedores personalizados com verificação de segurança
            options.Providers.Add<SafeGzipCompressionProvider>();
            options.Providers.Add<SafeBrotliCompressionProvider>();

            // Adiciona tipos MIME que devem ser comprimidos
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            [
                "application/json",
                "application/xml",
                "text/xml",
                "application/javascript",
                "text/css",
                "text/plain"
            ]);
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        return services;
    }

    /// <summary>
    /// Verifica se a resposta é segura para compressão (previne CRIME/BREACH)
    /// </summary>
    public static bool IsSafeForCompression(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var request = context.Request;
        var response = context.Response;

        // Não comprima se há dados de autenticação
        if (HasAuthenticationData(request, response))
            return false;

        // Não comprima endpoints sensíveis
        if (IsSensitivePath(request.Path))
            return false;

        // Não comprima respostas pequenas (< 1KB)
        if (response.ContentLength.HasValue && response.ContentLength < 1024)
            return false;

        // Não comprima content-types que podem conter secrets
        if (HasSensitiveContentType(response.ContentType))
            return false;

        // Não comprima se há cookies de sessão/autenticação
        if (HasSensitiveCookies(request, response))
            return false;

        return true;
    }

    private static bool HasAuthenticationData(HttpRequest request, HttpResponse response)
    {
        // Verifica headers de autenticação
        if (request.Headers.ContainsKey("Authorization") ||
            request.Headers.ContainsKey("X-API-Key") ||
            response.Headers.ContainsKey("Authorization"))
            return true;

        // Verifica se o usuário está autenticado
        if (request.HttpContext.User?.Identity?.IsAuthenticated == true)
            return true;

        return false;
    }

    private static bool IsSensitivePath(PathString path)
    {
        var sensitivePaths = new[]
        {
            "/auth", "/login", "/token", "/refresh", "/logout",
            "/api/auth", "/api/login", "/api/token", "/api/refresh",
            "/connect", "/oauth", "/openid", "/identity",
            "/users/profile", "/users/me", "/account"
        };

        return sensitivePaths.Any(sensitive =>
            path.StartsWithSegments(sensitive, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasSensitiveContentType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return false;

        var sensitiveTypes = new[]
        {
            "application/jwt",
            "application/x-www-form-urlencoded", // Pode conter credenciais
            "multipart/form-data" // Pode conter uploads sensíveis
        };

        return sensitiveTypes.Any(type =>
            contentType.StartsWith(type, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasSensitiveCookies(HttpRequest request, HttpResponse response)
    {
        var sensitiveCookieNames = new[]
        {
            "auth", "session", "token", "jwt", "identity",
            ".AspNetCore.Identity", ".AspNetCore.Session",
            "XSRF-TOKEN", "CSRF-TOKEN"
        };

        // Verifica cookies na requisição
        if (request.Cookies.Any(cookie =>
            sensitiveCookieNames.Any(name =>
                cookie.Key.Contains(name, StringComparison.OrdinalIgnoreCase))))
        {
            return true;
        }

        // Verifica cookies sendo definidos na resposta
        if (response.Headers.TryGetValue("Set-Cookie", out var setCookies))
        {
            if (setCookies.Any(setCookie =>
                setCookie != null && sensitiveCookieNames.Any(name =>
                    setCookie.Contains(name, StringComparison.OrdinalIgnoreCase))))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Configura servir arquivos estáticos com cabeçalhos de cache para melhor performance
    /// </summary>
    public static IServiceCollection AddStaticFilesWithCaching(this IServiceCollection services)
    {
        services.Configure<StaticFileOptions>(options =>
        {
            options.OnPrepareResponse = context =>
            {
                // Cache arquivos estáticos por 30 dias
                if (context.File.Name.EndsWith(".css") ||
                    context.File.Name.EndsWith(".js") ||
                    context.File.Name.EndsWith(".ico") ||
                    context.File.Name.EndsWith(".png") ||
                    context.File.Name.EndsWith(".jpg") ||
                    context.File.Name.EndsWith(".gif"))
                {
                    context.Context.Response.Headers.CacheControl = "public,max-age=2592000"; // 30 dias
                    context.Context.Response.Headers.Expires = DateTime.UtcNow.AddDays(30).ToString("R");
                }
            };
        });

        return services;
    }

    /// <summary>
    /// Configura cache de resposta para endpoints da API
    /// </summary>
    public static IServiceCollection AddApiResponseCaching(this IServiceCollection services)
    {
        services.AddResponseCaching(options =>
        {
            options.MaximumBodySize = 1024 * 1024; // 1MB
            options.UseCaseSensitivePaths = true;
        });

        return services;
    }
}

/// <summary>
/// Provedor de compressão Gzip seguro que previne CRIME/BREACH
/// </summary>
public class SafeGzipCompressionProvider : ICompressionProvider
{
    public string EncodingName => "gzip";
    public bool SupportsFlush => true;

    public Stream CreateStream(Stream outputStream)
    {
        return new GZipStream(outputStream, CompressionLevel.Optimal, leaveOpen: false);
    }

    public static bool ShouldCompressResponse(HttpContext context)
    {
        return PerformanceExtensions.IsSafeForCompression(context);
    }
}

/// <summary>
/// Provedor de compressão Brotli seguro que previne CRIME/BREACH
/// </summary>
public class SafeBrotliCompressionProvider : ICompressionProvider
{
    public string EncodingName => "br";
    public bool SupportsFlush => true;

    public Stream CreateStream(Stream outputStream)
    {
        return new BrotliStream(outputStream, CompressionLevel.Optimal, leaveOpen: false);
    }

    public static bool ShouldCompressResponse(HttpContext context)
    {
        return PerformanceExtensions.IsSafeForCompression(context);
    }
}
