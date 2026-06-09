namespace MeAjudaAi.Modules.Bookings.Application.DTOs;

public record AvailabilityDto(DayOfWeek DayOfWeek, IReadOnlyList<AvailableSlotDto> Slots);

