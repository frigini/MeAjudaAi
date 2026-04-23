namespace MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;

public record TimeSlotDto(TimeOnly Start, TimeOnly End);

public record AvailableSlotDto(DateTimeOffset Start, DateTimeOffset End);

public record AvailabilityDto(DayOfWeek DayOfWeek, IReadOnlyList<AvailableSlotDto> Slots);

public record ProviderScheduleDto(DayOfWeek DayOfWeek, IReadOnlyList<TimeSlotDto> Slots);
