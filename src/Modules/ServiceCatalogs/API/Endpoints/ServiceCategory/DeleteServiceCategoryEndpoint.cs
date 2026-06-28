using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.ServiceCategory;

/// <summary>
/// Endpoint para deletar uma categoria de serviço.
/// Requer privilégios de administrador. A categoria não pode ter serviços associados.
/// </summary>
public class DeleteServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Mapeia o endpoint DELETE /{id} para deletar uma categoria.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete(ApiEndpoints.ServiceCatalogs.Categories.Delete, DeleteAsync)
            .WithName("DeleteServiceCategory")
            .WithSummary("Deletar categoria de serviço")
            .WithDescription("""
                Deleta uma categoria de serviços permanentemente.
                
                **Validações:**
                - ID não pode ser vazio
                - Categoria deve existir
                - Categoria não pode ter serviços associados
                
                **Importante:** Operação destrutiva. Categorias com serviços não podem
                ser deletadas. Use desativação ou mova os serviços primeiro.
                
                **Permissões:** Requer privilégios de administrador
                """)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAdmin();

    /// <summary>
    /// Deleta a categoria permanentemente. Falha se houver serviços associados.
    /// </summary>
    private static async Task<IResult> DeleteAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new DeleteServiceCategoryCommand(id);
        var result = await commandDispatcher.SendAsync<DeleteServiceCategoryCommand, Result>(command, cancellationToken);
        return HandleNoContent(result);
    }
}
