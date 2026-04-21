using MeAjudaAi.Modules.Bookings.Application;
using MeAjudaAi.Modules.Bookings.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.Bookings.API;

public static class Extensions
{
    /// <summary>
    /// Registra os serviços e configurações do módulo de agendamentos no container de DI.
    /// </summary>
    public static IServiceCollection AddBookingsModule(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);

        return services;
    }

    /// <summary>
    /// Configura e mapeia os middlewares do módulo de agendamentos.
    /// </summary>
    public static IApplicationBuilder UseBookingsModule(this IApplicationBuilder app)
    {
        // Middlewares específicos do módulo se necessário
        return app;
    }

    /// <summary>
    /// Mapeia os endpoints do módulo de agendamentos.
    /// </summary>
    public static IEndpointRouteBuilder MapBookingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Endpoints serão mapeados aqui (ex: BookingsEndpoints.Map(endpoints))
        return endpoints;
    }
}
