using MeAjudaAi.Modules.Payments.API.Endpoints.Public;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Payments.API.Endpoints;

public static class PaymentsEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = BaseEndpoint.CreateVersionedGroup(app, "payments", "Payments");

        group.MapEndpoint<CreateSubscriptionEndpoint>()
             .MapEndpoint<StripeWebhookEndpoint>();
    }
}
