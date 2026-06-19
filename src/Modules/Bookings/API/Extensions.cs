using MeAjudaAi.Modules.Bookings.API.Endpoints;
using MeAjudaAi.Modules.Bookings.Application;
using MeAjudaAi.Modules.Bookings.Infrastructure;
using System.Diagnostics.CodeAnalysis;

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

    public static IEndpointRouteBuilder UseBookingsModule(this IEndpointRouteBuilder app)
    {
        BookingsEndpoints.Map(app);
        return app;
    }
}