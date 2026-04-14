using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MeAjudaAi.Shared.Endpoints;
using Stripe;

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
            CancellationToken cancellationToken) =>
        {
            try
            {
                var json = await new System.IO.StreamReader(context.Request.Body).ReadToEndAsync();
                var stripeSignature = context.Request.Headers["Stripe-Signature"];
                var webhookSecret = configuration["Stripe:WebhookSecret"];

                if (string.IsNullOrEmpty(json))
                {
                    return Results.BadRequest(new { error = "Empty body" });
                }

                // In Testing environment, we might want to bypass signature validation
                if (environment.IsEnvironment("Testing") && string.IsNullOrEmpty(stripeSignature))
                {
                    // throwOnApiVersionMismatch: false para evitar erros em testes com payloads manuais
                    var mockEvent = EventUtility.ParseEvent(json, throwOnApiVersionMismatch: false);
                    if (mockEvent == null) return Results.BadRequest(new { error = "Failed to parse mock event" });
                    
                    await SaveToInbox(mockEvent.Type, json, dbContext, cancellationToken);
                    return Results.Ok();
                }

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    webhookSecret,
                    throwOnApiVersionMismatch: false);

                await SaveToInbox(stripeEvent.Type, json, dbContext, cancellationToken);

                return Results.Ok();
            }
            catch (StripeException e)
            {
                return Results.BadRequest(new { error = e.Message });
            }
            catch (Exception e)
            {
                return Results.InternalServerError(new { error = e.Message, stack = e.StackTrace });
            }
        })
        .AllowAnonymous()
        .WithTags("Payments")
        .WithName("StripeWebhook")
        .WithSummary("Recebe webhooks do Stripe de forma assíncrona.");
    }

    private static async Task SaveToInbox(string type, string content, PaymentsDbContext dbContext, CancellationToken ct)
    {
        if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

        var inboxMessage = new InboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.InboxMessages.Add(inboxMessage);
        await dbContext.SaveChangesAsync(ct);
    }
}
