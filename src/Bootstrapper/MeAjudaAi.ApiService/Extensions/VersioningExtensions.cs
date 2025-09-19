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
            // Use only URL segment versioning for simplicity and clarity
            options.ApiVersionReader = new UrlSegmentApiVersionReader(); // /api/v1/users
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
        });

        return services;
    }
}