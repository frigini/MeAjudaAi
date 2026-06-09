using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.Application.DTOs.Requests;

[ExcludeFromCodeCoverage]
public record CreateBookingRequest(
    Guid ProviderId,
    Guid ServiceId,
    DateTimeOffset Start,
    DateTimeOffset End);
