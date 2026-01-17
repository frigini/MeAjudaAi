using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.ServiceCategory;

public class DeactivateServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id:guid}/deactivate", DeactivateAsync)
            .WithName("DeactivateServiceCategory")
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
