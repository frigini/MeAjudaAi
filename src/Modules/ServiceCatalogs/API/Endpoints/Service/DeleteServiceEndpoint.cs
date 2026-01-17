using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

public class DeleteServiceEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteService")
            .WithSummary("Deletar serviço")
            .WithDescription("""
                Deleta um serviço do catálogo permanentemente.
                
                **Validações:**
                - ID não pode ser vazio
                - Serviço deve existir
                - Nenhum provedor pode estar oferecendo este serviço
                
                **Importante:** Operação destrutiva. Se provedores oferecem o serviço,
                use desativação em vez de deleção para preservar dados históricos.
                
                **Permissões:** Requer privilégios de administrador
                """)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAdmin();

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new DeleteServiceCommand(id);
        var result = await commandDispatcher.SendAsync<DeleteServiceCommand, Result>(command, cancellationToken);
        return HandleNoContent(result);
    }
}
