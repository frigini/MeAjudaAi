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
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            return await HandleRequest(request, dispatcher, configuration, httpContext, cancellationToken);
        })
        .WithTags(PaymentsEndpoints.Tag)
        .WithName("GetBillingPortal")
        .RequireAuthorization()
        .WithSummary("Gera um link para o portal de gerenciamento de faturamento do Stripe.");
    }

    private static async Task<IResult> HandleRequest(
        GetBillingPortalRequest request, 
        ICommandDispatcher dispatcher, 
        IConfiguration configuration, 
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (request.ProviderId == Guid.Empty)
            return Results.BadRequest(new { error = "ProviderId é obrigatório." });

        // Validação de Ownership: O usuário autenticado deve ser o dono do ProviderId
        var userProviderIdStr = httpContext.User?.FindFirst("provider_id")?.Value;
        if (string.IsNullOrEmpty(userProviderIdStr) || !Guid.TryParse(userProviderIdStr, out var userProviderId) || userProviderId != request.ProviderId)
        {
            return Results.Forbid();
        }

        var clientBaseUrl = configuration["ClientBaseUrl"];
        if (string.IsNullOrEmpty(clientBaseUrl))
        {
            throw new InvalidOperationException("Missing 'ClientBaseUrl' configuration; cannot resolve return URL.");
        }

        clientBaseUrl = clientBaseUrl.TrimEnd('/');

        var resolvedReturnUrl = request.ReturnUrl?.ToLowerInvariant() switch
        {
            "account" => $"{clientBaseUrl}/account",
            "billing" => $"{clientBaseUrl}/billing",
            _ => clientBaseUrl // Proteção contra Open Redirect: ignora valores desconhecidos
        };

        var command = new GetBillingPortalCommand(request.ProviderId, resolvedReturnUrl);
        var portalUrl = await dispatcher.SendAsync<GetBillingPortalCommand, string>(command, cancellationToken);
        
        return Results.Ok(new { portalUrl });
    }
}

public record GetBillingPortalRequest(Guid ProviderId, string? ReturnUrl);
