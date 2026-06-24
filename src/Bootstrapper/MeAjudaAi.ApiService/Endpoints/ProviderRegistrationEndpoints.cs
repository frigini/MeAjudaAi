using MeAjudaAi.ApiService.Services.Orchestration;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.ApiService.Endpoints;

[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class ProviderRegistrationEndpoints
{
    public static IEndpointRouteBuilder MapProviderRegistrationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/providers")
            .WithTags("Providers - Public");

        group.MapPost("/register", RegisterProviderAsync)
            .WithName("RegisterProvider")
            .WithSummary("Auto-registro de prestador de serviços")
            .WithDescription(
                "Inicia o cadastro de um prestador. Cria usuário no Keycloak com role 'provider-standard' " +
                "e a entidade Provider com Tier=Standard. Endpoint público, sem autenticação.")
            .Produces<Response<ProviderDto>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        return endpoints;
    }

    private static async Task<IResult> RegisterProviderAsync(
        [FromBody] RegisterProviderRequest request,
        [FromServices] IProviderRegistrationOrchestrator orchestrator,
        CancellationToken cancellationToken)
    {
        var result = await orchestrator.RegisterProviderAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return Results.BadRequest(result.Error?.Message);
        }

        return Results.Created(
            $"/api/v1/providers/{result.Value.Id}",
            new Response<ProviderDto>(result.Value));
    }
}
