namespace MeAjudaAi.Contracts.Utilities.Constants;

/// <summary>
/// Chaves padrão para templates de comunicação.
/// Cada chave corresponde a um template armazenado na tabela email_templates.
/// </summary>
public static class CommunicationTemplateKeys
{
    // --- User ---
    public const string UserRegistered = "user_registered";
    public const string UserProfileUpdated = "user_profile_updated";
    public const string UserDeleted = "user_deleted";

    // --- Provider ---
    public const string ProviderRegistered = "provider_registered";
    public const string ProviderActivated = "provider_activated";
    public const string ProviderDeleted = "provider_deleted";
    public const string ProviderAwaitingVerification = "provider_awaiting_verification";

    // --- Provider Verification ---
    public const string ProviderVerificationApproved = "provider_verification_approved";
    public const string ProviderVerificationRejected = "provider_verification_rejected";
    public const string ProviderVerificationPending = "provider_verification_pending";
    public const string ProviderVerificationStatusUpdate = "provider_verification_status_update";
    public const string ProviderVerificationStatusUpdated = "provider_verification_status_updated";

    // --- Document ---
    public const string DocumentVerified = "document_verified";
    public const string DocumentRejected = "document_rejected";

    // --- Booking ---
    public const string BookingCreated = "booking_created";
    public const string BookingConfirmed = "booking_confirmed";
    public const string BookingCancelled = "booking_cancelled";
    public const string BookingRejected = "booking_rejected";
    public const string BookingCompleted = "booking_completed";

    // --- Review ---
    public const string ReviewApproved = "review_approved";

    // --- Subscription ---
    public const string SubscriptionActivated = "subscription_activated";
    public const string SubscriptionCanceled = "subscription_canceled";
    public const string SubscriptionExpired = "subscription_expired";
    public const string SubscriptionRenewed = "subscription_renewed";
}
