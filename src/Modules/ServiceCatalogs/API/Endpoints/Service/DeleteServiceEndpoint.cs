using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

/// <summary>
/// Endpoint para deletar um serviço do catálogo.
/// Requer privilégios de administrador. Nenhum provedor pode estar oferecendo o serviço.
/// </summary>
public class DeleteServiceEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Mapeia o endpoint DELETE /{id} para deletar um serviço.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete(ApiEndpoints.ServiceCatalogs.Services.Delete, DeleteAsync)
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

    /// <summary>
    /// Deleta o serviço permanentemente. Falha se houver provedores oferecendo-o.
    /// </summary>
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
