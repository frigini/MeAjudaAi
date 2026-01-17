using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.ServiceCategory;

public class ActivateServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id:guid}/activate", ActivateAsync)
            .WithName("ActivateServiceCategory")
            .WithSummary("Ativar categoria de serviço")
            .WithDescription("""
                Ativa uma categoria de serviços.
                
                **Efeitos:**
                - Categoria fica visível em listagens públicas
                - Permite criação de novos serviços nesta categoria
                - Serviços existentes na categoria voltam a ser acessíveis
                
                **Permissões:** Requer privilégios de administrador
                """)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAdmin();

    private static async Task<IResult> ActivateAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new ActivateServiceCategoryCommand(id);
        var result = await commandDispatcher.SendAsync<ActivateServiceCategoryCommand, Result>(command, cancellationToken);
        return HandleNoContent(result);
    }
}
