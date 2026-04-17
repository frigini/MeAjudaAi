using MeAjudaAi.Modules.Payments.API.Endpoints.Public;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Payments.API.Endpoints;

public static class PaymentsEndpoints
{
    public const string Route = "payments";
    public const string Tag = "Payments";

    public static void Map(IEndpointRouteBuilder app)
    {
        var group = BaseEndpoint.CreateVersionedGroup(app, Route, Tag);

        group.MapEndpoint<CreateSubscriptionEndpoint>()
             .MapEndpoint<GetBillingPortalEndpoint>();

        // Webhooks do Stripe devem ficar fora do grupo versionado para garantir estabilidade da URL
        var webhookGroup = app.MapGroup("/api/payments/webhooks")
                              .WithTags("Webhooks");
        webhookGroup.MapEndpoint<StripeWebhookEndpoint>();
    }
}
