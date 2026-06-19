using MeAjudaAi.Modules.Payments.API.Endpoints;
using MeAjudaAi.Modules.Payments.Application;
using MeAjudaAi.Modules.Payments.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Payments.API;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);

        return services;
    }

    public static IEndpointRouteBuilder UsePaymentsModule(this IEndpointRouteBuilder app)
    {
        PaymentsEndpoints.Map(app);
        return app;
    }
}

