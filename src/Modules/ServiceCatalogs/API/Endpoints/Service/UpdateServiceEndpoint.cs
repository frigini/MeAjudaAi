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
/// Endpoint para atualizar um serviço existente.
/// Requer privilégios de administrador.
/// </summary>
public class UpdateServiceEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Mapeia o endpoint PUT /{id} para atualizar um serviço.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut(ApiEndpoints.ServiceCatalogs.Services.Update, UpdateAsync)
            .WithName("UpdateService")
            .WithSummary("Atualizar serviço")
            .WithDescription("""
                Atualiza as informações de um serviço existente.
                
                **Validações:**
                - ID não pode ser vazio
                - Serviço deve existir
                - Nome é obrigatório (máximo 150 caracteres)
                - Descrição opcional (máximo 1000 caracteres)
                - DisplayOrder deve ser >= 0
                
                **Nota:** Não altera a categoria do serviço. Use ChangeServiceCategory para isso.
                
                **Permissões:** Requer privilégios de administrador
                """)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAdmin();

    /// <summary>
    /// Atualiza os dados de um serviço existente.
    /// </summary>
    private static async Task<IResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateServiceRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceCommand(id, request.Name, request.Description, request.DisplayOrder);
        var result = await commandDispatcher.SendAsync<UpdateServiceCommand, Result>(command, cancellationToken);
        return HandleNoContent(result);
    }
}
