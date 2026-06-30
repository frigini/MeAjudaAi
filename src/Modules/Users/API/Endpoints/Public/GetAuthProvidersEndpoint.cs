using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Identity.Enums;
using MeAjudaAi.Shared.Endpoints;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Users.API.Endpoints.Public;

/// <summary>
/// Endpoint responsável por fornecer a lista de provedores de identidade social disponíveis para autenticação.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthProvidersEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("auth/providers", () =>
        {
            var providers = Enum.GetNames<EAuthProvider>();
            return Results.Ok(providers);
        })
        .AllowAnonymous()
        .WithTags("Auth")
        .WithName(ApiEndpoints.Users.Names.GetAuthProviders)
        .WithSummary("Lista os provedores de identidade social disponíveis.");
    }
}
