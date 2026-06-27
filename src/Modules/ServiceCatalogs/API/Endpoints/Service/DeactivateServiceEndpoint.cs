using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

/// <summary>
/// Endpoint para desativar um serviço no catálogo.
/// Requer privilégios de administrador.
/// </summary>
public class DeactivateServiceEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Mapeia o endpoint POST /{id}/deactivate para desativar um serviço.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost(ApiEndpoints.ServiceCatalogs.Services.Deactivate, DeactivateAsync)
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

    /// <summary>
    /// Desativa o serviço, removendo-o do catálogo ativo.
    /// </summary>
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
