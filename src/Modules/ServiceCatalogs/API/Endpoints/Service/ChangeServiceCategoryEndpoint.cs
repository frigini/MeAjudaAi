using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

/// <summary>
/// Endpoint para mover um serviço para outra categoria.
/// Requer privilégios de administrador.
/// </summary>
public class ChangeServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Mapeia o endpoint POST /{id}/change-category para alterar a categoria de um serviço.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost(ApiEndpoints.ServiceCatalogs.Services.ChangeCategory, ChangeAsync)
            .WithName(ApiEndpoints.ServiceCatalogs.Services.Names.ChangeCategory)
            .WithSummary("Alterar categoria do serviço")
            .WithDescription("""
                Move um serviço para uma categoria diferente.
                
                **Validações:**
                - Serviço deve existir
                - Nova categoria deve existir e estar ativa
                - Nova categoria não pode ser a mesma que a atual
                
                **Casos de Uso:**
                - Reorganizar catálogo de serviços
                - Corrigir categorização incorreta
                - Adaptar estrutura de categorias
                
                **Permissões:** Requer privilégios de administrador
                """)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAdmin();

    /// <summary>
    /// Move o serviço para a categoria informada.
    /// </summary>
    private static async Task<IResult> ChangeAsync(
        Guid id,
        [FromBody] ChangeServiceCategoryRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new ChangeServiceCategoryCommand(id, request.NewCategoryId);
        var result = await commandDispatcher.SendAsync<ChangeServiceCategoryCommand, Result>(command, cancellationToken);
        return HandleNoContent(result);
    }
}
