namespace MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs;

public record StripeEventData(
    string Type,
    string? ExternalEventId,
    string? SubscriptionId,
    string? CustomerId,
    Guid? ProviderId,
    DateTime? PeriodEnd = null,
    long AmountPaid = 0,
    string? Currency = null,
    string? InvoiceId = null);
