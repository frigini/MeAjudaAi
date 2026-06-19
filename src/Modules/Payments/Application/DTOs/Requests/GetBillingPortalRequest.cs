namespace MeAjudaAi.Modules.Payments.Application.DTOs.Requests;

public record GetBillingPortalRequest(Guid ProviderId, string? ReturnUrl);
