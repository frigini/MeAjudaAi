using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderServices;

/// <summary>
/// Endpoint para remover um serviço do catálogo de um provider.
/// </summary>
public class RemoveServiceFromProviderEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/{providerId:guid}/services/{serviceId:guid}", RemoveServiceAsync)
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
        ISearchProvidersModuleApi searchProvidersApi,
        ILogger<RemoveServiceFromProviderEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var command = new RemoveServiceFromProviderCommand(providerId, serviceId);
        var result = await commandDispatcher.SendAsync<RemoveServiceFromProviderCommand, Result>(command, cancellationToken);

        if (result.IsFailure)
            return Handle(result);

        logger.LogInformation(
            "Service {ServiceId} removed from provider {ProviderId}, starting synchronous reindexing",
            serviceId, providerId);

        // Reindexar provider no módulo de busca de forma síncrona
        // O comando já executou SaveChangesAsync no repositório, então a transação está commitada
        // Isso garante que buscas subsequentes refletem a remoção do serviço
        var indexResult = await searchProvidersApi.IndexProviderAsync(providerId, cancellationToken);
        
        if (indexResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to reindex provider {ProviderId} after removing service {ServiceId}: {Error}",
                providerId, serviceId, indexResult.Error.Message);
            // Não falhamos a requisição porque o serviço foi removido com sucesso
            // O evento assíncrono vai tentar reindexar novamente
        }
        else
        {
            logger.LogInformation(
                "Successfully reindexed provider {ProviderId} after removing service {ServiceId}",
                providerId, serviceId);
        }

        return Results.NoContent();
    }
}
