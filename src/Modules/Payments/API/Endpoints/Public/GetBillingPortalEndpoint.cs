using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Payments.API.Endpoints.Public;

public class GetBillingPortalEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("subscriptions/billing-portal", async (
            [FromBody] GetBillingPortalRequest request,
            [FromServices] ICommandDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            if (request.ProviderId == Guid.Empty)
                return Results.BadRequest(new { error = "ProviderId is required." });

            var command = new GetBillingPortalCommand(request.ProviderId, request.ReturnUrl);
            var portalUrl = await dispatcher.SendAsync<GetBillingPortalCommand, string>(command, cancellationToken);
            
            return Results.Ok(new { portalUrl });
        })
        .WithTags(PaymentsEndpoints.Tag)
        .WithName("GetBillingPortal")
        .WithSummary("Gera um link para o portal de gerenciamento de faturamento do Stripe.");
    }
}

public record GetBillingPortalRequest(Guid ProviderId, string ReturnUrl);
