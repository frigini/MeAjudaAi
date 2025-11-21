namespace MeAjudaAi.Shared.Constants;

/// <summary>
/// Constantes para nomes de feature flags.
/// Usado com Microsoft.FeatureManagement para controle dinâmico de funcionalidades.
/// </summary>
public static class FeatureFlags
{
    /// <summary>
    /// Feature flag para restrição geográfica (cidades piloto MVP).
    /// Quando habilitado, apenas cidades configuradas em AllowedCities podem acessar.
    /// </summary>
    public const string GeographicRestriction = "GeographicRestriction";

    /// <summary>
    /// Feature flag para notificações push via Firebase Cloud Messaging.
    /// Sprint 2-3 planejado.
    /// </summary>
    public const string PushNotifications = "PushNotifications";

    /// <summary>
    /// Feature flag para pagamentos via Stripe.
    /// Sprint 4-5 planejado.
    /// </summary>
    public const string StripePayments = "StripePayments";

    /// <summary>
    /// Feature flag para modo de manutenção (bloqueia todas as requisições).
    /// Útil para deploys ou manutenção programada.
    /// </summary>
    public const string MaintenanceMode = "MaintenanceMode";
}
