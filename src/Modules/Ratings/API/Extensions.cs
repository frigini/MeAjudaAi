using MeAjudaAi.Modules.Ratings.API.Endpoints;
using MeAjudaAi.Modules.Ratings.Application;
using MeAjudaAi.Modules.Ratings.Infrastructure;
using MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Ratings.Domain.Events;
using MeAjudaAi.Shared.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Ratings.API;

public static class Extensions
{
    public static IServiceCollection AddRatingsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);

        // Manipuladores de eventos de domínio
        services.AddScoped<IEventHandler<ReviewApprovedDomainEvent>, ReviewApprovedDomainEventHandler>();

        return services;
    }

    public static IEndpointRouteBuilder UseRatingsModule(this IEndpointRouteBuilder app)
    {
        RatingsEndpoints.Map(app);
        return app;
    }
}
