namespace MeAjudaAi.ApiService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services)
    {
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