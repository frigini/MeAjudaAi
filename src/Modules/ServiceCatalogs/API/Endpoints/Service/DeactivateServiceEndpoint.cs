using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

public class DeactivateServiceEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id:guid}/deactivate", DeactivateAsync)
            .WithName("DeactivateService")
            .WithSummary("Desativar serviço")
            .WithDescription("""
                Desativa um serviço, removendo-o do catálogo ativo.
                
                **Efeitos:**
                - Serviço não aparece em listagens públicas
                - Provedores não podem adicionar este serviço a novas ofertas
                - Serviço preserva dados históricos (soft-delete)
                
                **Nota:** Preferível à deleção quando provedores já oferecem o serviço.
                
                **Permissões:** Requer privilégios de administrador
                """)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAdmin();

    private static async Task<IResult> DeactivateAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateServiceCommand(id);
        var result = await commandDispatcher.SendAsync<DeactivateServiceCommand, Result>(command, cancellationToken);
        return HandleNoContent(result);
    }
}
