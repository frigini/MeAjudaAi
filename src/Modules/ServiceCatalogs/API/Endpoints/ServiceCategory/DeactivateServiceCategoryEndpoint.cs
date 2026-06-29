using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.ServiceCategory;

/// <summary>
/// Endpoint para desativar uma categoria de serviço.
/// Requer privilégios de administrador.
/// </summary>
public class DeactivateServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Mapeia o endpoint POST /{id}/deactivate para desativar uma categoria.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost(ApiEndpoints.ServiceCatalogs.Categories.Deactivate, DeactivateAsync)
            .WithName(ApiEndpoints.ServiceCatalogs.Categories.Names.Deactivate)
            .WithSummary("Desativar categoria de serviço")
            .WithDescription("""
                Desativa uma categoria de serviços.
                
                **Efeitos:**
                - Categoria não aparece em listagens públicas
                - Impede criação de novos serviços nesta categoria
                - Serviços existentes permanecem no sistema (soft-delete)
                
                **Nota:** Preferível à deleção quando há serviços associados.
                
                **Permissões:** Requer privilégios de administrador
                """)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAdmin();

    /// <summary>
    /// Desativa a categoria, removendo-a das listagens públicas.
    /// </summary>
    private static async Task<IResult> DeactivateAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateServiceCategoryCommand(id);
        var result = await commandDispatcher.SendAsync<DeactivateServiceCategoryCommand, Result>(command, cancellationToken);
        return HandleNoContent(result);
    }
}
