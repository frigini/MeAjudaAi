using MeAjudaAi.Modules.Payments.API.Endpoints.Public;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
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
             .MapEndpoint<StripeWebhookEndpoint>();
    }
}
