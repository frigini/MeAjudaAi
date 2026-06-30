using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Modules.Payments.Application.Services;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Modules.Payments.API.Endpoints.Public;

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
        .WithName(ApiEndpoints.Payments.Names.StripeWebhook)
        .WithSummary("Recebe webhooks do Stripe")
        .WithDescription("Recebe e processa webhooks do Stripe de forma assíncrona.");
    }

    private static async Task<IResult> HandleStripeWebhookAsync(
        HttpContext context,
        [FromServices] IPaymentCommandService paymentService,
        CancellationToken cancellationToken)
    {
        try
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var payload = await reader.ReadToEndAsync(cancellationToken);
            context.Request.Body.Position = 0;

            var stripeSignature = context.Request.Headers["Stripe-Signature"].ToString();

            var result = await paymentService.HandleStripeWebhookAsync(payload, stripeSignature, cancellationToken);

            if (result.IsFailure)
            {
                return result.Error.ToProblem();
            }

            return Results.Ok();
        }
        catch (OperationCanceledException)
        {
            return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
    }
}