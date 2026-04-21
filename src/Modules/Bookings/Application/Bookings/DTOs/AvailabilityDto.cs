namespace MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;

public record TimeSlotDto(DateTime Start, DateTime End);

public record AvailabilityDto(DayOfWeek DayOfWeek, IEnumerable<TimeSlotDto> Slots);
