using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Utilities.Constants;

/// <summary>
/// Constantes referentes a integração com o Stripe.
/// Centraliza strings usadas em eventos, metadata e configurações do gateway.
/// </summary>
[ExcludeFromCodeCoverage]
public static class StripeConstants
{
    /// <summary>Tipo do evento Stripe disparado quando uma sessão de checkout é concluída.</summary>
    public const string CheckoutSessionCompleted = "checkout.session.completed";

    /// <summary>Tipo do evento Stripe disparado quando uma fatura é paga com sucesso.</summary>
    public const string InvoicePaid = "invoice.paid";

    /// <summary>Tipo do evento Stripe disparado quando uma assinatura é deletada pelo cliente.</summary>
    public const string CustomerSubscriptionDeleted = "customer.subscription.deleted";

    /// <summary>Chave de metadata usada para associar uma assinatura Stripe ao provider do domínio.</summary>
    public const string ProviderIdMetadataKey = "provider_id";

    /// <summary>Status de uma assinatura Stripe que foi cancelada.</summary>
    public const string SubscriptionStatusCanceled = "canceled";

    /// <summary>Tipo de método de pagamento aceito no checkout (cartão de crédito/débito).</summary>
    public const string PaymentMethodCard = "card";
}
