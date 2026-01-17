using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

public class ActivateServiceEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id:guid}/activate", ActivateAsync)
            .WithName("ActivateService")
            .WithSummary("Ativar serviço")
            .WithDescription("""
                Ativa um serviço, tornando-o disponível no catálogo.
                
                **Efeitos:**
                - Serviço fica visível em listagens públicas
                - Provedores podem adicionar este serviço às suas ofertas
                - Serviço aparece em buscas de serviços ativos
                
                **Permissões:** Requer privilégios de administrador
                """)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAdmin();

    private static async Task<IResult> ActivateAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new ActivateServiceCommand(id);
        var result = await commandDispatcher.SendAsync<ActivateServiceCommand, Result>(command, cancellationToken);
        return HandleNoContent(result);
    }
}
