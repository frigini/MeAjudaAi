using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.ServiceCategory;

public class DeleteServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/{id:guid}", DeleteAsync)
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
