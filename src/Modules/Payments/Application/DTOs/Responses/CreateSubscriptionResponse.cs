namespace MeAjudaAi.Modules.Payments.Application.DTOs.Responses;

/// <summary>
/// Resposta da criação de assinatura com a URL de checkout.
/// </summary>
public record CreateSubscriptionResponse(string CheckoutUrl);
