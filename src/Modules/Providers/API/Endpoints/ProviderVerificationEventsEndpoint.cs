using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Streaming;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints;

[ExcludeFromCodeCoverage]
public class ProviderVerificationEventsEndpoint : IEndpoint
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:guid}/verification-events", async (
            Guid id,
            HttpContext context,
            ISseHub<ProviderVerificationSseDto> sseHub,
            CancellationToken ct) =>
        {
            var user = context.User;
            var userId = Guid.TryParse(user.FindFirst(AuthConstants.Claims.Subject)?.Value, out var uId) ? uId : (Guid?)null;
            var providerId = Guid.TryParse(user.FindFirst(AuthConstants.Claims.ProviderId)?.Value, out var pId) ? pId : (Guid?)null;
            var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);

            if (!isSystemAdmin && providerId != id)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            context.Response.Headers.Append("Content-Type", "text/event-stream");
            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");

            var topic = SseTopic.ForProviderVerification(id);
            var stream = sseHub.SubscribeAsync(topic, ct);

            await foreach (var @event in stream.WithCancellation(ct))
            {
                var json = JsonSerializer.Serialize(@event, CamelCaseOptions);
                
                await context.Response.WriteAsync($"data: {json}\n\n", ct);
                await context.Response.Body.FlushAsync(ct);
            }
        })
        .WithName("GetProviderVerificationEvents")
        .WithTags("Providers")
        .RequireAuthorization();
    }
}
