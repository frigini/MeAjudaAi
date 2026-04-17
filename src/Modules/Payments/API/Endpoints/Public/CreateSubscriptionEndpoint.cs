using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Payments.API.Endpoints.Public;

public class CreateSubscriptionEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("subscriptions", async (
            CreateSubscriptionRequest request,
            [FromServices] ICommandDispatcher dispatcher,
            [FromServices] IProvidersModuleApi providersApi,
            [FromServices] ILogger<CreateSubscriptionEndpoint> logger,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (request.ProviderId == Guid.Empty)
            {
                return Results.BadRequest(new { error = "ProviderId é obrigatório." });
            }

            if (string.IsNullOrWhiteSpace(request.PlanId))
            {
                return Results.BadRequest(new { error = "PlanId é obrigatório." });
            }

            // Validação de Autorização (Admin ou Dono do Provider)
            var isSystemAdmin = string.Equals(
                httpContext.User?.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, 
                "true", 
                StringComparison.OrdinalIgnoreCase);

            if (!isSystemAdmin)
            {
                var userProviderIdClaim = httpContext.User?.FindFirst(AuthConstants.Claims.ProviderId)?.Value;
                if (string.IsNullOrEmpty(userProviderIdClaim) || !Guid.TryParse(userProviderIdClaim, out var userProviderId) || userProviderId != request.ProviderId)
                {
                    return string.IsNullOrEmpty(userProviderIdClaim) ? Results.Unauthorized() : Results.Forbid();
                }
            }

            // Valida se o prestador existe no banco via API do módulo
            var existsResult = await providersApi.ProviderExistsAsync(request.ProviderId, cancellationToken);
            
            if (existsResult.IsFailure)
            {
                logger.LogError("Error checking provider existence for {ProviderId}: {Error}", 
                    request.ProviderId, existsResult.Error);
                return Results.Problem("Erro ao validar prestador. Tente novamente mais tarde.", statusCode: 502);
            }

            if (!existsResult.Value)
            {
                return Results.NotFound(new { error = "Prestador não encontrado." });
            }

            // Repassar Idempotency-Key se presente
            var idempotencyKey = httpContext.Request.Headers["Idempotency-Key"].ToString();
            
            var command = new CreateSubscriptionCommand(request.ProviderId, request.PlanId, idempotencyKey);
            var checkoutUrl = await dispatcher.SendAsync<CreateSubscriptionCommand, string>(command, cancellationToken);
            
            return Results.Ok(new { checkoutUrl });
        })
        .RequireAuthorization()
        .WithTags(PaymentsEndpoints.Tag)
        .WithName("CreateSubscription")
        .WithSummary("Cria uma nova assinatura e retorna a URL do checkout.");
    }
}

public record CreateSubscriptionRequest(Guid ProviderId, string PlanId);
