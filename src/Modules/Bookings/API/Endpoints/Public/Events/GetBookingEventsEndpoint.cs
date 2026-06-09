using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Streaming;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public.Events;

[ExcludeFromCodeCoverage]
public class GetBookingEventsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:guid}/events", GetBookingEventsAsync)
            .WithName("GetBookingEvents")
            .WithTags(BookingsEndpoints.Tag)
            .WithSummary("Obtém o stream de eventos")
            .WithDescription("Obtém o stream de eventos de um agendamento via Server-Sent Events (SSE).")
            .RequireAuthorization();
    }

    /// <summary>
    /// Obtém um stream de eventos (Server-Sent Events) relacionados a um agendamento.
    /// </summary>
    /// <param name="id">ID do agendamento.</param>
    /// <param name="context">Contexto da requisição HTTP para extração de identidade.</param>
    /// <param name="dispatcher">Disparador de queries.</param>
    /// <param name="sseHub">Hub para assinatura de eventos SSE.</param>
    /// <param name="serializer">Serializador de JSON configurado para API.</param>
    /// <param name="ct">Token de cancelamento.</param>
    private static async Task GetBookingEventsAsync(
        Guid id,
        HttpContext context,
        [FromServices] IQueryDispatcher dispatcher,
        [FromServices] ISseHub<BookingStatusSseDto> sseHub,
        [FromServices, FromKeyedServices("Api")] ISerializer serializer,
        CancellationToken ct)
    {
        var correlationIdHeader = context.Request.Headers[AuthConstants.Headers.CorrelationId].FirstOrDefault();
        var correlationId = Guid.TryParse(correlationIdHeader, out var cId) ? cId : Guid.NewGuid();

        var user = context.User;
        var userId = Guid.TryParse(user.FindFirst(AuthConstants.Claims.Subject)?.Value, out var uId) ? uId : (Guid?)null;
        var providerId = Guid.TryParse(user.FindFirst(AuthConstants.Claims.ProviderId)?.Value, out var pId) ? pId : (Guid?)null;
        var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);

        var query = new GetBookingByIdQuery(id, userId, providerId, isSystemAdmin, correlationId);
        var result = await dispatcher.QueryAsync<GetBookingByIdQuery, Result<BookingDto>>(query, ct);

        if (result.IsFailure)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{ \"error\": \"Acesso negado: não foi possível consultar o agendamento. Detalhes: {result.Error.Message}\" }}", ct);
            return;
        }

        context.Response.Headers.Append("Content-Type", "text/event-stream");
        context.Response.Headers.Append("Cache-Control", "no-cache");
        context.Response.Headers.Append("Connection", "keep-alive");

        var topic = SseTopic.ForBooking(id);
        var stream = sseHub.SubscribeAsync(topic, ct);

        await foreach (var @event in stream.WithCancellation(ct))
        {
            var json = serializer.Serialize(@event);
            
            await context.Response.WriteAsync($"data: {json}\n\n", ct);
            await context.Response.Body.FlushAsync(ct);
        }
    }
}
