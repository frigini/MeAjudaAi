namespace MeAjudaAi.Modules.Payments.Application.DTOs.Requests;

/// <summary>
/// DTO de requisição para criação de uma nova assinatura de pagamento.
/// </summary>
/// <param name="ProviderId">Identificador único do prestador que irá assinar o plano.</param>
/// <param name="PlanId">Identificador do plano de assinatura no Stripe.</param>
public record CreateSubscriptionRequest(Guid ProviderId, string PlanId);
