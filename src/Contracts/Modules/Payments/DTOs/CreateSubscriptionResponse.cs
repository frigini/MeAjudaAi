namespace MeAjudaAi.Contracts.Modules.Payments.DTOs;

/// <summary>
/// Resposta da criação de assinatura com a URL de checkout.
/// </summary>
public sealed record CreateSubscriptionResponse(string CheckoutUrl);
