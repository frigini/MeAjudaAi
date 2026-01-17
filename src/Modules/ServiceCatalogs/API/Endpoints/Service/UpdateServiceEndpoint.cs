using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

public class UpdateServiceEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/{id:guid}", UpdateAsync)
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
