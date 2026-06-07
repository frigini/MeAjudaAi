namespace MeAjudaAi.Modules.Providers.Domain.Entities;

public record ProcessedIntegrationEvent(string CorrelationId, DateTime ProcessedAt);
