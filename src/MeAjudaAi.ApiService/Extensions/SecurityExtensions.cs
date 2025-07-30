namespace MeAjudaAi.ApiService.Extensions;

public static class SecurityExtensions
{
    public static IServiceCollection AddCorsPolicy(
    this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        services.AddAuthentication();
        services.AddAuthorization();

        return services;
    }
}