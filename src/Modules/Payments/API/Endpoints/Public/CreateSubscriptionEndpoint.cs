using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Payments.API.Endpoints.Public;

public class CreateSubscriptionEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("subscriptions", async (
            [FromBody] CreateSubscriptionRequest request,
            [FromServices] ICommandDispatcher dispatcher,
            [FromServices] IQueryDispatcher queryDispatcher,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (request.ProviderId == Guid.Empty)
            {
                return Results.BadRequest(new { error = "ProviderId inválido." });
            }

            if (string.IsNullOrWhiteSpace(request.PlanId))
            {
                return Results.BadRequest(new { error = "PlanId inválido." });
            }

            // Validação de Propriedade (Ownership Check)
            var userProviderIdClaim = httpContext.User?.FindFirst(AuthConstants.Claims.ProviderId)?.Value;
            if (string.IsNullOrEmpty(userProviderIdClaim))
            {
                return Results.Unauthorized();
            }

            if (!Guid.TryParse(userProviderIdClaim, out var userProviderId) || userProviderId != request.ProviderId)
            {
                // Se o usuário não for o próprio prestador, verificamos se é admin
                var isSystemAdmin = httpContext.User?.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value == "true";
                if (!isSystemAdmin)
                {
                    return Results.Forbid();
                }
            }

            // Valida se o prestador existe no banco
            var providerResult = await queryDispatcher.QueryAsync<GetProviderByIdQuery, MeAjudaAi.Contracts.Functional.Result<MeAjudaAi.Modules.Providers.Application.DTOs.ProviderDto?>>(new GetProviderByIdQuery(request.ProviderId), cancellationToken);
            if (providerResult.IsFailure || providerResult.Value == null)
            {
                return Results.NotFound(new { error = "Prestador não encontrado." });
            }

            var command = new CreateSubscriptionCommand(request.ProviderId, request.PlanId);

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
