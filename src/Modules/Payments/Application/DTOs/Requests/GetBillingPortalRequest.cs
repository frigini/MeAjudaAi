namespace MeAjudaAi.Modules.Payments.Application.DTOs.Requests;

/// <summary>
/// DTO de requisição para geração de link do portal de faturamento Stripe.
/// </summary>
/// <param name="ProviderId">Identificador único do prestador cujo portal será acessado.</param>
/// <param name="ReturnUrl">URL de retorno opcional para redirecionamento após acesso ao portal.</param>
public record GetBillingPortalRequest(Guid ProviderId, string? ReturnUrl);
