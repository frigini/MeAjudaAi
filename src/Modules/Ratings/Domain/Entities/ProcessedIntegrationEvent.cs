namespace MeAjudaAi.Modules.Ratings.Domain.Entities;

public sealed record ProcessedIntegrationEvent(string CorrelationId, DateTime ProcessedAt);
