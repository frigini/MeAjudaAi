using System.Text.Json;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Streaming;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints;

public class BookingEventsEndpoint : IEndpoint
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:guid}/events", async (
            Guid id,
            HttpContext context,
            IQueryDispatcher dispatcher,
            ISseHub<BookingStatusSseDto> sseHub,
            CancellationToken ct) =>
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
                return;
            }

            context.Response.Headers.Append("Content-Type", "text/event-stream");
            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");

            var topic = SseTopic.ForBooking(id);
            var stream = sseHub.SubscribeAsync(topic, ct);

            await foreach (var @event in stream.WithCancellation(ct))
            {
                var json = JsonSerializer.Serialize(@event, CamelCaseOptions);
                
                await context.Response.WriteAsync($"data: {json}\n\n", ct);
                await context.Response.Body.FlushAsync(ct);
            }
        })
        .WithName("GetBookingEvents")
        .WithTags("Bookings")
        .RequireAuthorization();
    }
}
