using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.Configuration;

namespace MeAjudaAi.Modules.Payments.API.Endpoints.Public;

public class GetBillingPortalEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        // POST: /api/v1/payments/subscriptions/billing-portal
        app.MapPost("subscriptions/billing-portal", async (
            GetBillingPortalRequest request,
            ICommandDispatcher dispatcher,
            IConfiguration configuration,
            CancellationToken cancellationToken) =>
        {
            return await HandleRequest(request, dispatcher, configuration, cancellationToken);
        })
        .WithTags(PaymentsEndpoints.Tag)
        .WithName("GetBillingPortal")
        .WithSummary("Gera um link para o portal de gerenciamento de faturamento do Stripe.");

        // GET: /api/v1/payments/subscriptions/billing-portal?providerId=...&returnUrl=...
        // Suporte para compatibilidade com chamadas GET do frontend
        app.MapGet("subscriptions/billing-portal", async (
            [FromQuery] Guid providerId,
            [FromQuery] string? returnUrl,
            ICommandDispatcher dispatcher,
            IConfiguration configuration,
            CancellationToken cancellationToken) =>
        {
            var request = new GetBillingPortalRequest(providerId, returnUrl ?? "");
            return await HandleRequest(request, dispatcher, configuration, cancellationToken);
        })
        .WithTags(PaymentsEndpoints.Tag)
        .ExcludeFromDescription();
    }

    private static async Task<IResult> HandleRequest(
        GetBillingPortalRequest request, 
        ICommandDispatcher dispatcher, 
        IConfiguration configuration, 
        CancellationToken cancellationToken)
    {
        if (request.ProviderId == Guid.Empty)
            return Results.BadRequest(new { error = "ProviderId é obrigatório." });

        var clientBaseUrl = configuration["ClientBaseUrl"] ?? "http://localhost:5165";
        
        var resolvedReturnUrl = request.ReturnUrl?.ToLowerInvariant() switch
        {
            "account" => $"{clientBaseUrl}/account",
            "billing" => $"{clientBaseUrl}/billing",
            _ => clientBaseUrl
        };

        var command = new GetBillingPortalCommand(request.ProviderId, resolvedReturnUrl);
        var portalUrl = await dispatcher.SendAsync<GetBillingPortalCommand, string>(command, cancellationToken);
        
        return Results.Ok(new { portalUrl });
    }
}

public record GetBillingPortalRequest(Guid ProviderId, string ReturnUrl);
