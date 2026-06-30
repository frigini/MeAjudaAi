using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

/// <summary>
/// Endpoint para ativar um serviço no catálogo.
/// Requer privilégios de administrador.
/// </summary>
public class ActivateServiceEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Mapeia o endpoint POST /{id}/activate para ativar um serviço.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost(ApiEndpoints.ServiceCatalogs.Services.Activate, ActivateAsync)
            .WithName(ApiEndpoints.ServiceCatalogs.Services.Names.Activate)
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

    /// <summary>
    /// Ativa o serviço, tornando-o disponível no catálogo.
    /// </summary>
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
