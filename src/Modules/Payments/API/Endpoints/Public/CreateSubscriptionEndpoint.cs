using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Payments.DTOs;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Payments.API.Helpers;
using MeAjudaAi.Modules.Payments.API.Mappers;
using MeAjudaAi.Modules.Payments.Application.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Payments.API.Endpoints.Public;

[ExcludeFromCodeCoverage]
public class CreateSubscriptionEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Payments.CreateSubscription, CreateSubscriptionAsync)
        .RequireAuthorization()
        .Produces<CreateSubscriptionResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status502BadGateway)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithTags(PaymentsEndpoints.Tag)
        .WithName("CreateSubscription")
        .WithSummary("Cria uma nova assinatura")
        .WithDescription("Cria uma nova assinatura e retorna a URL do checkout do Stripe.");
    }

    /// <summary>
    /// Cria uma nova assinatura para o prestador e retorna a URL de checkout do Stripe.
    /// </summary>
    /// <param name="request">Dados da assinatura (prestador e plano).</param>
    /// <param name="dispatcher">Disparador de comandos.</param>
    /// <param name="providersApi">API do módulo de prestadores para validação cruzada.</param>
    /// <param name="logger">Logger do endpoint.</param>
    /// <param name="httpContext">Contexto da requisição HTTP para extração de identidade e headers.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <returns>URL de checkout do Stripe.</returns>
    private static async Task<IResult> CreateSubscriptionAsync(
        CreateSubscriptionRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        [FromServices] IProvidersModuleApi providersApi,
        [FromServices] ILogger<CreateSubscriptionEndpoint> logger,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (request.ProviderId == Guid.Empty || string.IsNullOrWhiteSpace(request.PlanId))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["ProviderId"] = request.ProviderId == Guid.Empty ? ["O campo ProviderId é obrigatório."] : [],
                ["PlanId"] = string.IsNullOrWhiteSpace(request.PlanId) ? ["O campo PlanId é obrigatório."] : []
            });
        }

        var authResult = PaymentAuthorizationHelper.AuthorizeProviderAccess(httpContext, request.ProviderId);
        if (authResult is not null)
        {
            return authResult;
        }

        var existsResult = await providersApi.ProviderExistsAsync(request.ProviderId, cancellationToken);

        if (existsResult.IsFailure)
        {
            logger.LogError("Error checking provider existence for {ProviderId}: {Error}",
                request.ProviderId, existsResult.Error);
            return new Error("Erro ao validar prestador. Tente novamente mais tarde.", StatusCodes.Status502BadGateway).ToProblem();
        }

        if (!existsResult.Value)
        {
            return Error.NotFound("Prestador não encontrado.").ToProblem();
        }

        string? idempotencyKey = null;
        if (httpContext.Request.Headers.TryGetValue("Idempotency-Key", out var v))
        {
            var headerValue = v.ToString();
            idempotencyKey = string.IsNullOrWhiteSpace(headerValue) || headerValue.Length > 255
                ? null
                : headerValue;
        }

        var command = request.ToCommand(idempotencyKey);
        var checkoutUrl = await dispatcher.SendAsync<CreateSubscriptionCommand, string>(command, cancellationToken);

        return Results.Ok(new CreateSubscriptionResponse(checkoutUrl));
    }
}
