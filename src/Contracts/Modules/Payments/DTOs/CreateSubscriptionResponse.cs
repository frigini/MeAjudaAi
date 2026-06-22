namespace MeAjudaAi.Contracts.Modules.Payments.DTOs;

/// <summary>
/// Resposta da criação de assinatura com a URL de checkout.
/// </summary>
public record CreateSubscriptionResponse(string CheckoutUrl);
