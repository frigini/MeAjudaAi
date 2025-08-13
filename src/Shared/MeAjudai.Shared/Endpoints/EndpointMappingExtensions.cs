using Microsoft.AspNetCore.Routing;
using System.Reflection;

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

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app, params Type[] endpointTypes)
    {
        foreach (var type in endpointTypes)
        {
            if (type.IsAssignableTo(typeof(IEndpoint)))
            {
                var mapMethod = type.GetMethod("Map", [typeof(IEndpointRouteBuilder)]);
                mapMethod?.Invoke(null, [app]);
            }
        }
        return app;
    }

    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();

        var endpointTypes = assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IEndpoint)) &&
                       !t.IsInterface &&
                       !t.IsAbstract);

        return app.MapEndpoints([.. endpointTypes]);
    }
}