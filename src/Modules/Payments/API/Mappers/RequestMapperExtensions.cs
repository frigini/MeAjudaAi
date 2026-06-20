using MeAjudaAi.Modules.Payments.Application.Commands;
using MeAjudaAi.Modules.Payments.Application.DTOs.Requests;

namespace MeAjudaAi.Modules.Payments.API.Mappers;

/// <summary>
/// Métodos de extensão para mapear DTOs para Commands do módulo Payments.
/// </summary>
public static class RequestMapperExtensions
{
    /// <summary>
    /// Mapeia CreateSubscriptionRequest para CreateSubscriptionCommand.
    /// </summary>
    public static CreateSubscriptionCommand ToCommand(this CreateSubscriptionRequest request, string? idempotencyKey = null)
    {
        return new CreateSubscriptionCommand(
            request.ProviderId,
            request.PlanId,
            idempotencyKey);
    }

    /// <summary>
    /// Mapeia GetBillingPortalRequest para CreateBillingPortalSessionCommand.
    /// </summary>
    public static CreateBillingPortalSessionCommand ToCommand(this GetBillingPortalRequest request, string finalReturnUrl)
    {
        return new CreateBillingPortalSessionCommand(
            request.ProviderId,
            finalReturnUrl);
    }
}
