using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.EntityFrameworkCore;
using Stripe;

using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.API.Endpoints.Public;

public class StripeWebhookEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("webhooks/stripe", async (
            HttpContext context,
            [FromServices] IConfiguration configuration,
            [FromServices] IHostEnvironment environment,
            [FromServices] PaymentsDbContext dbContext,
            [FromServices] ILogger<StripeWebhookEndpoint> logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                string json;
                using (var reader = new System.IO.StreamReader(
                    context.Request.Body, 
                    encoding: System.Text.Encoding.UTF8, 
                    detectEncodingFromByteOrderMarks: true, 
                    bufferSize: 1024, 
                    leaveOpen: true))
                {
                    json = await reader.ReadToEndAsync();
                }

                var stripeSignature = context.Request.Headers["Stripe-Signature"];
                var webhookSecret = configuration["Stripe:WebhookSecret"];

                if (string.IsNullOrEmpty(json))
                {
                    return Results.BadRequest(new { error = "Corpo da requisição vazio" });
                }

                // No ambiente Testing, podemos ignorar a validação de assinatura
                if (environment.IsEnvironment("Testing") && string.IsNullOrEmpty(stripeSignature))
                {
                    // throwOnApiVersionMismatch: false para evitar erros em testes com payloads manuais
                    var mockEvent = EventUtility.ParseEvent(json, throwOnApiVersionMismatch: false);
                    if (mockEvent == null) return Results.BadRequest(new { error = "Falha ao processar evento mock" });
                    
                    await SaveToInbox(mockEvent.Type, json, mockEvent.Id, dbContext, cancellationToken);
                    return Results.Ok();
                }

                if (string.IsNullOrWhiteSpace(webhookSecret))
                {
                    logger.LogError("Stripe:WebhookSecret not configured.");
                    return Results.InternalServerError(new { error = "Erro de configuração do webhook: Stripe:WebhookSecret ausente." });
                }

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    webhookSecret,
                    throwOnApiVersionMismatch: false);

                await SaveToInbox(stripeEvent.Type, json, stripeEvent.Id, dbContext, cancellationToken);

                return Results.Ok();
            }
            catch (StripeException e)
            {
                logger.LogWarning(e, "Stripe signature validation failed.");
                return Results.BadRequest(new { error = "Requisição de webhook inválida" });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Internal error processing Stripe webhook.");
                return Results.InternalServerError(new { error = "Erro interno no servidor" });
            }
        })
        .AllowAnonymous()
        .WithTags(PaymentsEndpoints.Tag)
        .WithName("StripeWebhook")
        .WithSummary("Recebe webhooks do Stripe de forma assíncrona.");
    }

    private static async Task SaveToInbox(string type, string content, string externalEventId, PaymentsDbContext dbContext, CancellationToken ct)
    {
        if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

        try
        {
            // Idempotência: verifica se o evento já existe
            var exists = await dbContext.InboxMessages.AnyAsync(m => m.ExternalEventId == externalEventId, ct);
            if (exists) return;

            var inboxMessage = new InboxMessage(type, content, externalEventId);

            dbContext.InboxMessages.Add(inboxMessage);
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
            // Violação de chave única: evento já processado por outra requisição concorrente.
            // Tratamos como sucesso silencioso para idempotência.
            return;
        }
    }
}
