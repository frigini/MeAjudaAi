using System.Text.Json;
using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Streaming;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints;

/// <summary>
/// Endpoint para streaming SSE de eventos de uma reserva específica.
/// </summary>
public class BookingEventsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:guid}/events", async (
            Guid id,
            HttpContext context,
            ISseHub<BookingStatusSseDto> sseHub,
            CancellationToken ct) =>
        {
            context.Response.Headers.Append("Content-Type", "text/event-stream");
            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");

            var topic = SseTopic.ForBooking(id);
            var stream = sseHub.SubscribeAsync(topic, ct);

            // Log de início de conexão (útil para debug de conexões persistentes)
            // Note: O middleware de logging padrão pode não capturar o fluxo contínuo adequadamente
            
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
        .WithName("GetBookingEvents")
        .WithTags("Bookings")
        .RequireAuthorization();
    }
}
