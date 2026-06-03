using MeAjudaAi.Contracts.Modules.Bookings.Enums;

namespace MeAjudaAi.Contracts.Modules.Bookings.DTOs;

public record BookingDto(
    Guid Id,
    Guid ProviderId,
    Guid ClientId,
    Guid ServiceId,
    DateTimeOffset Start,
    DateTimeOffset End,
    EBookingStatus Status,
    string? RejectionReason = null,
    string? CancellationReason = null);
