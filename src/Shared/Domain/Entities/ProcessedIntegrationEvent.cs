namespace MeAjudaAi.Shared.Domain.Entities;

/// <summary>
/// Representa um evento de integração processado para idempotência.
/// </summary>
public sealed record ProcessedIntegrationEvent(string CorrelationId, DateTime ProcessedAt);
