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
        app.MapPost("stripe", async (
            HttpContext context,
            [FromServices] IConfiguration configuration,
            [FromServices] IHostEnvironment environment,
            [FromServices] PaymentsDbContext dbContext,
            [FromServices] ILogger<StripeWebhookEndpoint> logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Copiamos para MemoryStream para suportar CancellationToken na leitura e manter body aberto se necessário
                using var ms = new System.IO.MemoryStream();
                await context.Request.Body.CopyToAsync(ms, context.RequestAborted);
                ms.Position = 0;

                string json;
                using (var reader = new System.IO.StreamReader(ms, 
                    encoding: System.Text.Encoding.UTF8, 
                    detectEncodingFromByteOrderMarks: true, 
                    bufferSize: 1024, 
                    leaveOpen: true))
                {
                    json = await reader.ReadToEndAsync(cancellationToken);
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
                    var mockEvent = EventUtility.ParseEvent(json, throwOnApiVersionMismatch: false);
                    if (mockEvent == null) return Results.BadRequest(new { error = "Falha ao processar evento mock" });
                    
                    await SaveToInbox(mockEvent.Type, json, mockEvent.Id, dbContext, cancellationToken);
                    return Results.Ok();
                }

                if (string.IsNullOrWhiteSpace(webhookSecret))
                {
                    logger.LogError("Stripe:WebhookSecret not configured.");
                    return Results.InternalServerError(new { error = "Erro interno no servidor" });
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
            catch (OperationCanceledException)
            {
                // Requisição abortada pelo cliente ou host
                return Results.StatusCode(499); 
            }
            catch (Exception e)
            {
                logger.LogError(e, "Internal error processing Stripe webhook.");
                return Results.InternalServerError(new { error = "Erro interno no servidor" });
            }
        })
        .AllowAnonymous()
        .DisableRateLimiting()
        .RequireRateLimiting("StripeWebhookPolicy")
        .WithMetadata(new Microsoft.AspNetCore.Mvc.RequestSizeLimitAttribute(256_000))
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
        catch (DbUpdateException)
        {
            // Violação de chave única ou erro de persistência: verifica se o evento já existe (idempotência agnóstica)
            var exists = await dbContext.InboxMessages.AsNoTracking().AnyAsync(m => m.ExternalEventId == externalEventId, ct);
            if (exists) return;
            
            throw;
        }
    }
}
