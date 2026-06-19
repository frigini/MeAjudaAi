namespace MeAjudaAi.Modules.Payments.Application.DTOs.Requests;

public record CreateSubscriptionRequest(Guid ProviderId, string PlanId);
