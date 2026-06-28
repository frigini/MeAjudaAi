using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.ServiceCategory;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.ServiceCategory;

/// <summary>
/// Endpoint para atualizar uma categoria de serviço existente.
/// Requer privilégios de administrador.
/// </summary>
public class UpdateServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Mapeia o endpoint PUT /{id} para atualizar uma categoria.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut(ApiEndpoints.ServiceCatalogs.Categories.Update, UpdateAsync)
            .WithName("UpdateServiceCategory")
            .WithSummary("Atualizar categoria de serviço")
            .WithDescription("""
                Atualiza as informações de uma categoria existente.
                
                **Validações:**
                - ID não pode ser vazio
                - Categoria deve existir
                - Nome é obrigatório (máximo 100 caracteres)
                - Descrição opcional (máximo 500 caracteres)
                - DisplayOrder deve ser >= 0
                
                **Nota:** Requer atualização completa (full-update pattern).
                Todos os campos devem ser fornecidos.
                
                **Permissões:** Requer privilégios de administrador
                """)
            .Produces<Result>(StatusCodes.Status204NoContent)
            .RequireAdmin();

    /// <summary>
    /// Atualiza os dados de uma categoria existente.
    /// </summary>
    private static async Task<IResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateServiceCategoryRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceCategoryCommand(id, request.Name, request.Description, request.DisplayOrder);
        var result = await commandDispatcher.SendAsync<UpdateServiceCategoryCommand, Result>(command, cancellationToken);
        return HandleNoContent(result);
    }
}
