using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Billing
{
    public record PaymentProcessed(
        Guid PaymentId,
        Guid RequestId,
        decimal Amount,
        string Currency,
        DateTime ProcessedAt
    ) : IntegrationEvent("Billing");
}