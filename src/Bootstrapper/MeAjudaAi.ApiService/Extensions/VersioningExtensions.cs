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
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),           // /api/v1/users
                new HeaderApiVersionReader("X-Version"),    // Header: X-Version: 1.0
                new QueryStringApiVersionReader("version")  // ?version=1.0
            );
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}