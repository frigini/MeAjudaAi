using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Exceptions;

internal static class Extensions
{
    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
        => services.AddScoped<GlobalExceptionHandler>();

    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionHandler>();
}