using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Shared.Endpoints;

public static class EndpointMappingExtensions
{
    public static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }

    public static RouteGroupBuilder MapEndpoint<TEndpoint>(this RouteGroupBuilder group)
        where TEndpoint : IEndpoint
    {
        TEndpoint.Map(group);
        return group;
    }
}