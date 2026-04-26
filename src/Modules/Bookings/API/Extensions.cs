using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Modules.Bookings.API.Endpoints;
using MeAjudaAi.Modules.Bookings.API.Endpoints.Public;
using MeAjudaAi.Modules.Bookings.Application;
using MeAjudaAi.Modules.Bookings.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.Bookings.API;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddBookingsModule(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);

        return services;
    }

    public static WebApplication UseBookingsModule(this WebApplication app)
    {
        app.MapBookingsEndpoints();
        return app;
    }

    public static IEndpointRouteBuilder MapBookingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        BookingsEndpoints.Map(endpoints);
        return endpoints;
    }
}
