namespace MeAjudaAi.Shared.Streaming;

/// <summary>
/// Utilitário para padronização de tópicos SSE.
/// </summary>
public static class SseTopic
{
    public static string ForBooking(Guid bookingId) => $"bookings:{bookingId}";
    public static string ForProviderVerification(Guid providerId) => $"providers:{providerId}:verification";
}
