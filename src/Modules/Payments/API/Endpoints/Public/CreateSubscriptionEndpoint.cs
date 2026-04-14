using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Shared.Commands;
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
            var command = new CreateSubscriptionCommand(
                request.ProviderId,
                request.PlanId,
                request.Amount,
                request.Currency ?? "BRL");

            var checkoutUrl = await dispatcher.SendAsync<CreateSubscriptionCommand, string>(command, cancellationToken);

            return Results.Ok(new { checkoutUrl });
        })
        .WithTags("Payments")
        .WithName("CreateSubscription")
        .WithSummary("Cria uma nova assinatura e retorna a URL do checkout.");
    }
}

public record CreateSubscriptionRequest(Guid ProviderId, string PlanId, decimal Amount, string? Currency);
