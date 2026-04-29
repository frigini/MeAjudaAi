using System.Text.Json;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Streaming;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints;

/// <summary>
/// Endpoint para streaming SSE de eventos de verificação de um prestador específico.
/// </summary>
public class ProviderVerificationEventsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:guid}/verification-events", async (
            Guid id,
            HttpContext context,
            ISseHub<ProviderVerificationSseDto> sseHub,
            CancellationToken ct) =>
        {
            context.Response.Headers.Append("Content-Type", "text/event-stream");
            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");

            var topic = SseTopic.ForProviderVerification(id);
            var stream = sseHub.SubscribeAsync(topic, ct);

            await foreach (var @event in stream.WithCancellation(ct))
            {
                var json = JsonSerializer.Serialize(@event, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                
                await context.Response.WriteAsync($"data: {json}\n\n", ct);
                await context.Response.Body.FlushAsync(ct);
            }
        })
        .WithName("GetProviderVerificationEvents")
        .WithTags("Providers")
        .RequireAuthorization(); // Idealmente admin ou o próprio prestador
    }
}
