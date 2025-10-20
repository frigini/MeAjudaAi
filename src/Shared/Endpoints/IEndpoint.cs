using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Shared.Endpoints;

public interface IEndpoint
{
    static abstract void Map(IEndpointRouteBuilder app);
}
