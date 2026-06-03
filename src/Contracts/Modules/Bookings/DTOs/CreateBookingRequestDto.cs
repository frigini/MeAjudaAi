namespace MeAjudaAi.Contracts.Modules.Bookings.DTOs;

public record CreateBookingRequestDto(
    Guid ProviderId,
    Guid ServiceId,
    DateTimeOffset Start,
    DateTimeOffset End);
