namespace MeAjudaAi.Contracts.Modules.Bookings.DTOs;

public record AvailabilityDto(DayOfWeek DayOfWeek, IReadOnlyList<AvailableSlotDto> Slots);
