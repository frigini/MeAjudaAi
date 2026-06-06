using MeAjudaAi.Modules.Ratings.API.Endpoints;
using MeAjudaAi.Modules.Ratings.Application;
using MeAjudaAi.Modules.Ratings.Infrastructure;
using MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Ratings.Domain.Events;
using MeAjudaAi.Shared.Events;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers.Integration;
using MeAjudaAi.Shared.Messaging.Messages.Users;

namespace MeAjudaAi.Modules.Ratings.API;

public static class Extensions
{
    public static IServiceCollection AddRatingsModule(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);

        // Manipuladores de eventos de domínio
        services.AddScoped<IEventHandler<ReviewApprovedDomainEvent>, ReviewApprovedDomainEventHandler>();
        services.AddScoped<IEventHandler<ReviewRejectedDomainEvent>, ReviewRejectedDomainEventHandler>();

        // Manipuladores de eventos de integração
        services.AddScoped<IEventHandler<UserDeletedIntegrationEvent>, UserDeletedIntegrationEventHandler>();

        return services;
    }

    public static IEndpointRouteBuilder UseRatingsModule(this IEndpointRouteBuilder app)
    {
        RatingsEndpoints.Map(app);
        return app;
    }
}
