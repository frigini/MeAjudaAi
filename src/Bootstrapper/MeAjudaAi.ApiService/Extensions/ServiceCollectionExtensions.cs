using MeAjudaAi.ApiService.Options;

namespace MeAjudaAi.ApiService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RateLimitOptions>()
            .Configure(opts => configuration.GetSection(RateLimitOptions.SectionName).Bind(opts))
            .Validate(opts => opts.DefaultRequestsPerMinute > 0, "DefaultRequestsPerMinute must be greater than zero")
            .Validate(opts => opts.AuthRequestsPerMinute > 0, "AuthRequestsPerMinute must be greater than zero")
            .Validate(opts => opts.SearchRequestsPerMinute > 0, "SearchRequestsPerMinute must be greater than zero")
            .Validate(opts => opts.WindowInSeconds > 0, "WindowInSeconds must be greater than zero")
            .ValidateOnStart();

        services.AddDocumentation();
        services.AddCorsPolicy();
        services.AddMemoryCache();

        return services;
    }

    public static IApplicationBuilder UseApiServices(
        this IApplicationBuilder app,
        IWebHostEnvironment environment)
    {
        app.UseApiMiddlewares();

        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "MeAjudaAi API v1");
                options.RoutePrefix = "docs";
                options.DisplayRequestDuration();
                options.EnableTryItOutByDefault();
            });
        }

        app.UseCors("DefaultPolicy");
        app.UseAuthentication();
        app.UseAuthorization();

        if (!environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        return app;
    }
}