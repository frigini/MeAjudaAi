using MeAjudaAi.ApiService.Services.Orchestration;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.ApiService.Endpoints;

[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class ConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapConfigurationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/configuration")
            .WithTags("Configuration");

        group.MapGet("/client", GetClientConfiguration)
            .WithName("GetClientConfiguration")
            .WithSummary("Obtém a configuração do cliente Blazor WASM")
            .WithDescription("Retorna configurações não-sensíveis necessárias para o frontend (Keycloak, URLs, feature flags)")
            .Produces<MeAjudaAi.Contracts.Configuration.ClientConfiguration>(StatusCodes.Status200OK)
            .AllowAnonymous();

        return endpoints;
    }

    private static IResult GetClientConfiguration(
        [FromServices] IClientConfigurationService configurationService)
    {
        var clientConfig = configurationService.GetClientConfiguration();
        return TypedResults.Ok(clientConfig);
    }
}
