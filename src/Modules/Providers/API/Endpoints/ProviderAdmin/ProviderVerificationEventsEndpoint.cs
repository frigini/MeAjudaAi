using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Streaming;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;

/// <summary>
/// Endpoint SSE (Server-Sent Events) para streaming de eventos de verificação de prestador em tempo real.
/// </summary>
/// <remarks>
/// Estabelece uma conexão SSE persistente para transmitir eventos de alteração no status de verificação
/// do prestador. Ideal para dashboards administrativos que precisam de updates em tempo real.
/// Requer autenticação e autorização via Self ou Admin.
/// </remarks>
[ExcludeFromCodeCoverage]
public class ProviderVerificationEventsEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint SSE de eventos de verificação.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint GET em "/{id:guid}/verification-events" com:
    /// - Autorização via RequireSelfOrAdmin()
    /// - Conexão SSE (text/event-stream)
    /// - Cache-Control desabilitado para manter conexão ativa
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{id:guid}/verification-events", GetVerificationEventsAsync)
            .WithName("GetProviderVerificationEvents")
            .WithTags("Providers")
            .WithSummary("Stream de eventos de verificação do prestador")
            .WithDescription("Estabelece conexão SSE para receber eventos de alteração no status de verificação do prestador em tempo real.")
            .RequireSelfOrAdmin()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task GetVerificationEventsAsync(
        Guid id,
        HttpContext context,
        ISseHub<ProviderVerificationSseDto> sseHub,
        ISerializer serializer,
        CancellationToken ct)
    {
        context.Response.ContentType = "text/event-stream";
        context.Response.Headers.CacheControl = "no-cache";
        await context.Response.Body.FlushAsync(ct);

        var topic = SseTopic.ForProviderVerification(id);
        var stream = sseHub.SubscribeAsync(topic, ct);

        try
        {
            await foreach (var @event in stream.WithCancellation(ct))
            {
                var json = serializer.Serialize(@event);

                await context.Response.WriteAsync($"data: {json}\n\n", ct);
                await context.Response.Body.FlushAsync(ct);

                await context.Response.WriteAsync(": ping\n\n", ct);
                await context.Response.Body.FlushAsync(ct);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}