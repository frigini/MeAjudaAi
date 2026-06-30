namespace MeAjudaAi.Contracts.Modules.Bookings.DTOs;

public sealed record AvailabilityDto(DayOfWeek DayOfWeek, IReadOnlyList<AvailableSlotDto> Slots);
