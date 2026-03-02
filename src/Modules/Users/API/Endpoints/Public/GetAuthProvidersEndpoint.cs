using MeAjudaAi.Contracts.Identity.Enums;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.Public;

public class GetAuthProvidersEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("auth/providers", () =>
        {
            var providers = Enum.GetValues<EAuthProvider>();
            return Results.Ok(providers);
        })
        .AllowAnonymous()
        .WithTags("Auth")
        .WithName("GetAuthProviders")
        .WithSummary("Lista os provedores de identidade social disponíveis.");
    }
}
