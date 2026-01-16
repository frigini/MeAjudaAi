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
/// Endpoint para adicionar um serviço do catálogo a um provider.
/// </summary>
public class AddServiceToProviderEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{providerId:guid}/services/{serviceId:guid}", AddServiceAsync)
            .WithName("AddServiceToProvider")
            .WithTags("Providers - Services")
            .WithSummary("Adiciona serviço ao provider")
            .WithDescription("""
                ### Adiciona um serviço do catálogo ao provider
                
                **Funcionalidades:**
                - ✅ Valida existência e status do serviço via IServiceCatalogsModuleApi
                - ✅ Verifica se o serviço está ativo
                - ✅ Previne duplicação de serviços
                - ✅ Emite evento de domínio ProviderServiceAddedDomainEvent
                
                **Campos obrigatórios:**
                - providerId: ID do provider (UUID)
                - serviceId: ID do serviço do catálogo (UUID)
                
                **Validações:**
                - Serviço deve existir no catálogo
                - Serviço deve estar ativo
                - Provider não pode já oferecer o serviço
                - Provider não pode estar deletado
                """)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("SelfOrAdmin");

    /// <summary>
    /// Processa requisição de adição de serviço ao provider.
    /// </summary>
    private static async Task<IResult> AddServiceAsync(
        [FromRoute] Guid providerId,
        [FromRoute] Guid serviceId,
        ICommandDispatcher commandDispatcher,
        ISearchProvidersModuleApi searchProvidersApi,
        ILogger<AddServiceToProviderEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var command = new AddServiceToProviderCommand(providerId, serviceId);
        var result = await commandDispatcher.SendAsync<AddServiceToProviderCommand, Result>(command, cancellationToken);

        if (result.IsFailure)
            return Handle(result);

        logger.LogInformation(
            "Service {ServiceId} added to provider {ProviderId}, starting synchronous reindexing",
            serviceId, providerId);

        // Reindexar provider no módulo de busca de forma síncrona
        // O comando já executou SaveChangesAsync no repositório, então a transação está commitada
        // Isso garante que buscas subsequentes refletem a adição do serviço
        var indexResult = await searchProvidersApi.IndexProviderAsync(providerId, cancellationToken);
        
        if (indexResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to reindex provider {ProviderId} after adding service {ServiceId}: {Error}",
                providerId, serviceId, indexResult.Error.Message);
            // Não falhamos a requisição porque o serviço foi adicionado com sucesso
            // O evento assíncrono vai tentar reindexar novamente
        }
        else
        {
            logger.LogInformation(
                "Successfully reindexed provider {ProviderId} after adding service {ServiceId}",
                providerId, serviceId);
        }

        return Results.NoContent();
    }
}
