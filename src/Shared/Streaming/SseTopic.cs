using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Streaming;

[ExcludeFromCodeCoverage]
public static class SseTopic
{
    public static string ForBooking(Guid bookingId) => $"bookings:{bookingId}";
    public static string ForProviderVerification(Guid providerId) => $"providers:{providerId}:verification";
}
