using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Payments.API.Endpoints.Public;

public class CreateSubscriptionEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("subscriptions", async (
            [FromBody] CreateSubscriptionRequest request,
            [FromServices] ICommandDispatcher dispatcher,
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

            var command = new CreateSubscriptionCommand(request.ProviderId, request.PlanId);

            var checkoutUrl = await dispatcher.SendAsync<CreateSubscriptionCommand, string>(command, cancellationToken);

            return Results.Ok(new { checkoutUrl });
        })
        .WithTags(PaymentsEndpoints.Tag)
        .WithName("CreateSubscription")
        .WithSummary("Cria uma nova assinatura e retorna a URL do checkout.");
    }
}

public record CreateSubscriptionRequest(Guid ProviderId, string PlanId);
