namespace MeAjudaAi.Modules.Providers.Domain.Entities;

public sealed record ProcessedIntegrationEvent(string CorrelationId, DateTime ProcessedAt);
