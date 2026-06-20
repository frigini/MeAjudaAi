using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Payments.Application.Services;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Payments.API.Endpoints.Public;

[ExcludeFromCodeCoverage]
public class StripeWebhookEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Payments.StripeWebhook, HandleStripeWebhookAsync)
        .AllowAnonymous()
        .RequireRateLimiting("StripeWebhookPolicy")
        .WithMetadata(new RequestSizeLimitAttribute(256_000))
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithTags(PaymentsEndpoints.Tag)
        .WithName("StripeWebhook")
        .WithSummary("Recebe webhooks do Stripe")
        .WithDescription("Recebe e processa webhooks do Stripe de forma assíncrona.");
    }

    /// <summary>
    /// Processa um evento webhook do Stripe, validando a assinatura e armazenando a mensagem na inbox.
    /// </summary>
    /// <param name="context">Contexto HTTP com o corpo da requisição e headers.</param>
    /// <param name="configuration">Configuração da aplicação para o segredo do webhook.</param>
    /// <param name="environment">Ambiente atual (Testing ignora validação de assinatura).</param>
    /// <param name="paymentService">Serviço de pagamento para armazenar a mensagem na inbox.</param>
    /// <param name="logger">Logger do endpoint.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <returns>200 OK se processado com sucesso; 400 Bad Request para requisições inválidas.</returns>
    private static async Task<IResult> HandleStripeWebhookAsync(
        HttpContext context,
        [FromServices] IConfiguration configuration,
        [FromServices] IHostEnvironment environment,
        [FromServices] IPaymentCommandService paymentService,
        [FromServices] ILogger<StripeWebhookEndpoint> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            using var ms = new MemoryStream();
            await context.Request.Body.CopyToAsync(ms, context.RequestAborted);

            string json;
            using (var reader = new StreamReader(ms,
                encoding: System.Text.Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024,
                leaveOpen: true))
            {
                json = await reader.ReadToEndAsync(cancellationToken);
            }

            if (string.IsNullOrEmpty(json))
            {
                return Error.BadRequest("Corpo da requisição vazio.").ToProblem();
            }

            var stripeSignature = context.Request.Headers["Stripe-Signature"];
            var webhookSecret = configuration["Stripe:WebhookSecret"];

            if (environment.IsEnvironment("Testing") && string.IsNullOrEmpty(stripeSignature))
            {
                return await ProcessMockEventAsync(json, paymentService, cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(webhookSecret))
            {
                logger.LogError("Stripe:WebhookSecret not configured.");
                return Error.Internal("Configuração do webhook Stripe ausente.").ToProblem();
            }

            var stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                webhookSecret,
                throwOnApiVersionMismatch: false);

            await paymentService.SaveInboxMessageAsync(stripeEvent.Type, json, stripeEvent.Id, cancellationToken);

            return Results.Ok();
        }
        catch (StripeException e)
        {
            logger.LogWarning(e, "Stripe signature validation failed.");
            return Error.BadRequest("Requisição de webhook inválida.").ToProblem();
        }
        catch (OperationCanceledException)
        {
            return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Internal error processing Stripe webhook.");
            return Error.Internal("Erro interno no servidor.").ToProblem();
        }
    }

    /// <summary>
    /// Processa um evento mock no ambiente de Testing, sem validação de assinatura.
    /// </summary>
    /// <param name="json">JSON do evento Stripe.</param>
    /// <param name="paymentService">Serviço de pagamento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>200 OK se processado; 400 Bad Request se o evento for inválido.</returns>
    private static async Task<IResult> ProcessMockEventAsync(
        string json,
        IPaymentCommandService paymentService,
        CancellationToken cancellationToken)
    {
        var mockEvent = EventUtility.ParseEvent(json, throwOnApiVersionMismatch: false);
        if (mockEvent is null)
        {
            return Error.BadRequest("Falha ao processar evento mock.").ToProblem();
        }

        await paymentService.SaveInboxMessageAsync(mockEvent.Type, json, mockEvent.Id, cancellationToken);
        return Results.Ok();
    }
}
