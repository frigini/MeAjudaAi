using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.API.Endpoints.Public;

public class GetBillingPortalEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        // POST: /api/v1/payments/subscriptions/billing-portal
        app.MapPost("subscriptions/billing-portal", async (
            GetBillingPortalRequest request,
            [FromServices] ICommandDispatcher dispatcher,
            [FromServices] IConfiguration configuration,
            [FromServices] ILogger<GetBillingPortalEndpoint> logger,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            return await HandleRequest(request, dispatcher, configuration, logger, httpContext, cancellationToken);
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
        ILogger<GetBillingPortalEndpoint> logger,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (request.ProviderId == Guid.Empty)
            return Results.BadRequest(new { error = "ProviderId é obrigatório." });

        // Validação de Autorização (Admin ou Dono do Provider)
        var isSystemAdmin = string.Equals(
            httpContext.User?.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, 
            "true", 
            StringComparison.OrdinalIgnoreCase);

        if (!isSystemAdmin)
        {
            var userProviderIdClaim = httpContext.User?.FindFirst(AuthConstants.Claims.ProviderId)?.Value;
            if (string.IsNullOrEmpty(userProviderIdClaim) || !Guid.TryParse(userProviderIdClaim, out var userProviderId) || userProviderId != request.ProviderId)
            {
                return string.IsNullOrEmpty(userProviderIdClaim) ? Results.Unauthorized() : Results.Forbid();
            }
        }

        var clientBaseUrl = configuration["ClientBaseUrl"];
        if (string.IsNullOrEmpty(clientBaseUrl))
        {
            logger.LogError("ClientBaseUrl configuration missing");
            return Results.Problem("ClientBaseUrl não configurada.");
        }

        clientBaseUrl = clientBaseUrl.TrimEnd('/');

        // Se ReturnUrl é um caminho conhecido, resolver. Se for uma URL completa, passar para o handler validar
        string finalReturnUrl;
        var returnUrl = request.ReturnUrl ?? "";

        if (returnUrl.Equals("account", StringComparison.OrdinalIgnoreCase))
            finalReturnUrl = $"{clientBaseUrl}/account";
        else if (returnUrl.Equals("billing", StringComparison.OrdinalIgnoreCase))
            finalReturnUrl = $"{clientBaseUrl}/billing";
        else if (Uri.TryCreate(returnUrl, UriKind.Absolute, out _))
            finalReturnUrl = returnUrl; // Passar URL completa para o handler validar
        else
        {
            logger.LogInformation("Billing portal ReturnUrl fallback taken for Provider {ProviderId}. Original value: {ReturnUrl}", 
                request.ProviderId, returnUrl);
            finalReturnUrl = clientBaseUrl; // Fallback para URL inválida/não reconhecida
        }
        var command = new GetBillingPortalCommand(request.ProviderId, finalReturnUrl);
        var portalUrl = await dispatcher.SendAsync<GetBillingPortalCommand, string>(command, cancellationToken);

        return Results.Ok(new { portalUrl });
    }
}

public record GetBillingPortalRequest(Guid ProviderId, string? ReturnUrl);
