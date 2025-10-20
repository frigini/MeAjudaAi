using Asp.Versioning;

namespace MeAjudaAi.ApiService.Extensions;

public static class VersioningExtensions
{
    public static IServiceCollection AddApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;

            // Use composite reader para manter compatibilidade com clientes existentes
            // Suporta: URL segments (/api/v1/users), headers (api-version), query strings (?api-version=1.0)
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),    // /api/v1/users (preferido para novos endpoints)
                new HeaderApiVersionReader("api-version"),      // Header: api-version: 1.0
                new QueryStringApiVersionReader("api-version")  // Query: ?api-version=1.0
            );
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
        });

        return services;
    }
}
