using MeAjudaAi.Modules.Payments.API.Endpoints.Public;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Payments.API.Endpoints;

public static class PaymentsEndpoints
{
    public const string Tag = "Payments";

    public static void Map(IEndpointRouteBuilder app)
    {
        var group = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.Payments.Base, Tag);

        group.MapEndpoint<CreateSubscriptionEndpoint>()
             .MapEndpoint<GetBillingPortalEndpoint>();

        // Webhooks do Stripe devem ficar fora do grupo versionado para garantir estabilidade da URL
        var webhookGroup = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.Payments.Base + "/webhooks", "Webhooks");
        webhookGroup.MapEndpoint<StripeWebhookEndpoint>();
    }
}
