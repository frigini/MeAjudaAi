using System.IO.Compression;
using MeAjudaAi.ApiService.Providers.Compression;
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
            // Permite compressão HTTPS - proteção contra CRIME/BREACH via middleware de segurança
            options.EnableForHttps = true;

            // Usa provedores personalizados
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
