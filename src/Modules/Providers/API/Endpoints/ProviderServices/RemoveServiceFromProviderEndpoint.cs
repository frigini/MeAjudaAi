using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderServices;

/// <summary>
/// Endpoint para remover um serviço do catálogo de um provider.
/// </summary>
public class RemoveServiceFromProviderEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/api/v1/providers/{providerId:guid}/services/{serviceId:guid}", RemoveServiceAsync)
            .WithName("RemoveServiceFromProvider")
            .WithTags("Providers - Services")
            .WithSummary("Remove serviço do provider")
            .WithDescription("""
                ### Remove um serviço do catálogo do provider
                
                **Funcionalidades:**
                - ✅ Remove associação entre provider e serviço
                - ✅ Emite evento de domínio ProviderServiceRemovedDomainEvent
                - ✅ Valida que o provider oferece o serviço antes de remover
                
                **Campos obrigatórios:**
                - providerId: ID do provider (UUID)
                - serviceId: ID do serviço do catálogo (UUID)
                
                **Validações:**
                - Provider deve existir
                - Provider deve oferecer o serviço
                - Provider não pode estar deletado
                """)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("SelfOrAdmin");

    /// <summary>
    /// Processa requisição de remoção de serviço do provider.
    /// </summary>
    private static async Task<IResult> RemoveServiceAsync(
        [FromRoute] Guid providerId,
        [FromRoute] Guid serviceId,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new RemoveServiceFromProviderCommand(providerId, serviceId);
        var result = await commandDispatcher.SendAsync<RemoveServiceFromProviderCommand, Result>(command, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Handle(result);
    }
}
